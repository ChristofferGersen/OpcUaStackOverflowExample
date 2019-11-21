using Opc.Ua;
using Opc.Ua.Server;

namespace OpcUaStackOverflowExample
{
    public class OverflowServer : StandardServer
    {
        private readonly uint folderCount;
        public OverflowServer(uint folderCount)
        {
            this.folderCount = folderCount;
        }

        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            return new MasterNodeManager(server, configuration, null, new OverflowNodeManager(server, configuration, folderCount));
        }
    }
}
