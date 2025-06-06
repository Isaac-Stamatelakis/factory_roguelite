using System.Collections.Generic;
using Item.Burnables;
using Items;
using Player;
using UnityEngine;

namespace UI.Catalogue.InfoViewer
{
    public interface ICatalogueElement
    {
        public abstract string GetName();
        public abstract ItemObject GetDisplayItem();
        public string GetPageIndicatorString(int pageIndex);
        public int GetPageCount();
        public void DisplayAllElements(PlayerGameStageCollection gameStageCollection);
        public bool Filter(PlayerGameStageCollection gameStageCollection);
    }

    public interface IColorableCatalogueElement
    {
        public Color GetColor();
    }

    public interface ICatalogueElementPage
    {
        
    }
    
}
