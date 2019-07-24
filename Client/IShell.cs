using System;

namespace Client
{
    public interface IShell : IDisposable
    {
        void Stop();

        void Start();
    }
}