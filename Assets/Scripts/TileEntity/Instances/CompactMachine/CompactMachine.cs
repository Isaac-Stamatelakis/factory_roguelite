using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using UnityEngine.Tilemaps;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine")]
    public class CompactMachine : TileEntity, IClickableTileEntity, IConduitInteractable, IEnergyConduitInteractable, IItemConduitInteractable, IFluidConduitInteractable
    {
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        [SerializeField] public GameObject tilemapContainer;
        public ItemSlot extractItem()
        {
            throw new System.NotImplementedException();
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public ref int getEnergy()
        {
            throw new System.NotImplementedException();
        }

        public int insertEnergy(int energy)
        {
            throw new System.NotImplementedException();
        }

        public ItemSlot insertItem(ItemSlot itemSlot)
        {
            throw new System.NotImplementedException();
        }

        public void onClick()
        {
            throw new System.NotImplementedException();
        }
    }
}

