using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using Items;
using UnityEngine.EventSystems;
using UI;
using UI.NodeNetwork;
using UI.QuestBook.Data.Node;

namespace UI.QuestBook {
    public class QuestBookNodeObject : NodeUI<QuestBookNode,QuestBookPageUI>
    {
        private ItemObject itemObject;
        public override void Initialize(QuestBookNode node, QuestBookPageUI nodeNetwork)
        {
            base.Initialize(node, nodeNetwork);
            itemObject = ItemRegistry.GetInstance().GetItemObject(node?.NodeData.ImageSeralizedItemSlot?.id);
            DisplayImage();
        }

        public override void DisplayImage()
        {
            if (!itemObject) return;
            mItemSlotUI.Display(new ItemSlot(itemObject,1,null));
        }
        
        public override void OpenContent(NodeUIContentOpenMode contentOpenMode)
        {
            UIAssetManager assetManager = nodeNetwork.QuestBookUI.AssetManager;
            QuestBookTaskPageUI pageUI = assetManager.cloneElement<QuestBookTaskPageUI>("TASK_PAGE");
            pageUI.Initialize(node,nodeNetwork,nodeNetwork.QuestBookPath);
            pageUI.transform.SetParent(nodeNetwork.QuestBookUI.transform,false);
        }
    }
}

