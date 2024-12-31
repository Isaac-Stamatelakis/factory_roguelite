using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Items;

namespace TileEntity.Instances.Matrix {
    public class AutoCraftingSequence
    {

        private void build(MatrixRecipeCollection matrixRecipeCollection, ItemSlot toCraft) {

        }
    }

    public static class AutoCraftingSequenceFactory {

        public static Tree<PreparedRecipePreview> CreateRecipeTree(ItemMatrixControllerInstance controller, ItemSlot toCraft, uint amount) {
            MatrixRecipeCollection recipeCollection = controller.Recipes;
            (EncodedRecipe, MatrixInterfaceInstance) primaryTuple = recipeCollection.getRecipeAndInterface(toCraft);
            EncodedRecipe rootRecipe = primaryTuple.Item1;
            if (rootRecipe == null || primaryTuple.Item2 == null) {
                return null;
            }
            PreparedRecipePreview rootPreparedRecipe = new PreparedRecipePreview(
                inputs: PrepareInputs(rootRecipe.Inputs,rootRecipe),
                outputs: PrepareOutputs(rootRecipe.Outputs,rootRecipe,amount),
                amount: amount
            );
            Tree<PreparedRecipePreview> craftingTree = new Tree<PreparedRecipePreview>(rootPreparedRecipe);
            BuildTreeNode(craftingTree.Root, controller);
            return craftingTree;
        }

        private static List<(ItemSlot,uint,bool)> PrepareInputs(List<ItemSlot> inputs, EncodedRecipe recipe) {
            List<(ItemSlot, uint,bool)> preparedInputs = new List<(ItemSlot, uint,bool)>();
            foreach (ItemSlot input in recipe.Inputs) {
                if (input == null || input.itemObject == null) {
                    continue;
                }
                preparedInputs.Add((input,0,false));
            }
            return preparedInputs;
        }
        private static List<(ItemSlot,uint)> PrepareOutputs(List<ItemSlot> outputs, EncodedRecipe recipe, uint amount) {
            List<(ItemSlot, uint)> preparedOutputs = new List<(ItemSlot, uint)>();
            foreach (ItemSlot output in recipe.Outputs) {
                if (output == null || output.itemObject == null) {
                    continue;
                }
                uint produceAmount = output.amount * amount;
                preparedOutputs.Add((output,produceAmount));
            }
            return preparedOutputs;
        }
        private static void BuildTreeNode(TreeNode<PreparedRecipePreview> node, ItemMatrixControllerInstance controller) {
            PreparedRecipePreview preparedRecipe = node.Value;
            for (int i = 0; i < preparedRecipe.AvailableInputs.Count; i++) {
                ItemSlot input = preparedRecipe.AvailableInputs[i].Item1;
                if (input == null || input.itemObject == null) {
                    continue;
                }
                string id = input.itemObject.id;
                ItemTagKey tagKey = new ItemTagKey(input.tags);
                uint requiredAmount = preparedRecipe.Amount * input.amount;
                uint amountOfInput = controller.AmountOf(id, tagKey);
                //Debug.Log(input.itemObject.name + ":" + amountOfInput);
                if (amountOfInput >= requiredAmount) {
                    (ItemSlot, uint,bool) tuple = preparedRecipe.AvailableInputs[i];
                    tuple.Item2 = requiredAmount;
                    preparedRecipe.AvailableInputs[i] = tuple;
                    continue;
                }
                EncodedRecipe encodedRecipe = controller.Recipes.getRecipe(id,tagKey);
                uint craftAmount = requiredAmount-amountOfInput;
                (ItemSlot, uint,bool) tuple1 = preparedRecipe.AvailableInputs[i];
                tuple1.Item2 = amountOfInput;
                tuple1.Item3 = encodedRecipe != null;
                preparedRecipe.AvailableInputs[i] = tuple1;
                if (encodedRecipe==null) {
                    continue;
                }
                PreparedRecipePreview newRecipe = new PreparedRecipePreview(
                    inputs: PrepareInputs(encodedRecipe.Inputs,encodedRecipe),
                    outputs: PrepareOutputs(encodedRecipe.Outputs,encodedRecipe,craftAmount),
                    amount: craftAmount
                );
                TreeNode<PreparedRecipePreview> inputNode = new TreeNode<PreparedRecipePreview>(newRecipe);
                node.Children.Add(inputNode);
                BuildTreeNode(inputNode,controller);
            }
        }

