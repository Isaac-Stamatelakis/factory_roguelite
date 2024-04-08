using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                    removeRecipe(matrixInterface,id,key);
                }
            }
        }

        public void addInterface(MatrixInterface matrixInterface) {
            foreach (EncodedRecipe encodedRecipe in matrixInterface.getRecipes()) {
                if (encodedRecipe == null) {
                    continue;
                }
                foreach (ItemSlot output in encodedRecipe.Outputs) {
                    if (output == null || output.itemObject == null) {
                        continue;
                    }
                    string id = output.itemObject.id;
                    ItemTagKey key = new ItemTagKey(output.tags);
                    addRecipe(matrixInterface,id,key,encodedRecipe);
                }
            }
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

        public void addRecipe(MatrixInterface matrixInterface, string id, ItemTagKey itemTagKey, EncodedRecipe encodedRecipe) {
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

        public void removeRecipe(MatrixInterface matrixInterface, string id, ItemTagKey itemTagKey) {
            List<(EncodedRecipe,MatrixInterface)> recipeInterfaceList = recipes[id][itemTagKey];
            for (int i = 0; i < recipeInterfaceList.Count; i++) {
                (EncodedRecipe, MatrixInterface) value = recipeInterfaceList[i];
                if (value.Item2 == matrixInterface) {
                    recipeInterfaceList.RemoveAt(i);
                    return;
                }
            }
            // O(nlogn) deletion, where n is the number of recipes in the interface
            // Only removes the recipe if it is the highest priority recipe
            //HashSet<(string, ItemTagKey)> usedRecipes = interfaceRecipes[matrixInterface];
            /*
            foreach ((string, ItemTagKey) val in usedRecipes) {
                List<(EncodedRecipe, MatrixInterface)> recipeInterfaceList = getRecipeAndInterface(val.Item1,val.Item2);
                if (recipeInterfaceList == null || recipeInterfaceList.Count < 1) {
                    continue;
                }
                (EncodedRecipe, MatrixInterface) currentRecipeInterface = recipeInterfaceList[0];
                if (currentRecipeInterface.Item2 != matrixInterface) {
                    //int deletionIndex = getIndexByPriority(recipeInterfaceList,matrixInterface);
                    // This gives a the index of the priority, but due to collisions, we have to check all nearby interfaces until we find.

                    continue;
                }
                recipeInterfaceList.RemoveAt(0);
                if (recipeInterfaceList.Count < 1) {
                    (EncodedRecipe, MatrixInterface) newRecipeInterface = recipeInterfaceList[0];
                    interfaceRecipes[newRecipeInterface.Item2].Add(val);
                }
            }
            */
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

        private List<(EncodedRecipe,MatrixInterface)> getRecipeAndInterface(string id, ItemTagKey itemTagKey) {
            // O(1) lookup
            if (!recipes.ContainsKey(id) || !recipes[id].ContainsKey(itemTagKey)) {
                return null;
            }
            return recipes[id][itemTagKey];
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

