using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Session = Opc.Ua.Client.Session;

namespace OpcUaStackOverflowExample
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var serverApp = new ApplicationInstance
            {
                ApplicationName = "UA Core StackOverflow Example Server",
                ApplicationType = ApplicationType.Server,
                ConfigSectionName = "Opc.Server"
            };
            var folderCount = uint.Parse(args.Where(arg => uint.TryParse(arg, out var value) && value > 0).FirstOrDefault() ?? "2500");
            var serverConfig = await serverApp.LoadApplicationConfiguration(false);
            var serverAddress = serverConfig.ServerConfiguration.BaseAddresses.First();
            if (args.FirstOrDefault() == "server-only")
            {
                await RunServer(serverApp, serverConfig, folderCount, () =>
                {
                    var completion = new TaskCompletionSource<bool>();
                    Console.CancelKeyPress += (sender, eArgs) =>
                    {
                        completion.SetResult(true);
                        eArgs.Cancel = true;
                    };
                    return completion.Task;
                });
            }
            else if (args.FirstOrDefault() == "client-only")
            {
                await RunClient(serverAddress);
            }
            else
            {
                await RunServer(serverApp, serverConfig, folderCount, () => RunClient(serverAddress));
            }
        }

        private static async Task RunServer(ApplicationInstance serverApp, ApplicationConfiguration serverConfig, uint folderCount, Func<Task> termination)
        {
            var haveServerAppCertificate = await serverApp.CheckApplicationInstanceCertificate(false, 0);
            if (!haveServerAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }

            if (!serverConfig.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                serverConfig.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidation);
            }

            using (var server = new OverflowServer(folderCount))
            {
                await serverApp.Start(server);
                Console.WriteLine("Server started");
                await termination();
                Console.WriteLine("Stopping server");
                server.Stop();
                Console.WriteLine("Server stopped");
            }
            Console.WriteLine("Server disposed");
        }

        private static async Task RunClient(string address)
        {
            var clientApp = new ApplicationInstance
            {
                ApplicationName = "OPC UA StackOverflow Example Client",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "Opc.Client"
            };

            // load the application configuration.
            var clientConfig = await clientApp.LoadApplicationConfiguration(true);

            var haveClientAppCertificate = await clientApp.CheckApplicationInstanceCertificate(true, 0);
            if (!haveClientAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }
            var endpointConfiguration = EndpointConfiguration.Create(clientConfig);
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(address, true, 15000);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            using (var session = await Session.Create(clientConfig, endpoint, false, clientConfig.ApplicationName, 60000, new UserIdentity(new AnonymousIdentityToken()), null))
            {
                Console.WriteLine("Client connected");
                var rootFolders = await session.BrowseAsync(ObjectIds.ObjectsFolder);
                Console.WriteLine("Received root folders");
                var childFolders = await Task.WhenAll(
                    from root in rootFolders
                    select session.BrowseAsync(ExpandedNodeId.ToNodeId(root.NodeId, session.NamespaceUris)));

                foreach (var childFolder in childFolders.SelectMany(x => x).OrderBy(c => c.BrowseName.Name))
                {
                    Console.WriteLine(childFolder.BrowseName.Name);
                }
                Console.WriteLine("Client disconnecting");
                session.Close();
                Console.WriteLine("Client disconnected");
            }
            Console.WriteLine("Client disposed");
        }

        private static void CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = true;
            }
        }

        public static Task<ReferenceDescriptionCollection> BrowseAsync(this Session session, NodeId nodeToBrowse)
        {
            return Task.Factory.FromAsync(
                (callback, state) =>
                    session.BeginBrowse(
                        null,
                        null,
                        nodeToBrowse,
                        0,
                        BrowseDirection.Forward,
                        ReferenceTypeIds.Organizes,
                        false,
                        (uint)NodeClass.Object,
                        callback,
                        state),
                result =>
                {
                    session.EndBrowse(result, out var continuationPoint, out var references);
                    return references;
                },
                null);
        }
    }
}