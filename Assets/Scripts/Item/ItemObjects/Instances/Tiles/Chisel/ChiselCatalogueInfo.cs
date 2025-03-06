using System.Collections.Generic;
using Item.ItemObjects.Instances.Tile.Chisel;
using Items;
using Player;
using UI.Catalogue.InfoViewer;
using UnityEngine;

namespace Item.ItemObjects.Instances.Tiles.Chisel
{
    public class ChiselCatalogueInfo : ICatalogueElement, IStageRestrictedCatalogueElement
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

        public Sprite GetSprite()
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

        public void Filter(PlayerGameStageCollection gameStageCollection)
        {
            for (int i = DisplayDataList.Count - 1; i >= 0; i--)
            {
                ChiselDisplayData chiselDisplayData = DisplayDataList[i];
                for (int j = 0; j < chiselDisplayData.ChiselTiles.Count; j++)
                {
                    ChiselTileItem chiselTileItem = chiselDisplayData.ChiselTiles[j];
                    if (gameStageCollection.HasStage(chiselTileItem.gameStage.GetGameStageId())) continue;
                    chiselDisplayData.ChiselTiles.RemoveAt(j);
                }
                if (chiselDisplayData.ChiselTiles.Count > 0) continue;
                DisplayDataList.RemoveAt(i);
            }
        }
    }

    public class ChiselDisplayData
    {
        public List<ChiselTileItem> ChiselTiles;
        public string CollectionName;
        public ChiselDisplayData(ChiselCollectionObject chiselCollectionObject)
        {
            ChiselTiles = new List<ChiselTileItem>();
            CollectionName = chiselCollectionObject.name;
            foreach (ChiselTileItem chiselTileItem in chiselCollectionObject.ChiselTiles)
            {
                ChiselTiles.Add(chiselTileItem);
            }
            
        }
    }
}
