using System;

namespace Ntreev.Crema.Services
{
    internal class CallbackBase
    {
        public Action<string, object[]> ActionDelegate { get; set; }

        public void Invoke<T>(string name, T arg)
        {
            this.InvokeDelegate(name, typeof(T), arg);
        }

        public void Invoke<T1, T2>(string name, T1 arg1, T2 arg2)
        {
            this.InvokeDelegate(name, typeof(T1), arg1, typeof(T2), arg2);
        }

        public void Invoke<T1, T2, T3>(string name, T1 arg1, T2 arg2, T3 arg3)
        {
            this.InvokeDelegate(name, typeof(T1), arg1, typeof(T2), arg2, typeof(T3), arg3);
        }

        public void Invoke<T1, T2, T3, T4>(string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            this.InvokeDelegate(name, typeof(T1), arg1, typeof(T2), arg2, typeof(T3), arg3, typeof(T4), arg4);
        }

        public void Invoke<T1, T2, T3, T4, T5>(string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            this.InvokeDelegate(name, typeof(T1), arg1, typeof(T2), arg2, typeof(T3), arg3, typeof(T4), arg4, typeof(T5), arg5);
        }

        private void InvokeDelegate(string name, params object[] args)
        {
            this.ActionDelegate(name, args);
        }
    }

    // class IUserServiceCallbackImpl : CallbackBase, Users.IUserServiceCallback
    // {
    //     public void OnLoggedIn(string userID)
    //     {
    //         Invoke("OnLoggedIn", userID);
    //     }

    //     public void OnAdd(string userID, int test)
    //     {
    //         Invoke("OnAdd", userID, test);
    //     }
    // }
}