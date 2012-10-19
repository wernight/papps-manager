using System.ServiceModel;

namespace PAppsManager.Core
{
    [ServiceContract]
    public interface ISingleInstanceApp
    {
        [OperationContract]
        void SignalExternalCommandLineArgs(string[] arg);
    }
}