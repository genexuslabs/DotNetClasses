using SecurityAPICommons.Commons;
using System.Security;
using GeneXusFtps.GeneXusFtps;


namespace GeneXusFtps.GeneXusCommons
{
    [SecuritySafeCritical]
    public abstract class IFtpsClientObject : SecurityAPIObject
    {
        public abstract bool Connect(FtpsOptions options);
        public abstract bool Put(string localPath, string remoteDir);
#pragma warning disable CA1716 // Identifiers should not match keywords
		public abstract bool Get(string remoteFilePath, string localDir);
#pragma warning restore CA1716 // Identifiers should not match keywords
		public abstract void Disconnect();

        public abstract string GetWorkingDirectory();
    }
}
