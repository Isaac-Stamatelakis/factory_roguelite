using System.Collections.Generic;
using Item.ItemObjects.Instances.Tile.Chisel;
using Items;
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

        public void DisplayAllElements()
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
            CatalogueInfoUtils.DisplayCatalogue(new List<CatalogueElementData>{catalogueElementData});
        }
    }

    public class ChiselDisplayData
    {
        public ChiselCollectionObject ChiselCollectionObject;

        public ChiselDisplayData(ChiselCollectionObject chiselCollectionObject)
        {
            ChiselCollectionObject = chiselCollectionObject;
        }
    }
}
