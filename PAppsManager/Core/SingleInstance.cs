using System;
using System.ServiceModel;
using System.Threading;

namespace PAppsManager.Core
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
    /// <typeparam name="TCommunicationInterface"></typeparam>
    internal class SingleInstance<TCommunicationInterface> : IDisposable
    {
        /// <summary>
        /// Note: Also used to avoid the mutex to be garbage collected.
        /// </summary>
        private readonly Mutex _mutex;

        private ServiceHost _serviceHost;

        public SingleInstance(TCommunicationInterface singleInstance, string uniqueApplicationName)
        {
            // Single instance of the applicaiton allowed.
            bool createdNew;
            _mutex = new Mutex(true, uniqueApplicationName, out createdNew);
            IsAlreadyRunning = !createdNew;

            if (!IsAlreadyRunning)
                StartServer(singleInstance);
            else
                AlreadyRunningApplication = CreateClient();
        }

        public bool IsAlreadyRunning { get; private set; }

        public TCommunicationInterface AlreadyRunningApplication { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (_serviceHost != null)
                _serviceHost.Close();

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode
            var communicationObject = AlreadyRunningApplication as ICommunicationObject;
            if (communicationObject != null)
                communicationObject.Close();
// ReSharper restore HeuristicUnreachableCode
// ReSharper restore ConditionIsAlwaysTrueOrFalse

            if (_mutex != null)
                _mutex.Dispose();
        }

        #endregion

        private void StartServer(TCommunicationInterface singleInstance)
        {
            // Run server
            _serviceHost = new ServiceHost(singleInstance);
            _serviceHost.AddServiceEndpoint(typeof (TCommunicationInterface),
                                            new NetNamedPipeBinding(),
                                            "net.pipe://localhost");
            _serviceHost.Open();
        }

        private TCommunicationInterface CreateClient()
        {
            // Run client
            var tcpFactory = new ChannelFactory<TCommunicationInterface>(new NetNamedPipeBinding(),
                                                                         "net.pipe://localhost");
            return tcpFactory.CreateChannel();
        }
    }
}