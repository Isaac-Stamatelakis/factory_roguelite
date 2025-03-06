using Player;
using UnityEngine;

namespace UI.Catalogue.InfoViewer
{
    public interface ICatalogueElementUI
    {
        public void Display(ICatalogueElement element, PlayerGameStageCollection gameStages);
        public void DisplayPage(int pageIndex);
    }
    public abstract class CatalogueInfoUI : MonoBehaviour, ICatalogueElementUI
    {
        public abstract void Display(ICatalogueElement element, PlayerGameStageCollection gameStages);
        public abstract void DisplayPage(int pageIndex);
    }
}
