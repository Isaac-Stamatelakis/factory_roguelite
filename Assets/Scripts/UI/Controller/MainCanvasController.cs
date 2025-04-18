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
        public override void EmptyListen()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DisplayObject(Instantiate(pauseScreenUIPrefab.gameObject));
            }
        }

        public override void ListenKeyPresses()
        {
            if (CanEscapePop())
            {
                PopStack();
            }
        }

        public Canvas GetCanvas()
        {
            return canvas;
        }
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

        public void DisplayUIWithPlayerInventory(GameObject uiObject, Color? color = null)
        {
            var stackedPlayerUI = Instantiate(stackedPlayerInvUIElementPrefab);
            
            stackedPlayerUI.DisplayWithPlayerInventory(uiObject,false);
                                         DisplayObject(stackedPlayerUI.gameObject);
            if (color != null)
            {
                stackedPlayerUI.SetBackgroundColor((Color)color);
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
    }
}
