namespace Conduits
{
    public class PortConduitData : ConduitData
    {
        public string PortData;

        public PortConduitData(int state, string portData) : base(state)
        {
            PortData = portData;
        }
    }
}