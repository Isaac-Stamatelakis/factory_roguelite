using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixRecipeCollection
    {
        Dictionary<string, Dictionary<ItemTagKey, List<(EncodedRecipe,MatrixInterface)>>> recipes;
        public int Count {get=> recipes.Count;}
        
        public MatrixRecipeCollection() {
            this.recipes = new Dictionary<string, Dictionary<ItemTagKey, List<(EncodedRecipe, MatrixInterface)>>>();
        }
        public void removeInterface(MatrixInterface matrixInterface) {
            foreach (EncodedRecipe encodedRecipe in matrixInterface.getRecipes()) {
                removeRecipe(encodedRecipe,matrixInterface);
            }
        }

        public void addInterface(MatrixInterface matrixInterface) {
            foreach (EncodedRecipe encodedRecipe in matrixInterface.getRecipes()) {
                addRecipe(encodedRecipe,matrixInterface);
            }
        }


        public List<(string,ItemTagKey,EncodedRecipe)> toList() {
            List<(string,ItemTagKey,EncodedRecipe)> idTagRecipeList = new List<(string, ItemTagKey, EncodedRecipe)>();
            foreach (KeyValuePair<string, Dictionary<ItemTagKey, List<(EncodedRecipe,MatrixInterface)>>> idTagRecipeListDict in recipes) {
                foreach (KeyValuePair<ItemTagKey, List<(EncodedRecipe,MatrixInterface)>> tagRecipeList in idTagRecipeListDict.Value) {
                    List<(EncodedRecipe,MatrixInterface)> list = tagRecipeList.Value;
                    if (list.Count > 0) {
                        idTagRecipeList.Add((idTagRecipeListDict.Key,tagRecipeList.Key,list[0].Item1));
                    }
                }
            }
            return idTagRecipeList;
        }
        public int getIndexByPriority(List<(EncodedRecipe, MatrixInterface)> recipeInterfaceList, MatrixInterface matrixInterface)
        {
            MatrixInterfacePriorityComparer comparer = new MatrixInterfacePriorityComparer();
            int insertionIndex = recipeInterfaceList.BinarySearch((null, matrixInterface), comparer);

            if (insertionIndex < 0)
            {
                insertionIndex = ~insertionIndex; // Convert to the actual index if not found
            }

            return insertionIndex;
        }

        private void addRecipeOutput(MatrixInterface matrixInterface, string id, ItemTagKey itemTagKey, EncodedRecipe encodedRecipe) {
            if (!recipes.ContainsKey(id)) {
                recipes[id] = new Dictionary<ItemTagKey, List<(EncodedRecipe, MatrixInterface)>>();
            }
            if (!recipes[id].ContainsKey(itemTagKey)) {
                recipes[id][itemTagKey] = new List<(EncodedRecipe, MatrixInterface)> {
                    (encodedRecipe,matrixInterface)
                };
                return;
            }
            List<(EncodedRecipe,MatrixInterface)> recipeInterfaceList = recipes[id][itemTagKey];
            int insertionIndex = getIndexByPriority(recipeInterfaceList,matrixInterface);
            recipeInterfaceList.Insert(insertionIndex,(encodedRecipe,matrixInterface));
        }
        public void addRecipe(EncodedRecipe encodedRecipe,MatrixInterface matrixInterface) {
            if (encodedRecipe == null) {
                return;
            }
            foreach (ItemSlot output in encodedRecipe.Outputs) {
                if (output == null || output.itemObject == null) {
                    continue;
                }
                string id = output.itemObject.id;
                ItemTagKey key = new ItemTagKey(output.tags);
                addRecipeOutput(matrixInterface,id,key,encodedRecipe);
            }
        }

        public void removeRecipe(EncodedRecipe encodedRecipe, MatrixInterface matrixInterface) {
            foreach (ItemSlot output in encodedRecipe.Outputs) {
                if (output == null || output.itemObject == null) {
                    continue;
                }
                string id = output.itemObject.id;
                if (!recipes.ContainsKey(id)) {
                    continue;
                }
                ItemTagKey key = new ItemTagKey(output.tags);
                if (!recipes[id].ContainsKey(key)) {
                    continue;
                }
                removeRecipeOutput(matrixInterface,id,key);
            }
        }
        private void removeRecipeOutput(MatrixInterface matrixInterface, string id, ItemTagKey itemTagKey) {
            List<(EncodedRecipe,MatrixInterface)> recipeInterfaceList = recipes[id][itemTagKey];
            for (int i = 0; i < recipeInterfaceList.Count; i++) {
                (EncodedRecipe, MatrixInterface) value = recipeInterfaceList[i];
                if (value.Item2.Equals(matrixInterface)) {
                    recipeInterfaceList.RemoveAt(i);
                    break;
                }
            }
            if (recipeInterfaceList.Count == 0) {
                recipes[id].Remove(itemTagKey);
            }
            if (recipes[id].Count == 0) {
                recipes.Remove(id);
            }
        }

        public EncodedRecipe getRecipe(string id, ItemTagKey itemTagKey) {
            // O(1) lookup
            if (!recipes.ContainsKey(id) || !recipes[id].ContainsKey(itemTagKey)) {
                return null;
            }
            List<(EncodedRecipe,MatrixInterface)> list = recipes[id][itemTagKey];
            if (list.Count < 1) {
                return null;
            }
            return list[0].Item1;
        }
        public EncodedRecipe getRecipe(ItemSlot itemSlot) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return null;
            }
            return getRecipe(itemSlot.itemObject.id,new ItemTagKey(itemSlot.tags));
        }

        public (EncodedRecipe, MatrixInterface) getRecipeAndInterface(string id, ItemTagKey itemTagKey) {
            if (!recipes.ContainsKey(id) || !recipes[id].ContainsKey(itemTagKey)) {
                return (null,null);
            }
            List<(EncodedRecipe,MatrixInterface)> list = recipes[id][itemTagKey];
            if (list.Count < 1) {
                return (null,null);
            }
            return list[0];
        }

        public (EncodedRecipe, MatrixInterface) getRecipeAndInterface(ItemSlot itemSlot) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return (null,null);
            }
            return getRecipeAndInterface(itemSlot.itemObject.id,new ItemTagKey(itemSlot.tags));
        }

        private class MatrixInterfacePriorityComparer : IComparer<(EncodedRecipe, MatrixInterface)>
        {
            public int Compare((EncodedRecipe, MatrixInterface) x, (EncodedRecipe, MatrixInterface) y)
            {
                return x.Item2.Priority.CompareTo(y.Item2.Priority);
            }
        }
    }
}

