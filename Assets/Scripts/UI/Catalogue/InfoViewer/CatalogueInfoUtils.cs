using System.Collections.Generic;
using Item.Burnables;
using Item.Slot;
using Item.Transmutation.Info;
using Items.Transmutable;
using Recipe;
using Recipe.Viewer;
using UnityEngine;
using LinqUtility = Unity.VisualScripting.LinqUtility;

namespace UI.Catalogue.InfoViewer
{
    public static class CatalogueInfoUtils
    {
        public static void DisplayItemInformation(ItemSlot itemSlot)
        {
            var elements = new List<CatalogueElementData>();
            var recipeToCreate = RecipeViewerHelper.GetRecipesForItem(itemSlot);
            elements.AddRange(recipeToCreate);

            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                TransmutationMaterialInfo materialInfo = new TransmutationMaterialInfo(transmutableItemObject.getMaterial());
                elements.Add(new CatalogueElementData(materialInfo,CatalogueInfoDisplayType.TransmutableMaterial));
            }
            DisplayCatalogue(elements);

        }

        public static void DisplayItemUses(ItemSlot itemSlot)
        {
            var elements = RecipeViewerHelper.GetRecipesWithItem(itemSlot);
            uint burnTime = RecipeRegistry.BurnableItemRegistry.GetBurnDuration(itemSlot.itemObject);
            if (burnTime > 0)
            {
                BurnableItemDisplay burnableItemDisplay = new BurnableItemDisplay(itemSlot);
                BurnableInfo burnableInfo = new BurnableInfo(new List<BurnableDisplay>{burnableItemDisplay});
                elements.Add(new CatalogueElementData(burnableInfo,CatalogueInfoDisplayType.Burnable));
            }
            DisplayCatalogue(elements);
        }

        public static void DisplayCatalogue(List<CatalogueElementData> elements)
        {
            if (elements.Count == 0) return;
            MainCanvasController mainCanvasController = MainCanvasController.TInstance;
            if (mainCanvasController.TopHasComponent<CatalogueInfoViewer>())
            {
                mainCanvasController.PopStack();
            }
            CatalogueInfoViewer catalogueInfoViewer = mainCanvasController.DisplayUIElement<CatalogueInfoViewer>(MainSceneUIElement.CatalogueInfo);
            catalogueInfoViewer.Initialize(elements);
        }
    }
}
