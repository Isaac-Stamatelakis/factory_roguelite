using System;
using System.Collections.Generic;
using Item.GameStage;
using Item.Transmutation.Info;
using Recipe.Viewer;
using UI.Catalogue.InfoViewer.Indicator;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Catalogue.InfoViewer
{
    public enum CatalogueInfoDisplayType
    {
        Recipe,
        TransmutableMaterial
    }
    public class CatalogueInfoViewer : MonoBehaviour
    {
        [SerializeField] private CatalogueElementIndicator elementIndicator;
        [SerializeField] private CatalogueNavigator elementNavigator;
        [SerializeField] private CatalogueNavigator pageNavigator;
        [SerializeField] private VerticalLayoutGroup contentList;
        [SerializeField] private ProcessorInfoPageUI processorUIPrefab;
        [SerializeField] private TransmutationMaterialInfoUI materialInfoUI;
        public List<CatalogueElementData> CatalogueElements;
        private CatalogueInfoUI displayedElementUI;
        private int currentElementIndex;
        private int currentPageIndex;

        public void Initialize(List<CatalogueElementData> elements)
        {
            CatalogueElements = elements;
            elementIndicator.Initialize(this);
            DisplayCurrentElement();
            elementNavigator.Initialize(
                CatalogueElements[currentElementIndex].CatalogueElement.GetName(),
                () => { MoveDisplayElement(-1); },
                () => { MoveDisplayElement(1); }
            );
            pageNavigator.Initialize(
                elements[0].CatalogueElement.GetPageIndicatorString(0),
                () => MovePage(-1), 
                () => MovePage(1)
            );
            
        }
        
        /// <summary>
        /// Returns the element adjacent to the currently displayed element
        /// </summary>
        /// <param name="offset">Offset from current displayed element</param>
        /// <returns></returns>
        public ICatalogueElement GetAdjacentDisplayElement(int offset)
        {
            int elementIndex = ((currentElementIndex + offset) % CatalogueElements.Count + CatalogueElements.Count) % CatalogueElements.Count;
            return CatalogueElements[elementIndex].CatalogueElement;
        }

        public void MoveDisplayElement(int offset)
        {
            currentElementIndex += offset;
            currentElementIndex = (currentElementIndex % CatalogueElements.Count + CatalogueElements.Count) % CatalogueElements.Count;
            DisplayCurrentElement();
        }
        

        public void DisplayCurrentElement()
        {
            if (!ReferenceEquals(displayedElementUI, null)) Destroy(displayedElementUI.gameObject);
            
            CatalogueElementData catalogueElementData = CatalogueElements[currentElementIndex];
            displayedElementUI = InstantiateElementUI(catalogueElementData.Type);
            displayedElementUI.Display(CatalogueElements[currentElementIndex].CatalogueElement);
            elementIndicator.DisplayNodes();
            elementNavigator.SetText(catalogueElementData.CatalogueElement.GetName());
        }

        private CatalogueInfoUI InstantiateElementUI(CatalogueInfoDisplayType type)
        {
            CatalogueInfoUI prefab = GetPrefab(type);
            return Instantiate(prefab, contentList.transform);
        }

        private CatalogueInfoUI GetPrefab(CatalogueInfoDisplayType type)
        {
            switch (type)
            {
                case CatalogueInfoDisplayType.Recipe:
                    return processorUIPrefab;
                case CatalogueInfoDisplayType.TransmutableMaterial:
                    return materialInfoUI;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        

        private void MovePage(int offset)
        {
            var current = CatalogueElements[currentElementIndex];
            int count = current.CatalogueElement.GetPageCount();
            currentPageIndex += offset;
            currentPageIndex = (currentPageIndex % count +count) % count;
            displayedElementUI.DisplayPage(currentPageIndex);
            pageNavigator.SetText(current.CatalogueElement.GetPageIndicatorString(currentPageIndex));
        }
    }

    public class CatalogueElementData
    {
        public readonly ICatalogueElement CatalogueElement;
        public readonly CatalogueInfoDisplayType Type;

        public CatalogueElementData(ICatalogueElement catalogueElement, CatalogueInfoDisplayType type)
        {
            CatalogueElement = catalogueElement;
            Type = type;
        }
    }
}
