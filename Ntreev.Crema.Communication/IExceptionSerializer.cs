using System;

namespace Ntreev.Crema.Communication
{
    public interface IExceptionSerializer
    {
        Type ExceptionType { get; }

        string Serialize(Exception e);

        Exception Deserialize(string text);
    }
}