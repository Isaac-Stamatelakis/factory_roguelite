using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.Tilemaps;
using UI;
using Items;


namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName ="New Fluid Tank",menuName="Tile Entity/Storage/Fluid/Standard")]
    public class FluidTank : TileEntity, IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ILoadableTileEntity, IFluidConduitInteractable
    {
        [SerializeField] private Tier tier;
        [SerializeField] private ConduitPortLayout layout;
        [SerializeField] private FluidTankUI uiPrefab;
        private ItemSlot itemSlot;
        public int FillAmount {get => itemSlot.amount;}
        public float FillRatio {get => ((float)FillAmount)/getStorage();}
        public ItemSlot ItemSlot { get => itemSlot; set => itemSlot = value; }
        private SpriteRenderer visualElement;

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public int getStorage() {
            return tier.getFluidStorage();
        }

        public void load()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;   
            }
            GameObject fluid = new GameObject();
            fluid.name = "Fluid";
            fluid.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
            fluid.transform.position = getWorldPosition();
            visualElement = fluid.AddComponent<SpriteRenderer>();
            updateVisual();
        }

        private void updateVisual() {
            if (visualElement == null) {
                return;
            }
            if (itemSlot == null || itemSlot.itemObject == null) {
                visualElement.gameObject.SetActive(false);
                return;
            }
            visualElement.gameObject.SetActive(true);
            visualElement.sprite = itemSlot.itemObject.getSprite();
            float height = FillRatio*0.5f;
            visualElement.transform.localScale = new Vector3(0.5f,height,1);
            Vector2 position = getWorldPosition();
            visualElement.transform.position = new Vector3(position.x,position.y-0.25f+height/2,1.5f);

        }

        public void onRightClick()
        {
            FluidTankUI fluidTankUI = GameObject.Instantiate(uiPrefab);
            fluidTankUI.init(this);
            GlobalUIContainer.getInstance().getUiController().setGUI(fluidTankUI.gameObject);
        }

        public string serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void unload()
        {
            GameObject.Destroy(visualElement.gameObject);
        }

        public void unserialize(string data)
        {
            this.itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(data);
        }

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            updateVisual();
            return itemSlot;
        }

        public void insertFluidItem(ItemSlot toInsert, Vector2Int portPosition)
        {
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = ItemSlotFactory.copy(toInsert);
                toInsert.amount = 0;
                return;
            }
            if (!ItemSlotHelper.areEqual(itemSlot,toInsert)) {
                return;
            }
            ItemSlotHelper.insertIntoSlot(itemSlot,toInsert,getStorage());
            updateVisual();
        }
    }
}

