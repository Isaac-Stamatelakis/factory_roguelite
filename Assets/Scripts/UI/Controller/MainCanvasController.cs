using System;
using System.Collections.Generic;
using Conduit.Port.UI;
using Item.ItemObjects.Instances.Tiles.Chisel;
using Recipe.Viewer;
using UI.Catalogue.InfoViewer;
using UI.JEI;
using UI.PauseScreen;
using UI.PlayerInvUI;
using UI.QuestBook;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum MainSceneUIElement
    {
        PauseScreen,
        IOPortViewer,
        CatalogueInfo,
    }
    public class MainCanvasController : CanvasController
    {
        public static MainCanvasController TInstance => instance as MainCanvasController;
        [SerializeField] private PauseScreenUI pauseScreenUIPrefab;
        [SerializeField] private StackedPlayerInvUIElement stackedPlayerInvUIElementPrefab;
        [SerializeField] private IOConduitPortUI ioConduitPortUIPrefab;
        [SerializeField] private CatalogueInfoViewer catalogueInfoViewerPrefab;
        [SerializeField] private Canvas canvas;
        [SerializeField] private ItemCatalogueController mItemCatalogueController;
        public QuestBookUIManager QuestBookUIManager;
        
        public T DisplayUIElement<T>(MainSceneUIElement mainSceneUIElement)
        {
            GameObject clone = GetMainSceneUIElement(mainSceneUIElement);
            T element = clone.GetComponent<T>();
            if (element == null)
            {
                Debug.LogWarning($"Tried to display UI element of type {mainSceneUIElement} which doesn't contain component of type {typeof(T)}");
                Destroy(clone);
                return default;
            }

            switch (mainSceneUIElement)
            {
                case MainSceneUIElement.IOPortViewer:
                    DisplayUIWithPlayerInventory(clone);
                    break;
                default:
                    DisplayObject(clone);
                    break;
            }
            
            return element;
        }

        public void DisplayUIWithPlayerInventory(GameObject uiObject)
        {
            var stackedPlayerUI = Instantiate(stackedPlayerInvUIElementPrefab);
            
            stackedPlayerUI.DisplayWithPlayerInventory(uiObject,false);
            DisplayObject(stackedPlayerUI.gameObject);
        }
        
        public void DisplayUIWithPlayerInventory(GameObject uiObject, DisplayedUIInfo displayedUIInfo)
        {
            var stackedPlayerUI = Instantiate(stackedPlayerInvUIElementPrefab);
            
            stackedPlayerUI.DisplayWithPlayerInventory(uiObject,false);
            DisplayObject(stackedPlayerUI.gameObject);
            Image image = stackedPlayerUI.GetComponent<Image>();
            
            if (displayedUIInfo.Color.HasValue)
            {
                image.color = displayedUIInfo.Color.Value;
            }

            if (displayedUIInfo.UIMaterial)
            {
                image.material = displayedUIInfo.UIMaterial;
            }
        }

        public GameObject GetMainSceneUIElement(MainSceneUIElement mainSceneUIElement)
        {
            return mainSceneUIElement switch
            {
                MainSceneUIElement.PauseScreen => Instantiate(pauseScreenUIPrefab).gameObject,
                MainSceneUIElement.IOPortViewer => Instantiate(ioConduitPortUIPrefab).gameObject,
                MainSceneUIElement.CatalogueInfo => Instantiate(catalogueInfoViewerPrefab).gameObject,
                _ => throw new ArgumentOutOfRangeException(nameof(mainSceneUIElement), mainSceneUIElement, null)
            };
        }

        protected override void OnInactiveEscapePress()
        {
            DisplayObject(Instantiate(pauseScreenUIPrefab.gameObject));
        }

        protected override void OnEscapePress()
        {
            PopStack();
        }
    }
}
