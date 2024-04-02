using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Storage {
    public class FluidTankUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private RectTransform spriteContainer;
        [SerializeField] private TextMeshProUGUI title;
        private FluidTank fluidTank;
        private string itemID;
        public void init(FluidTank fluidTank) {
            this.fluidTank = fluidTank;
            
        }

        public void Update() {
            if (fluidTank == null || fluidTank.ItemSlot == null || fluidTank.ItemSlot.itemObject == null) {
                image.gameObject.SetActive(false);
                return;
            }
            image.gameObject.SetActive(true);
            image.sprite = fluidTank.ItemSlot.itemObject.getSprite();
            title.text = fluidTank.ItemSlot.itemObject.name;
            Vector3 imagePosition = image.transform.localPosition;
            imagePosition.y = spriteContainer.sizeDelta.y * fluidTank.FillRatio;
            image.transform.localPosition = imagePosition;
        }


        public static FluidTankUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/TileEntities/FluidTank/FluidTankUI").GetComponent<FluidTankUI>();
        }
    }
}

