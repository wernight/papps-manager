using System;
using System.ServiceModel;
using System.Threading;

namespace PAppsManager.Core.SingleInstance
{
    /// <summary>
    /// Helper to force a single application instance and communicate with it from another instance.
    /// 
    ///  1. Create a communication interface (can be empty).
    ///  2. <code>
    ///     instance = new SingleInstance{ICommunicationInterface}(this, "UniqueName");
    ///     if (instance.IsAlreadyRunning)
    ///     {
    ///         instance.AlreadyRunningInstance.DoSomething();
    ///     }
    ///     </code>
    /// </summary>
    internal class SingleInstance : IDisposable
    {
        /// <summary>
        /// Note: Also used to avoid the mutex to be garbage collected.
        /// </summary>
        private readonly Mutex _mutex;

        private ServiceHost _serviceHost;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="uniqueApplicationName"></param>
        /// <param name="callback">Call by other instances with the command line arguments passed to them.</param>
        public SingleInstance(string uniqueApplicationName, Action<string[]> callback)
        {
            // Single instance of the applicaiton allowed.
            bool createdNew;
            _mutex = new Mutex(true, uniqueApplicationName, out createdNew);
            IsAlreadyRunning = !createdNew;

            if (!IsAlreadyRunning)
                StartServer(callback);
        }

        public bool IsAlreadyRunning { get; private set; }

        public void SignalExternalCommandLineArgs(string[] args)
        {
            var client = CreateClient();
            try
            {
                client.SignalExternalCommandLineArgs(args);
            }
            finally
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                // ReSharper disable HeuristicUnreachableCode
                var communicationObject = client as ICommunicationObject;
                if (communicationObject != null)
                    communicationObject.Close();
                // ReSharper restore HeuristicUnreachableCode
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_serviceHost != null)
                _serviceHost.Close();

            if (_mutex != null)
                _mutex.Dispose();
        }

        #endregion

        private void StartServer(Action<string[]> callback)
        {
            // Run server
            _serviceHost = new ServiceHost(new SingleInstanceApp(callback));
            _serviceHost.AddServiceEndpoint(typeof (ISingleInstanceApp),
                                            new NetNamedPipeBinding(),
                                            "net.pipe://localhost");
            _serviceHost.Open();
        }

        private ISingleInstanceApp CreateClient()
        {
            // Run client
            var tcpFactory = new ChannelFactory<ISingleInstanceApp>(new NetNamedPipeBinding(),
                                                                    "net.pipe://localhost");
            return tcpFactory.CreateChannel();
        }
    }
}