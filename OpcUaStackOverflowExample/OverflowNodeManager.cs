using Opc.Ua;
using Opc.Ua.Server;
using System.Collections.Generic;

namespace OpcUaStackOverflowExample
{
    public class OverflowNodeManager : CustomNodeManager2
    {
        private readonly uint folderCount;
        public OverflowNodeManager(IServerInternal server, ApplicationConfiguration configuration, uint folderCount) : base(server, configuration, "urn:stack-overflow-ns")
        {
            this.folderCount = folderCount;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            base.CreateAddressSpace(externalReferences);

            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var references))
            {
                externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
            }

            for (var i = 0; i < folderCount; i++)
            {
                var root = CreateFolder(null, $"root.r{i:D4}", $"root{i:D4}");
                root.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);
                references.Add(new NodeStateReference(ReferenceTypes.Organizes, false, root.NodeId));
                CreateFolder(root, $"root.r{i:D4}.c", $"child{i:D4}");
                AddPredefinedNode(SystemContext, root);
            }
        }

        private FolderState CreateFolder(NodeState parent, string path, string name)
        {
            var folder = new FolderState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId(path, NamespaceIndex),
                BrowseName = new QualifiedName(path, NamespaceIndex),
                DisplayName = new LocalizedText("en", name),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None
            };

            parent?.AddChild(folder);

            return folder;
        }
    }
}
