using System.Collections.Generic;
using Items;
using Player;
using UI.Catalogue.InfoViewer;
using UnityEngine;
using UnityEngine.UI;
using World.Cave.Registry;

namespace World.Cave.InfoUI
{
    public class CaveInfoUI : CatalogueInfoUI
    {
        [SerializeField] private CaveTileInfoElementUI mCaveTileInfoPrefab;
        [SerializeField] private VerticalLayoutGroup mList;
        public const int ELEMENTS_PER_PAGE = 7;
        private CaveInfoCatalogueElement caveInfo;
        
        public override void Display(ICatalogueElement element, PlayerGameStageCollection gameStages)
        {
            if (element is not CaveInfoCatalogueElement caveInfoCatalogueElement) return;
            caveInfo = caveInfoCatalogueElement;
            DisplayPage(0);
            
        }

        public override void DisplayPage(int pageIndex)
        {
            GlobalHelper.DeleteAllChildren(mList.transform);
            int startIndex = pageIndex * ELEMENTS_PER_PAGE;
            List<CaveTileInfoElement> tileInfoElements = caveInfo.TileInfoElements;
            for (int i = startIndex; i < ELEMENTS_PER_PAGE; i++)
            {
                if (i >= tileInfoElements.Count) break;
                CaveTileInfoElement caveTileInfoElement = tileInfoElements[i];
                CaveTileInfoElementUI tileInfoElementUI = Instantiate(mCaveTileInfoPrefab, mList.transform);
                tileInfoElementUI.Display(caveTileInfoElement);
            }
        }
    }

    public class CaveInfoCatalogueElement : ICatalogueElement
    {
        public CaveInfoCatalogueElement(string caveName, string baseId,List<CaveTileInfoElement> tileInfoElements)
        {
            this.TileInfoElements = tileInfoElements;
            this.caveName = caveName;
            this.baseId = baseId;
        }

        
        private string caveName;
        private string baseId;
        public List<CaveTileInfoElement> TileInfoElements;
        public string GetName()
        {
            return caveName;
        }

        public Sprite GetSprite()
        {
            return ItemRegistry.GetInstance().GetItemObject(baseId)?.getSprite();
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            return $"Tiles {pageIndex+1}/{GetPageCount()}";
        }

        public int GetPageCount()
        {
            return (TileInfoElements.Count-1) / CaveInfoUI.ELEMENTS_PER_PAGE + 1;
        }

        public void DisplayAllElements(PlayerGameStageCollection gameStageCollection)
        {
            CaveRegistry caveRegistry = CaveRegistry.Instance;
            if (!caveRegistry) return;
            List<CaveInfoCatalogueElement> caveInfoCatalogueElements = caveRegistry.GetInfoOfAllCaves();
            List<CatalogueElementData> catalogueElementDataList = new List<CatalogueElementData>();
            foreach (CaveInfoCatalogueElement caveInfoCatalogueElement in caveInfoCatalogueElements)
            {
                catalogueElementDataList.Add(new CatalogueElementData(caveInfoCatalogueElement,CatalogueInfoDisplayType.CaveTile));
            }
            CatalogueInfoUtils.DisplayCatalogue(catalogueElementDataList,gameStageCollection);
        }

        public bool Filter(PlayerGameStageCollection gameStageCollection)
        {
            return true; // No filtering
        }
    }

    public struct CaveTileInfoElement
    {
        public string Id;
        public float Chance;
    }
}
