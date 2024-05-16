using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Inventory;

namespace TileEntityModule {
    public abstract class TileEntityInventory
    {

    }

    public class StandardSolidAndFluidInventory : TileEntityInventory {
        protected Inventory itemInputs;
        protected Inventory itemOutputs;
        protected Inventory fluidInputs;
        protected Inventory fluidOutputs;
        public Inventory ItemInputs { get => itemInputs; set => itemInputs = value; }
        public Inventory ItemOutputs { get => itemOutputs; set => itemOutputs = value; }
        public Inventory FluidInputs { get => fluidInputs; set => fluidInputs = value; }
        public Inventory FluidOutputs { get => fluidOutputs; set => fluidOutputs = value; }
        public StandardSolidAndFluidInventory(List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs) {
            this.itemInputs = new Inventory(itemInputs);
            this.itemOutputs = new Inventory(itemOutputs);
            this.fluidInputs = new Inventory(fluidInputs);
            this.fluidOutputs = new Inventory(fluidOutputs);
        }
    }


}

