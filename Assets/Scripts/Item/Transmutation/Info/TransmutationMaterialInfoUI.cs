using System;
using Items;
using Items.Transmutable;
using Player;
using UI.Catalogue.InfoViewer;
using UnityEngine;

namespace Item.Transmutation.Info
{
    internal enum MaterialPage
    {
        Info,
        Mining,
        Processing
    }

    public interface ITransmutationMaterialPageUI
    {
        public void Display(TransmutationMaterialInfo materialInfo);
    }
    public class TransmutationMaterialInfoUI : CatalogueInfoUI
    {
        private GameObject currentPage;
        [SerializeField] private TransmutationMaterialPageUI infoPagePrefab;
        private TransmutationMaterialInfo materialInfo;
        public override void Display(ICatalogueElement element, PlayerGameStageCollection gameStages)
        {
            materialInfo = (TransmutationMaterialInfo)element;
            DisplayPage(0);
        }

        public override void DisplayPage(int pageIndex)
        {
            if (!ReferenceEquals(currentPage,null)) GameObject.Destroy(currentPage);
            MaterialPage page = (MaterialPage)pageIndex;
            GameObject prefab = GetPrefab(page);
            currentPage = Instantiate(prefab,transform);
            currentPage.GetComponent<ITransmutationMaterialPageUI>().Display(materialInfo);
        }

        

        private GameObject GetPrefab(MaterialPage page)
        {
            switch (page)
            {
                case MaterialPage.Info:
                    return infoPagePrefab.gameObject;
                case MaterialPage.Mining:
                    break;
                case MaterialPage.Processing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }

            return null;
        }
        
        
    }

    public class TransmutationMaterialInfo : ICatalogueElement
    {
        public TransmutableItemMaterial Material;
        public string GetName()
        {
            return Material.name + " Info";
        }

        public ItemObject GetDisplayItem()
        {
            var baseState = Material.MaterialOptions.BaseState;
            return TransmutableItemUtils.GetMaterialItem(Material, baseState);
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            MaterialPage page = (MaterialPage)pageIndex;
            return page.ToString();
        }

        public int GetPageCount()
        {
            return 3;
        }

        public void DisplayAllElements(PlayerGameStageCollection gameStageCollection)
        {
            // Do nothing for now. Maybe show all materials ?
        }

        public bool Filter(PlayerGameStageCollection gameStageCollection)
        {
            return gameStageCollection.HasStage(Material?.gameStageObject);
        }
        

        public TransmutationMaterialInfo(TransmutableItemMaterial material)
        {
            Material = material;
        }
    }
}
