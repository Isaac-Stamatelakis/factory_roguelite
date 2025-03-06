using System.Collections.Generic;
using Player;
using UnityEngine;

namespace UI.Catalogue.InfoViewer
{
    public interface IStageRestrictedCatalogueElement
    {
        public void Filter(PlayerGameStageCollection gameStageCollection);
    }
    public interface ICatalogueElement
    {
        public abstract string GetName();
        public abstract Sprite GetSprite();
        public string GetPageIndicatorString(int pageIndex);
        public int GetPageCount();
        public void DisplayAllElements(PlayerGameStageCollection gameStageCollection);
    }

    public interface IColorableCatalogueElement
    {
        public Color GetColor();
    }

    public interface ICatalogueElementPage
    {
        
    }
    
}
