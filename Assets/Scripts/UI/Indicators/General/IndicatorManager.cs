using Player;
using Player.Controls;
using TMPro;
using UI.Catalogue.ItemSearch;
using UI.QuestBook;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public enum IndicatorDisplayBundle
    {
        TilePlace = 1,
        ConduitPlace = 2,
        ConduitSystem = 4,
        AutoSelect = 8,
    }
    public class IndicatorManager : MonoBehaviour
    {
        public GameObject keyCodePrefab;
        public Transform keyCodeContainer;
        public ConduitPortIndicatorUI conduitPortIndicatorUI;
        public ConduitViewIndicatorUI conduitViewIndicatorUI;
        public ConduitPlacementModeIndicatorUI conduitPlacementModeIndicatorUI;
        public TilePlacementIndicatorUI tilePlacementIndicatorUI;
        public TileHighligherIndicatorUI tilePreviewerIndicatorUI;
        public GenericIndicatorUI questBookIndicator;
        public GenericIndicatorUI inventoryIndicator;
        public RobotLoadOutIndicator loadOutIndicator;
        public GenericIndicatorUI searchIndicator;
        public CaveIndicatorUI caveIndicatorUI;
        public TileAutoSelectIndicatorUI autoSelectIndicator;
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
                playerScript.PlayerInventory.ToggleInventoryMode();
            }
            inventoryIndicator.Initialize(PlayerControl.OpenInventory, ()=> "Open Inventory", OnInventoryClick);
            
            void OnSearchClick()
            {
                PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
                ItemSearchUI itemSearchUI = Instantiate(playerScript.Prefabs.ItemSearchUIPrefab);
                itemSearchUI.Initialize(playerScript);
                CanvasController.Instance.DisplayObject(itemSearchUI.gameObject,keyCodes:ControlUtils.GetKeyCodes(PlayerControl.OpenSearch),blocker:false,blockMovement:false);
            }
            searchIndicator.Initialize(PlayerControl.OpenSearch, ()=> "Search Items", OnSearchClick);
            
        }

        public void Initialize(PlayerScript playerScript)
        {
            conduitPortIndicatorUI?.Display(playerScript);
            conduitViewIndicatorUI?.Display(playerScript);
            conduitPlacementModeIndicatorUI?.Display(playerScript.ConduitPlacementOptions);
            tilePlacementIndicatorUI.Initialize(playerScript);
            tilePreviewerIndicatorUI.Display(playerScript);
            loadOutIndicator.Initialize(playerScript);
            autoSelectIndicator.Initialize(playerScript.PlayerMouse);

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
            searchIndicator.gameObject.SetActive(true);

            bool ViewBundleActive(IndicatorDisplayBundle bundle)
            {
                return (viewMode & (int)bundle) != 0;
            }

            if (ViewBundleActive(IndicatorDisplayBundle.TilePlace))
            {
                tilePreviewerIndicatorUI.gameObject.SetActive(true);
                tilePlacementIndicatorUI.gameObject.SetActive(true);
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

            if (ViewBundleActive(IndicatorDisplayBundle.AutoSelect))
            {
                autoSelectIndicator.gameObject.SetActive(true);
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

                if (keyCodeIndicator is IOptionalKeyCodeIndicator optionalKeyCodeIndicator)
                {
                    text += $"\n{optionalKeyCodeIndicator.GetOptionalKeyCode()}";
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
