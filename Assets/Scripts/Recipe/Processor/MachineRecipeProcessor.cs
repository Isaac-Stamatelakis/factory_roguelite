using TileEntity.Instances.Machine;
using UnityEngine;

namespace Recipe.Processor
{
    [CreateAssetMenu(fileName = "New Recipe Processor", menuName = "Crafting/Processor/Machine")]
    public class MachineRecipeProcessor : RecipeProcessor
    {
        public MachineLayoutObject MachineLayout;
    }
}