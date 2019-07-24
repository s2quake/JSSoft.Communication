using System;

namespace Server
{
    public interface IShell : IDisposable
    {
        void Stop();

        void Start();
    }
}