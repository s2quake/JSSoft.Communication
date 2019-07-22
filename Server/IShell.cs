using System;

namespace Server
{
    public interface IShell : IDisposable
    {
        void Cancel();

        void Start();
    }
}