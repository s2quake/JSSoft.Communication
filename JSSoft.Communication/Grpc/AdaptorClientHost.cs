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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication.Grpc;

class AdaptorClientHost : IAdaptorHost
{
    private readonly IServiceContext _serviceContext;
    private readonly IInstanceContext _instanceContext;
    private readonly IContainer<IServiceHost> _serviceHosts;
    private readonly Dictionary<IServiceHost, MethodDescriptorCollection> _methodsByServiceHost;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _task;
    private Channel? _channel;
    private AdaptorClientImpl? _adaptorImpl;
    private ISerializer? _serializer;
    private PeerDescriptor? _descriptor;

    public AdaptorClientHost(IServiceContext serviceContext, IInstanceContext instanceContext)
    {
        _serviceContext = serviceContext;
        _instanceContext = instanceContext;
        _serviceHosts = serviceContext.ServiceHosts;
        _methodsByServiceHost = _serviceHosts.ToDictionary(item => item, item => new MethodDescriptorCollection(item));
    }

    public async Task OpenAsync(string host, int port, CancellationToken cancellationToken)
    {
        if (_adaptorImpl != null)
            throw new InvalidOperationException();
        try
        {
            _channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            _adaptorImpl = new AdaptorClientImpl(_channel, Guid.NewGuid(), _serviceHosts.ToArray());
            await _adaptorImpl.OpenAsync(cancellationToken);
            _descriptor = _instanceContext.CreateInstance(_adaptorImpl);
            _cancellationTokenSource = new CancellationTokenSource();
            _serializer = (ISerializer)_serviceContext.GetService(typeof(ISerializer))!;
            _task = PollAsync(_cancellationTokenSource.Token);
        }
        catch
        {
            if (_channel != null)
            {
                await _channel.ShutdownAsync();
                _channel = null;
            }
            throw;
        }
    }

    public async Task CloseAsync(int closeCode, CancellationToken cancellationToken)
    {
        if (_adaptorImpl == null)
            throw new InvalidOperationException();

        _cancellationTokenSource?.Cancel();
        if (_task != null)
            await _task;
        if (_adaptorImpl != null)
            _instanceContext.DestroyInstance(_adaptorImpl);
        if (_adaptorImpl != null)
            await _adaptorImpl.CloseAsync(cancellationToken);
        _adaptorImpl = null;
        if (_channel != null)
            await _channel.ShutdownAsync();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _channel = null;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public event EventHandler<CloseEventArgs>? Disconnected;

    protected virtual void OnDisconnected(CloseEventArgs e)
    {
        Disconnected?.Invoke(this, e);
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        if (_adaptorImpl == null)
            throw new InvalidOperationException();

        var closeCode = int.MinValue;
        try
        {
            using var call = _adaptorImpl.Poll();
            while (!cancellationToken.IsCancellationRequested)
            {
                var request = new PollRequest()
                {
                    Token = $"{_adaptorImpl.Token}"
                };
                await call.RequestStream.WriteAsync(request);
                var s = await call.ResponseStream.MoveNext();
                var reply = call.ResponseStream.Current;
                if (reply.Code != int.MinValue)
                {
                    closeCode = reply.Code;
                    break;
                }
                InvokeCallback(reply.Items);
                reply.Items.Clear();
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
            _task = null;
            await _adaptorImpl.AbortAsync();
            _adaptorImpl = null;
            OnDisconnected(new CloseEventArgs(closeCode));
        }
    }

    private void InvokeCallback(IServiceHost serviceHost, string name, string[] data)
    {
        if (_adaptorImpl == null)
            throw new InvalidOperationException();
        var methodDescriptors = _methodsByServiceHost[serviceHost];
        if (methodDescriptors.ContainsKey(name) != true)
            throw new InvalidOperationException();

        var methodDescriptor = methodDescriptors[name];
        var args = _serializer!.DeserializeMany(methodDescriptor.ParameterTypes, data);
        var instance = _descriptor!.Callbacks[serviceHost];
        Task.Run(() => methodDescriptor.InvokeAsync(_serviceContext, instance, args));
    }

    private void InvokeCallback(IEnumerable<PollReplyItem> pollItems)
    {
        foreach (var item in pollItems)
        {
            var service = _serviceHosts[item.ServiceName];
            InvokeCallback(service, item.Name, item.Data.ToArray());
        }
    }

    private void ThrowException(Type exceptionType, string data)
    {
        if (_serializer == null)
            throw new InvalidOperationException();
        // if (_serviceContext.GetService(typeof(IComponentProvider)) is not IComponentProvider componentProvider)
        //     throw new InvalidOperationException("can not get interface of IComponentProvider at serviceProvider");

        if (Newtonsoft.Json.JsonConvert.DeserializeObject(data, exceptionType) is Exception exception)
            throw exception;
        // var exceptionDescriptor = componentProvider.GetExceptionDescriptor(id);
        // if (_serializer.Deserialize(exceptionDescriptor.ExceptionType, data) is Exception exception)
        //     throw exception;
        throw new UnreachableException();
    }

    #region IAdaptorHost

    void IAdaptorHost.Invoke(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        if (_adaptorImpl == null)
            throw new InvalidOperationException();

        var token = $"{_adaptorImpl.Token}";
        var data = _serializer!.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Data.AddRange(data);
        var reply = _adaptorImpl.Invoke(request);
        if (reply.ID != string.Empty && Type.GetType(reply.ID) is { } exceptionType)
        {
            ThrowException(exceptionType, reply.Data);
        }
    }

    T IAdaptorHost.Invoke<T>(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        if (_adaptorImpl == null || _serializer == null)
            throw new InvalidOperationException();

        var token = $"{_adaptorImpl.Token}";
        var data = _serializer.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Data.AddRange(data);
        var reply = _adaptorImpl.Invoke(request);
        if (reply.ID != string.Empty && Type.GetType(reply.ID) is { } exceptionType)
        {
            ThrowException(exceptionType, reply.Data);
        }
        if (_serializer.Deserialize(typeof(T), reply.Data) is T value)
            return value;
        throw new UnreachableException();
    }

    async Task IAdaptorHost.InvokeAsync(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        if (_adaptorImpl == null || _serializer == null)
            throw new InvalidOperationException();

        var token = $"{_adaptorImpl.Token}";
        var data = _serializer.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Data.AddRange(data);
        var reply = await _adaptorImpl.InvokeAsync(request);
        if (reply.ID != string.Empty && Type.GetType(reply.ID) is { } exceptionType)
        {
            ThrowException(exceptionType, reply.Data);
        }
    }

    async Task<T> IAdaptorHost.InvokeAsync<T>(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        if (_adaptorImpl == null || _serializer == null)
            throw new InvalidOperationException();

        var token = $"{_adaptorImpl.Token}";
        var data = _serializer.SerializeMany(types, args);
        var request = new InvokeRequest()
        {
            ServiceName = instance.ServiceName,
            Name = name,
            Token = token
        };
        request.Data.AddRange(data);
        var reply = await _adaptorImpl.InvokeAsync(request);
        if (reply.ID != string.Empty && Type.GetType(reply.ID) is { } exceptionType)
        {
            ThrowException(exceptionType, reply.Data);
        }
        if (_serializer.Deserialize(typeof(T), reply.Data) is T value)
            return value;
        return default!;
    }

    #endregion
}
