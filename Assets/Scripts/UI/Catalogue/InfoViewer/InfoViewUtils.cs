using System.Collections.Generic;
using Item.Slot;
using Items.Transmutable;
using Recipe.Viewer;
using UnityEngine;
using LinqUtility = Unity.VisualScripting.LinqUtility;

namespace UI.Catalogue.InfoViewer
{
    public static class InfoViewUtils
    {
        public static void DisplayItemInformation(ItemSlot itemSlot)
        {
            var elements = new List<CatalogueElementData>();
            var recipeToCreate = RecipeViewerHelper.GetRecipesOfItem(itemSlot);
            elements.AddRange(recipeToCreate);

            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                // TODO material info if transmutation
            }
            DisplayCatalogue(elements);

        }

        public static void DisplayItemUses(ItemSlot itemSlot)
        {
            var elements = new List<CatalogueElementData>();
            var recipesWith = RecipeViewerHelper.GetRecipesWithItem(itemSlot);
            DisplayCatalogue(recipesWith);
        }

        private static void DisplayCatalogue(List<CatalogueElementData> elements)
        {
            if (elements.Count == 0) return;
            
            CatalogueInfoViewer catalogueInfoViewer = MainCanvasController.TInstance.DisplayUIElement<CatalogueInfoViewer>(MainSceneUIElement.CatalogueInfo);
            catalogueInfoViewer.Initialize(elements);
        }
    }
}
