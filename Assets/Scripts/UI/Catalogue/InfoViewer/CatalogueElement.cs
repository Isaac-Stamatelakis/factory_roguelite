using System.Collections.Generic;
using UnityEngine;

namespace UI.Catalogue.InfoViewer
{
    public interface ICatalogueElement
    {
        public abstract string GetName();
        public abstract Sprite GetSprite();
        public string GetPageIndicatorString(int pageIndex);
        public int GetPageCount();
    }

    public interface IColorableCatalogueElement
    {
        public Color GetColor();
    }

    public interface ICatalogueElementPage
    {
        
    }
    
}
