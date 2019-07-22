using System;

namespace Client
{
    public interface IShell : IDisposable
    {
        void Cancel();

        void Start();
    }
}