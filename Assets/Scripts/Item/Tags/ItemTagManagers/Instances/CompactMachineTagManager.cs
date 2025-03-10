using System.IO;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachines;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class CompactMachineTagManager : ItemTagManager, IToolTipTagViewable
    {
        public override string Serialize(object obj)
        {
            return obj as string;
        }

        public override object Deserialize(string data)
        {
            return data;
        }

        public string GetToolTip(object obj)
        {
            string hash = obj as string;
            if (hash == null) return string.Empty;
            string path = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(), hash);
            CompactMachineMetaData metaData = CompactMachineUtils.GetMetaData(path);
            if (metaData == null) return string.Empty;
            return $"Name: '{metaData.Name}'\nID: '{hash}'";
        }
    }
}
