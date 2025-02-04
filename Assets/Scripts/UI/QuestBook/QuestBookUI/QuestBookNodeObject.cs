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

namespace UI.QuestBook {
    public class QuestBookNodeObject : NodeUI<QuestBookNode,QuestBookPageUI>
    {
        private int counter;
        private ItemObject itemObject;
        public override void Init(QuestBookNode node, QuestBookPageUI nodeNetwork)
        {
            base.Init(node, nodeNetwork);
            itemObject = ItemRegistry.GetInstance().GetItemObject(node?.ImageSeralizedItemSlot?.id);
            DisplayImage();
        }

        public override void DisplayImage()
        {
            
            if (ReferenceEquals(itemObject,null) || !itemObject) return;
            ItemDisplayUtils.DisplayItemSprite(image,itemObject,counter);
            bool small = image.transform.localScale is { x: 0.5f, y: 0.5f };
            if (small) image.transform.localScale = Vector3.one;
        }

        public void FixedUpdate()
        {
            counter++;
            DisplayImage();
        }

        protected override void openContent()
        {
            UIAssetManager assetManager = nodeNetwork.QuestBookUI.AssetManager;
            QuestBookTaskPageUI pageUI = assetManager.cloneElement<QuestBookTaskPageUI>("TASK_PAGE");
            pageUI.Initialize(node,nodeNetwork);
            pageUI.transform.SetParent(nodeNetwork.QuestBookUI.transform,false);
        }
    }
}

