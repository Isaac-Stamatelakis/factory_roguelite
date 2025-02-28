namespace TileEntity.Instances.CompactMachine
{
    internal class CompactMachineMetaData
    {
        public string Name;
        public bool Locked;
        public int Instances;
        public CompactMachineMetaData(string name, bool locked, int instances)
        {
            Name = name;
            Locked = locked;
            Instances = instances;

        }
    }
}
