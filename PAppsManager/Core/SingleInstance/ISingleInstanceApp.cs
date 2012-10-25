using System.ServiceModel;

namespace PAppsManager.Core.SingleInstance
{
    [ServiceContract]
    public interface ISingleInstanceApp
    {
        [OperationContract]
        void SignalExternalCommandLineArgs(string[] arg);
    }
}