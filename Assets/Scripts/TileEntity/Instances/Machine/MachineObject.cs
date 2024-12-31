using Conduits.Ports;
using Recipe.Processor;

namespace TileEntity.Instances.Machine
{
    public enum MachineType
    {
        Passive,
        Burner,
        Generator,
        Processor,
    }
    public abstract class MachineObject : TileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public MachineRecipeProcessor RecipeProcessor;
    }
}
