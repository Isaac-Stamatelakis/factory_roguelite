namespace Item.Tags.ItemTagManagers.Instances
{
    public class EnergyItemTagManager : ItemTagManager, IToolTipTagViewable
    {
        public override string Serialize(object obj)
        {
            return obj is not ulong value ? null : value.ToString();
        }

        public override object Deserialize(string data)
        {
            return System.Convert.ToUInt64(data);
        }

        public string GetToolTip(object obj)
        {
            return obj is not ulong value ? string.Empty : $"Energy:{value.ToString()}";
        }
    }
}
