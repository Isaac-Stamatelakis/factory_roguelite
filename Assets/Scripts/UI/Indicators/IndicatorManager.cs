
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
    public enum IndicatorManagerDisplayMode
    {
        Default = 0,
        TilePlace = 1,
        ConduitPlace = 2,
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
        private Transform indicatorTransform;
        private IndicatorManagerDisplayMode currentDisplayMode = IndicatorManagerDisplayMode.Default;

        public void Start()
        {
            indicatorTransform = conduitPortIndicatorUI.transform.parent; // The indicator doesn't matter, they all share the same parent

            void OnQuestBookClick()
            {
                QuestBookUIManager questBookUIManager = MainCanvasController.TInstance.QuestBookUIManager;
                questBookUIManager.DisplayQuestBook();
            }
            questBookIndicator.Initialize(PlayerControl.OpenQuestBook, "Open Quest Book", OnQuestBookClick);
            
            void OnInventoryClick()
            {
                PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
                PlayerInventoryUI playerInventoryUI = Instantiate(playerScript.Prefabs.PlayerInventoryUIPrefab);
                playerInventoryUI.Display(playerScript);
                CanvasController.Instance.DisplayObject(playerInventoryUI.gameObject, keyCodes: new List<KeyCode>{KeyCode.E});
            }
            inventoryIndicator.Initialize(PlayerControl.OpenInventory, "Open Inventory", OnInventoryClick);
            
            
        }

        public void Initialize(PlayerScript playerScript)
        {
            conduitPortIndicatorUI?.Display(playerScript);
            conduitViewIndicatorUI?.Display(playerScript);
            conduitPlacementModeIndicatorUI?.Display(playerScript.ConduitPlacementOptions);
            tileRotationIndicatorUI.Display(playerScript.TilePlacementOptions);
            tileStateIndicatorUI.Display(playerScript.TilePlacementOptions);
            tilePreviewerIndicatorUI.Display(playerScript);
            DisplayMode(IndicatorManagerDisplayMode.Default, force : true);
        }

        public void DisplayMode(IndicatorManagerDisplayMode displayMode, bool force = false)
        {
            if (!force && currentDisplayMode == displayMode) return;
            SyncKeyCodes(true);
            currentDisplayMode = displayMode;
            for (int i = 0; i < keyCodeContainer.childCount; i++)
            {
                indicatorTransform.transform.GetChild(i).gameObject.SetActive(false);
            }
            conduitPortIndicatorUI.gameObject.SetActive(true);
            conduitViewIndicatorUI.gameObject.SetActive(true);
            questBookIndicator.gameObject.SetActive(true);
            inventoryIndicator.gameObject.SetActive(true);
            loadOutIndicator.gameObject.SetActive(true);
            
            
            switch (displayMode)
            {
                case IndicatorManagerDisplayMode.Default:
                    break;
                case IndicatorManagerDisplayMode.TilePlace:
                    tilePreviewerIndicatorUI.gameObject.SetActive(true);
                    tileRotationIndicatorUI.gameObject.SetActive(true);
                    tileStateIndicatorUI.gameObject.SetActive(true);
                    break;
                case IndicatorManagerDisplayMode.ConduitPlace:
                    tilePreviewerIndicatorUI.gameObject.SetActive(true);
                    conduitPlacementModeIndicatorUI.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(displayMode), displayMode, null);
            }
        }
        
        public void SyncKeyCodes(bool instantiate)
        {
            if (instantiate)
            {
                GlobalHelper.DeleteAllChildren(keyCodeContainer);
            }
            for (int i = 0; i < indicatorTransform.childCount; i++)
            {
                GameObject keyCodeObject = indicatorTransform.GetChild(i).gameObject;
                if (!keyCodeObject.activeInHierarchy) continue;
                
                IKeyCodeIndicator keyCodeIndicator = keyCodeObject.GetComponent<IKeyCodeIndicator>();
                PlayerControl? nullableControl = keyCodeIndicator?.GetPlayerControl();
                string text = nullableControl.HasValue
                    ? ControlUtils.FormatKeyText(nullableControl.Value)
                    : string.Empty;
                GameObject keyCodeElement = instantiate ? 
                    Instantiate(keyCodePrefab, keyCodeContainer)
                    : keyCodeContainer.GetChild(i).gameObject;
                keyCodeElement.GetComponentInChildren<TextMeshProUGUI>().text = text;
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
