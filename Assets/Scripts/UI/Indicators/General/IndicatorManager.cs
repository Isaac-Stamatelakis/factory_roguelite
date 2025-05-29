using System.Collections.Generic;
using Chunks.Systems;
using Conduits.Systems;
using Dimensions;
using Player;
using Player.Controls;
using TMPro;
using UI.Catalogue.ItemSearch;
using UI.QuestBook;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Indicators.General
{
    public class IndicatorManager : BaseIndiciatorManagerUI
    {
        public ConduitPortIndicatorUI conduitPortIndicatorUI;
        public ConduitViewIndicatorUI conduitViewIndicatorUI;
       
        public GenericIndicatorUI questBookIndicator;
        public GenericIndicatorUI inventoryIndicator;
        public RobotLoadOutIndicator loadOutIndicator;
        public GenericIndicatorUI searchIndicator;
        public CaveIndicatorUI caveIndicatorUI;
        public TileAutoSelectIndicatorUI autoSelectIndicator;
        public TileHighligherIndicatorUI tilePreviewerIndicatorUI;
        public void Start()
        {
            
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
                CanvasController.Instance.DisplayObject(itemSearchUI.gameObject,keyCodes:PlayerControl.OpenSearch,
                    blocker:false,
                    blockMovement:false
                );
            }
            searchIndicator.Initialize(PlayerControl.OpenSearch, ()=> "Search Items", OnSearchClick);
            
        }

        public void Initialize(PlayerScript playerScript)
        {
            conduitPortIndicatorUI?.Display(playerScript);
            conduitViewIndicatorUI?.Display(playerScript);
            
            loadOutIndicator.Initialize(playerScript);
            autoSelectIndicator.Initialize(playerScript.PlayerMouse);
            tilePreviewerIndicatorUI.Display(playerScript);
        }

        
        public void Display(PlayerScript playerScript)
        {
            questBookIndicator.gameObject.SetActive(true);
            inventoryIndicator.gameObject.SetActive(true);
            loadOutIndicator.gameObject.SetActive(true);
            searchIndicator.gameObject.SetActive(true);
            tilePreviewerIndicatorUI.gameObject.SetActive(true);
            
            bool caveSystem = playerScript.CurrentSystem.Dim == (int)Dimension.Cave;
            caveIndicatorUI.gameObject.SetActive(caveSystem);
            
            bool conduitSystem = !caveSystem;
            conduitViewIndicatorUI.gameObject.SetActive(conduitSystem);
            conduitPortIndicatorUI.gameObject.SetActive(conduitSystem);
            autoSelectIndicator.gameObject.SetActive(true);
            
            SyncKeyCodes(true);
        }
    }
}
