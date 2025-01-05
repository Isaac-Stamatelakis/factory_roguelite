using Conduits.Ports;
using Recipe.Processor;
using TileEntity.Instances.WorkBenchs;

namespace TileEntity.Instances.Machine
{
    public enum MachineType
    {
        Passive,
        Burner,
        Generator,
        Processor,
    }
    public abstract class MachineObject : TileEntityObject, IProcessorTileEntity
    {
        public ConduitPortLayout ConduitPortLayout;
        public RecipeProcessor RecipeProcessor;
        public RecipeProcessor GetRecipeProcessor()
        {
            return RecipeProcessor;
        }
    }
}
