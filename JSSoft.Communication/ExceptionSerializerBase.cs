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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace JSSoft.Communication;

public abstract class ExceptionSerializerBase<T> : IExceptionDescriptor, IDataSerializer where T : Exception
{
    protected ExceptionSerializerBase(Guid id)
    {
        this.ID = id;
    }

    protected ExceptionSerializerBase(string id)
        : this(new Guid(id))
    {
    }

    public Type ExceptionType => typeof(T);

    public Guid ID { get; }

    protected virtual void GetSerializationInfo(IReadOnlyDictionary<string, object> properties, SerializationInfo info)
    {
        info.AddValue("ClassName", properties["ClassName"], typeof(string));
        info.AddValue("Message", properties["Message"], typeof(string));
        info.AddValue("Data", null, typeof(IDictionary));
        info.AddValue("InnerException", null, typeof(Exception));
        info.AddValue("HelpURL", properties["HelpURL"], typeof(string));
        info.AddValue("StackTraceString", null, typeof(string));
        info.AddValue("RemoteStackTraceString", null, typeof(string));
        info.AddValue("RemoteStackIndex", 0, typeof(int));
        info.AddValue("ExceptionMethod", null, typeof(string));
        info.AddValue("HResult", (int)(long)properties["HResult"], typeof(int));
        info.AddValue("Source", null, typeof(string));
        info.AddValue("WatsonBuckets", null, typeof(byte[]));
    }

    protected virtual void GetProperties(SerializationInfo info, IDictionary<string, object> properties)
    {
        properties.Add("ClassName", info.GetString("ClassName"));
        properties.Add("Message", info.GetString("Message"));
        properties.Add("HelpURL", info.GetString("HelpURL"));
        properties.Add("HResult", info.GetInt32("HResult"));
    }

    private static SerializationInfo GetSerializationInfo(T e)
    {
        var converter = new FormatterConverter();
        var info = new SerializationInfo(typeof(T), converter);
        var context = new StreamingContext(StreamingContextStates.Clone);
        e.GetObjectData(info, context);
        return info;
    }

    #region IDataSerializer

    object IDataSerializer.Deserialize(ISerializer serializer, string text)
    {
        var converter = new FormatterConverter();
        var info = new SerializationInfo(this.ExceptionType, converter);
        var context = new StreamingContext(StreamingContextStates.Clone);
        var propserties = serializer.Deserialize(typeof(Dictionary<string, object>), text) as Dictionary<string, object>;
        this.GetSerializationInfo(propserties, info);
        return Activator.CreateInstance(this.ExceptionType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { info, context }, null);
    }

    string IDataSerializer.Serialize(ISerializer serializer, object data)
    {
        var e = data as T;
        var info = GetSerializationInfo(e);
        var properties = new Dictionary<string, object>(info.MemberCount);
        this.GetProperties(info, properties);
        return serializer.Serialize(properties.GetType(), properties);
    }

    Type IDataSerializer.Type => this.ExceptionType;

    #endregion
}
