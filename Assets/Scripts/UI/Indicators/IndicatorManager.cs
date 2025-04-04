
using System;
using System.Collections.Generic;
using Dimensions;
using Player;
using Player.Controls;
using Player.UI;
using PlayerModule;
using TMPro;
using UI.QuestBook;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public enum IndicatorDisplayBundle
    {
        TilePlace = 1,
        ConduitPlace = 2,
        ConduitSystem = 4,
    }
    public class IndicatorManager : MonoBehaviour
    {
        public GameObject keyCodePrefab;
        public Transform keyCodeContainer;
        public ConduitPortIndicatorUI conduitPortIndicatorUI;
        public ConduitViewIndicatorUI conduitViewIndicatorUI;
        public ConduitPlacementModeIndicatorUI conduitPlacementModeIndicatorUI;
        public TileRotationIndicatorUI tileRotationIndicatorUI;
        public TileStateIndicatorUI tileStateIndicatorUI;
        public TileHighligherIndicatorUI tilePreviewerIndicatorUI;
        public GenericIndicatorUI questBookIndicator;
        public GenericIndicatorUI inventoryIndicator;
        public GenericIndicatorUI loadOutIndicator;
        public CaveIndicatorUI caveIndicatorUI;
        private Transform indicatorTransform;
        private int viewMode;

        public void Start()
        {
            indicatorTransform = conduitPortIndicatorUI.transform.parent; // The indicator doesn't matter, they all share the same parent

            void OnQuestBookClick()
            {
                QuestBookUIManager questBookUIManager = MainCanvasController.TInstance.QuestBookUIManager;
                questBookUIManager.DisplayQuestBook();
            }
            questBookIndicator.Initialize(PlayerControl.OpenQuestBook, ()=> "Open Quest Book", OnQuestBookClick);
            
            void OnInventoryClick()
            {
                PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
                PlayerInventoryUI playerInventoryUI = Instantiate(playerScript.Prefabs.PlayerInventoryUIPrefab);
                playerInventoryUI.Display(playerScript);
                CanvasController.Instance.DisplayObject(playerInventoryUI.gameObject, keyCodes: ControlUtils.GetKeyCodes(PlayerControl.OpenInventory));
            }
            inventoryIndicator.Initialize(PlayerControl.OpenInventory, ()=> "Open Inventory", OnInventoryClick);
            
            
        }

        public void Initialize(PlayerScript playerScript)
        {
            conduitPortIndicatorUI?.Display(playerScript);
            conduitViewIndicatorUI?.Display(playerScript);
            conduitPlacementModeIndicatorUI?.Display(playerScript.ConduitPlacementOptions);
            tileRotationIndicatorUI.Display(playerScript.TilePlacementOptions);
            tileStateIndicatorUI.Display(playerScript.TilePlacementOptions);
            tilePreviewerIndicatorUI.Display(playerScript);
            DisplayMode();
        }

        public void AddViewBundle(IndicatorDisplayBundle bundle)
        {
            if ((viewMode & (int)bundle) != 0) return;
            viewMode += (int)bundle;
            
            DisplayMode();
        }

        public void RemovePlaceBundles()
        {
            RemoveBundle(IndicatorDisplayBundle.ConduitPlace);
            RemoveBundle(IndicatorDisplayBundle.TilePlace);
        }

        public void RemoveBundle(IndicatorDisplayBundle bundle)
        {
            if ((viewMode & (int)bundle) == 0) return;
            viewMode -= (int)bundle;
            DisplayMode();
        }
        

        public void DisplayMode()
        {
            for (int i = 0; i < indicatorTransform.childCount; i++)
            {
                indicatorTransform.transform.GetChild(i).gameObject.SetActive(false);
            }
      
            questBookIndicator.gameObject.SetActive(true);
            inventoryIndicator.gameObject.SetActive(true);
            loadOutIndicator.gameObject.SetActive(true);

            bool ViewBundleActive(IndicatorDisplayBundle bundle)
            {
                return (viewMode & (int)bundle) != 0;
            }

            if (ViewBundleActive(IndicatorDisplayBundle.TilePlace))
            {
                tilePreviewerIndicatorUI.gameObject.SetActive(true);
                tileRotationIndicatorUI.gameObject.SetActive(true);
                tileStateIndicatorUI.gameObject.SetActive(true);
            }
            
            if (ViewBundleActive(IndicatorDisplayBundle.ConduitSystem))
            {
                conduitViewIndicatorUI.gameObject.SetActive(true);
                conduitPortIndicatorUI.gameObject.SetActive(true);
                if (ViewBundleActive(IndicatorDisplayBundle.ConduitPlace))
                {
                    tilePreviewerIndicatorUI.gameObject.SetActive(true);
                    conduitPlacementModeIndicatorUI.gameObject.SetActive(true);
                }
            }
            else
            {
                caveIndicatorUI.gameObject.SetActive(true);
            }
            
            
            SyncKeyCodes(true);
        }
        
        public void SyncKeyCodes(bool instantiate)
        {
            if (instantiate)
            {
                GlobalHelper.DeleteAllChildren(keyCodeContainer);
            }

            int idx = 0;
            for (int i = 0; i < indicatorTransform.childCount; i++)
            {
                GameObject keyCodeObject = indicatorTransform.GetChild(i).gameObject;
                if (!keyCodeObject.activeInHierarchy) continue;
                
                IKeyCodeIndicator keyCodeIndicator = keyCodeObject.GetComponent<IKeyCodeIndicator>();
                PlayerControl? nullableControl = keyCodeIndicator?.GetPlayerControl();
                string text = nullableControl.HasValue
                    ? ControlUtils.KeyCodeListAsString(ControlUtils.GetKeyCodes(nullableControl.Value),"\n")
                    : string.Empty;
                GameObject keyCodeElement = instantiate ? 
                    Instantiate(keyCodePrefab, keyCodeContainer)
                    : keyCodeContainer.GetChild(idx).gameObject;
                if (string.IsNullOrEmpty(text))
                {
                    keyCodeElement.GetComponent<Image>().enabled = false;
                }
                keyCodeElement.GetComponentInChildren<TextMeshProUGUI>().text = text;
                idx++;
            }
        }

        public void SetColor(Color color)
        {
            Image[] images = GetComponentsInChildren<Image>();
            foreach (Image image in images)
            {
                image.color = color;
            }
        }
    }
}
