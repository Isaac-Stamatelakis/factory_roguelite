using UnityEngine;

namespace UI.Catalogue.InfoViewer
{
    public interface ICatalogueElementUI
    {
        public void Display(ICatalogueElement element);
        public void DisplayPage(int pageIndex);
    }
    public abstract class CatalogueInfoUI : MonoBehaviour, ICatalogueElementUI
    {
        public abstract void Display(ICatalogueElement element);
        public abstract void DisplayPage(int pageIndex);
    }
}