        public static (int,int) getCoresAndMemory(Tree<PreparedRecipePreview> tree) {
            // memory required = 8 * total number of items involved in the craft
            // cores is the max number of active recipes at any moment. 
            return (0,0);
        }

        
    }

    public class PreparedRecipePreview {
        private List<(ItemSlot,uint,bool)> availableInputs;
        private List<(ItemSlot,uint)> outputAmounts;
        public PreparedRecipePreview(List<(ItemSlot,uint,bool)> inputs, List<(ItemSlot,uint)> outputs, uint amount) {
            this.availableInputs = inputs;
            this.outputAmounts = outputs;
            this.amount = amount;
        }

        public List<(ItemSlot, uint,bool)> AvailableInputs { get => availableInputs; set => availableInputs = value; }
        public List<(ItemSlot, uint)> OutputAmounts { get => outputAmounts; set => outputAmounts = value; }
        public uint Amount { get => amount; set => amount = value; }

        private uint amount;
    }
    public class PreparedRecipe {
        private List<ItemSlot> inputs;
        // Recipes may output multiple items, but it sufficent to only store the target
        private ItemSlot output;
        private int possibleCrafts;
        public PreparedRecipe(List<ItemSlot> inputs, ItemSlot output) {
            this.inputs = inputs;
            this.output = output;
        }
        public List<ItemSlot> Inputs { get => inputs; }
        public ItemSlot Output { get => output; }
        public int PossibleCrafts { get => possibleCrafts; set => possibleCrafts = value; }
    }

    public class TreeNode<T> {
        private T value;
        public T Value {get => value;}
        private List<TreeNode<T>> children;
        public List<TreeNode<T>> Children {get => children;}

        public TreeNode(T value) {
            this.value = value;
            this.children = new List<TreeNode<T>>();
        }
        public void AddChild(T value)
        {
            children.Add(new TreeNode<T>(value));
        }
        public int getSize() {
            int size = 1;
            foreach (TreeNode<T> child in children) {
                size += child.getSize();
            }
            return size;
        }
        public bool HasChildren {get => children.Count != 0;}
    }

    public class Tree<T> {
        private TreeNode<T> root;
        public TreeNode<T> Root {get => root;}

        public Tree(T rootValue)
        {
            root = new TreeNode<T>(rootValue);
        }

        public int getSize() {
            if (root == null) {
                return 0;
            }
            return root.getSize();
        }
    }

    public static class TreeHelper {
        public static List<TreeNode<T>> getTerminalNodes<T>(Tree<T> tree) {
            List<TreeNode<T>> leaves = new List<TreeNode<T>>();
            findTerminals<T>(tree.Root,leaves);
            return leaves;
        }   

        private static void findTerminals<T>(TreeNode<T> node, List<TreeNode<T>> leaves) {
            if (node.HasChildren) {
                foreach (TreeNode<T> child in node.Children) {
                    findTerminals<T>(child,leaves);
                }
            } else {
                leaves.Add(node);
            }
        }

        public static List<TreeNode<T>> postOrderTraversal<T>(Tree<T> tree) {
            List<TreeNode<T>> result = new List<TreeNode<T>>();
            postNodeTraverse<T>(tree.Root,result);
            return result;
        }

        private static void postNodeTraverse<T>(TreeNode<T> node, List<TreeNode<T>> result) {
            foreach (TreeNode<T> child in node.Children) {
                postNodeTraverse(child,result);
            }
            result.Add(node);
        }
    }

}

