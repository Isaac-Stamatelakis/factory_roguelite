using System.Collections.Generic;
using Item.Burnables;
using Item.GameStage;
using Item.ItemObjects.Instances.Tile.Chisel;
using Items;
using Player;
using UI.Catalogue.InfoViewer;
using UnityEngine;

namespace Item.ItemObjects.Instances.Tiles.Chisel
{
    public class ChiselCatalogueInfo : ICatalogueElement
    {
        public List<ChiselDisplayData> DisplayDataList;

        public ChiselCatalogueInfo(List<ChiselDisplayData> displayDataList)
        {
            DisplayDataList = displayDataList;
        }

        public string GetName()
        {
            return "Chisel Info";
        }

        public ItemObject GetDisplayItem()
        {
            return null;
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            return $"Collection {pageIndex+1}/{DisplayDataList.Count}";
        }

        public int GetPageCount()
        {
            return DisplayDataList.Count;
        }

        public void DisplayAllElements(PlayerGameStageCollection gameStageCollection)
        {
            HashSet<ChiselCollectionObject> chiselCollectionObjects = new HashSet<ChiselCollectionObject>();
            foreach (ItemObject item in ItemRegistry.GetInstance().GetAllItems())
            {
                if (item is not ChiselTileItem chiselTileItem) continue;
                chiselCollectionObjects.Add(chiselTileItem.Collection);
            }

            List<ChiselDisplayData> chiselDisplayDataList = new List<ChiselDisplayData>();
            foreach (ChiselCollectionObject chiselCollectionObject in chiselCollectionObjects)
            {
                chiselDisplayDataList.Add(new ChiselDisplayData(chiselCollectionObject));
            }
            ChiselCatalogueInfo chiselCatalogueInfo = new ChiselCatalogueInfo(chiselDisplayDataList);
            CatalogueElementData catalogueElementData = new CatalogueElementData(chiselCatalogueInfo, CatalogueInfoDisplayType.Chisel);
            CatalogueInfoUtils.DisplayCatalogue(new List<CatalogueElementData>{catalogueElementData},gameStageCollection);
        }

        public bool Filter(PlayerGameStageCollection gameStageCollection)
        {
            for (int i = DisplayDataList.Count - 1; i >= 0; i--)
            {
                ChiselDisplayData chiselDisplayData = DisplayDataList[i];
                CatalogueInfoUtils.FilterList(chiselDisplayData.ChiselTiles,gameStageCollection);
                if (chiselDisplayData.ChiselTiles.Count > 0) continue;
                DisplayDataList.RemoveAt(i);
            }

            return DisplayDataList.Count > 0;
        }
    }

    public class ChiselItemDisplay : IGameStageItemDisplay
    {
        public ChiselTileItem ChiselTileItem;
        public ChiselItemDisplay(ChiselTileItem chiselTileItem)
        {
            ChiselTileItem = chiselTileItem;
        }

        public bool FilterStage(PlayerGameStageCollection gameStageCollection)
        {
            return gameStageCollection.HasStage(ChiselTileItem?.gameStage);
        }
    }
    public class ChiselDisplayData
    {
        public List<ChiselItemDisplay> ChiselTiles;
        public string CollectionName;
        public ChiselDisplayData(ChiselCollectionObject chiselCollectionObject)
        {
            ChiselTiles = new List<ChiselItemDisplay>();
            CollectionName = chiselCollectionObject.name;
            foreach (ChiselTileItem chiselTileItem in chiselCollectionObject.ChiselTiles)
            {
                ChiselTiles.Add(new ChiselItemDisplay(chiselTileItem));
            }
            
        }
    }
}
