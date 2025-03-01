namespace TileEntity.Instances.CompactMachine
{
    internal class CompactMachineMetaData
    {
        public string Name;
        public bool Locked;
        public int Instances;
        public string TileID;
        public CompactMachineMetaData(string name, bool locked, int instances, string tileID)
        {
            Name = name;
            Locked = locked;
            Instances = instances;
            TileID = tileID;
        }
    }
}
