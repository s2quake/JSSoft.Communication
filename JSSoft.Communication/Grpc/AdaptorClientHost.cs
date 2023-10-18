// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Grpc.Core;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication.Grpc;

class AdaptorClientHost : IAdaptorHost
{
    private readonly IServiceContext _serviceContext;
    private readonly IInstanceContext _instanceContext;
    private readonly IContainer<IServiceHost> _serviceHosts;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _task;
    private Channel _channel;
    private AdaptorClientImpl _adaptorImpl;
    private ISerializer _serializer;
    private PeerDescriptor _descriptor;

    public AdaptorClientHost(IServiceContext serviceContext, IInstanceContext instanceContext)
    {
        this._serviceContext = serviceContext;
        this._instanceContext = instanceContext;
        this._serviceHosts = serviceContext.ServiceHosts;
    }

    public async Task OpenAsync(string host, int port)
    {
        try
        {
            await Task.Run(() =>
            {
                this._channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                this._adaptorImpl = new AdaptorClientImpl(this._channel, Guid.NewGuid(), this._serviceHosts.ToArray());
            });
            await this._adaptorImpl.OpenAsync();
            this._descriptor = await this._instanceContext.CreateInstanceAsync(this._adaptorImpl);
            await Task.Run(() =>
            {
                this._cancellationTokenSource = new CancellationTokenSource();
                this._serializer = this._serviceContext.GetService(typeof(ISerializer)) as ISerializer;
            });
            this._task = this.PollAsync(this._cancellationTokenSource.Token);
        }
        catch
        {
            if (this._channel != null)
            {
                await this._channel.ShutdownAsync();
                this._channel = null;
            }
            throw;
        }
    }

    public async Task CloseAsync(int closeCode)
    {
        await Task.Run(() =>
        {
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource = null;
            this._task?.Wait();
            this._task = null;
        });
        if (this._adaptorImpl != null)
            await this._instanceContext.DestroyInstanceAsync(this._adaptorImpl);
        if (this._adaptorImpl != null)
            await this._adaptorImpl.CloseAsync();
        this._adaptorImpl = null;
        if (this._channel != null)
            await this._channel.ShutdownAsync();
        this._channel = null;
    }

    public event EventHandler<CloseEventArgs> Disconnected;

    protected virtual void OnDisconnected(CloseEventArgs e)
    {
        this.Disconnected?.Invoke(this, e);
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        var closeCode = int.MinValue;
        try
        {
            using var call = this._adaptorImpl.Poll();
            while (!cancellationToken.IsCancellationRequested)
            {
                var request = new PollRequest()
                {
                    Token = $"{this._adaptorImpl.Token}"
                };
                await call.RequestStream.WriteAsync(request);
                await call.ResponseStream.MoveNext();
                var reply = call.ResponseStream.Current;
                if (reply.Code != int.MinValue)
                {
                    closeCode = reply.Code;
                    break;
                }
                this.InvokeCallback(reply.Items);
                reply.Items.Clear();
                await Task.Delay(1);
            }
            await call.RequestStream.CompleteAsync();
            await call.ResponseStream.MoveNext();
        }
        catch (Exception e)
        {
            closeCode = -1;
            GrpcEnvironment.Logger.Error(e, e.Message);
        }
        if (closeCode != int.MinValue)
        {
            this._task = null;
            await this._adaptorImpl.AbortAsync();
            this._adaptorImpl = null;
            this.OnDisconnected(new CloseEventArgs(closeCode));
        }
    }

    private void InvokeCallback(IServiceHost serviceHost, string name, string[] datas)
    {
        if (serviceHost.MethodDescriptors.ContainsKey(name) == false)
            throw new InvalidOperationException();
        var methodDescriptor = serviceHost.MethodDescriptors[name];
        var args = this._serializer.DeserializeMany(methodDescriptor.ParameterTypes, datas);
        var instance = this._descriptor.Callbacks[serviceHost];
        Task.Run(() => methodDescriptor.InvokeAsync(this._serviceContext, instance, args));
    }

    private void InvokeCallback(IEnumerable<PollReplyItem> pollItems)
    {
        foreach (var item in pollItems)
        {
            var service = this._serviceHosts[item.ServiceName];
            this.InvokeCallback(service, item.Name, item.Datas.ToArray());
        }
    }

    private void ThrowException(Guid id, string data)
    {
        if (this._serviceContext.GetService(typeof(IComponentProvider)) is not IComponentProvider componentProvider)
        {
            throw new InvalidOperationException("can not get interface of IComponentProvider at serviceProvider");
        }
        var exceptionDescriptor = componentProvider.GetExceptionDescriptor(id);
        var exception = (Exception)this._serializer.Deserialize(exceptionDescriptor.ExceptionType, data);
        throw exception;
    }

    #region IAdaptorHost

    void IAdaptorHost.Invoke(InstanceBase instance, string name, Type[] types, object[] args)
    {
        var token = $"{this._adaptorImpl.Token}";
        var datas = this._serializer.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Datas.AddRange(datas);
        var reply = this._adaptorImpl.Invoke(request);
        var id = Guid.Parse(reply.ID);
        if (id != Guid.Empty)
        {
            this.ThrowException(id, reply.Data);
        }
    }

    T IAdaptorHost.Invoke<T>(InstanceBase instance, string name, Type[] types, object[] args)
    {
        var token = $"{this._adaptorImpl.Token}";
        var datas = this._serializer.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Datas.AddRange(datas);
        var reply = this._adaptorImpl.Invoke(request);
        var id = Guid.Parse(reply.ID);
        if (id != Guid.Empty)
        {
            this.ThrowException(id, reply.Data);
        }
        return (T)this._serializer.Deserialize(typeof(T), reply.Data);
    }

    async Task IAdaptorHost.InvokeAsync(InstanceBase instance, string name, Type[] types, object[] args)
    {
        var token = $"{this._adaptorImpl.Token}";
        var datas = this._serializer.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Datas.AddRange(datas);
        var reply = await this._adaptorImpl.InvokeAsync(request);
        var id = Guid.Parse(reply.ID);
        if (id != Guid.Empty)
        {
            this.ThrowException(id, reply.Data);
        }
    }

    async Task<T> IAdaptorHost.InvokeAsync<T>(InstanceBase instance, string name, Type[] types, object[] args)
    {
        var token = $"{this._adaptorImpl.Token}";
        var datas = this._serializer.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Datas.AddRange(datas);
        var reply = await this._adaptorImpl.InvokeAsync(request);
        var id = Guid.Parse(reply.ID);
        if (id != Guid.Empty)
        {
            this.ThrowException(id, reply.Data);
        }
        return (T)this._serializer.Deserialize(typeof(T), reply.Data);
    }

    #endregion
}
