using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using Items;
using UnityEngine.EventSystems;
using UI;
using UI.NodeNetwork; 

namespace UI.QuestBook {
    public class QuestBookNodeObject : NodeUI<QuestBookNode,QuestBookPageUI>
    {
        [SerializeField] private ItemSlotUI itemSlotUI;
        protected override void setImage()
        {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(node.ImageSeralizedItemSlot);
            itemSlotUI.Display(itemSlot);
        }

        protected override void openContent()
        {
            UIAssetManager assetManager = nodeNetwork.QuestBookUI.AssetManager;
            QuestBookTaskPageUI pageUI = assetManager.cloneElement<QuestBookTaskPageUI>("TASK_PAGE");
            pageUI.init(node,nodeNetwork);
            pageUI.transform.SetParent(nodeNetwork.QuestBookUI.transform,false);
        }
    }
}

