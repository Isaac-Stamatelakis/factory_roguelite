using System;
using System.Collections.Generic;
using Recipe.Viewer;
using UI.PauseScreen;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public enum MainSceneUIElement
    {
        PauseScreen,
        RecipeViewer
    }
    public class MainCanvasController : CanvasController
    {
        public static MainCanvasController TInstance => instance as MainCanvasController; 
        [SerializeField] private PauseScreenUI pauseScreenUIPrefab;
        [SerializeField] private RecipeViewer recipeViewerPrefab;
        public override void EmptyListen()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DisplayObject(Instantiate(pauseScreenUIPrefab.gameObject));
            }
        }

        public override void ListenKeyPresses()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PopStack();
            }
        }

        public T DisplayUIElement<T>(MainSceneUIElement mainSceneUIElement)
        {
            GameObject prefab = GetMainSceneUIElement(mainSceneUIElement);
            GameObject clone = Instantiate(prefab);
            T element = clone.GetComponent<T>();
            if (element == null)
            {
                Debug.LogWarning($"Tried to display UI element of type {mainSceneUIElement} which doesn't contain component of type {typeof(T)}");
                Destroy(clone);
                return default;
            }
            DisplayObject(clone);
            return element;
        }

        private GameObject GetMainSceneUIElement(MainSceneUIElement mainSceneUIElement)
        {
            return mainSceneUIElement switch
            {
                MainSceneUIElement.PauseScreen => Instantiate(pauseScreenUIPrefab).gameObject,
                MainSceneUIElement.RecipeViewer => Instantiate(recipeViewerPrefab).gameObject,
                _ => throw new ArgumentOutOfRangeException(nameof(mainSceneUIElement), mainSceneUIElement, null)
            };
        }
    }
}
