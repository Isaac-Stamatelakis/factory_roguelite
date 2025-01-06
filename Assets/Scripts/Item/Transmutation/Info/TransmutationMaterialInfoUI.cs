using System;
using Items;
using Items.Transmutable;
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
        public override void Display(ICatalogueElement element)
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

    public class TransmutationMaterialInfo : ICatalogueElement, IColorableCatalogueElement
    {
        public TransmutableItemMaterial Material;
        public string GetName()
        {
            return Material.name + " Info";
        }

        public Sprite GetSprite()
        {
            var baseState = Material.MaterialOptions.BaseState;
            ItemObject itemObject = TransmutableItemUtils.GetMaterialItem(Material, baseState);
            return itemObject.getSprite();
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

        public void DisplayAllElements()
        {
            // Do nothing for now. Maybe show all materials ?
        }

        public Color GetColor()
        {
            return Material.color;
        }

        public TransmutationMaterialInfo(TransmutableItemMaterial material)
        {
            Material = material;
        }
    }
}
