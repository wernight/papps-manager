using System;
using System.ServiceModel;

namespace PAppsManager.Core.SingleInstance
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SingleInstanceApp : ISingleInstanceApp
    {
        private readonly Action<string[]> _callback;

        public SingleInstanceApp(Action<string[]> callback)
        {
            _callback = callback;
        }

        public void SignalExternalCommandLineArgs(string[] args)
        {
            _callback.Invoke(args);
        }
    }
}