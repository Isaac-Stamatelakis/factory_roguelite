using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
namespace RobotModule {
    public class RobotItemData
    {
        private List<ItemSlot> equipment;
        private List<ItemSlot> accessories;
        private string name;
        public List<ItemSlot> Equipment { get => equipment; set => equipment = value; }
        public List<ItemSlot> Accessories { get => accessories; set => accessories = value; }
        public string Name { get => name; set => name = value; }

        public RobotItemData(List<ItemSlot> equipment, List<ItemSlot> accessories, string name) {
            this.name = name;
            this.equipment = equipment;
            this.accessories = accessories;
        }
    }
}

