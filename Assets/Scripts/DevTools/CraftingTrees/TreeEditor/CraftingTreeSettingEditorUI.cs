using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevTools.CraftingTrees.Network;
using Item.GameStage;
using Items;
using Recipe;
using Recipe.Objects;
using Recipe.Processor;
using RecipeModule;
using TileEntity;
using TMPro;
using UI;
using UI.ToolTip;
using Unity.VisualScripting;
using Item.Slot;
using Items.Transmutable;
#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.VersionControl;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.TreeEditor
{
    internal interface ITreeGenerationListener
    {
        public void OnStatusChange(bool generated);
    }
    
    internal class CraftingTreeSettingEditorUI : MonoBehaviour
    {
        [SerializeField] private Button mItemButton;
        [SerializeField] private Button mTransmutationButton;
        [SerializeField] private Button mProcessorButton;
        [SerializeField] private Color highlightColor;
        [SerializeField] private TMP_Dropdown mTransmutationEfficiencyDropDown;
        [SerializeField] private Button mGenerateButton;
        [SerializeField] private Button mDeleteButton;
        [SerializeField] private Image mStatusIcon;
        
        private CraftingTreeNodeType generateNodeType;
        private Button currentHighlightButton;
        private CraftingTreeGenerator craftingTreeGenerator;
        private CraftingTreeNodeNetwork network;
        private CanvasController canvasController;
        private List<ITreeGenerationListener> craftingTreeGenerationStatusListeners;
        private InputActions inputActions;
        private System.Action<InputAction.CallbackContext> select1Handler;
        private System.Action<InputAction.CallbackContext> select2Handler;
        private System.Action<InputAction.CallbackContext> select3Handler;

        private string recipeTreeName;
        public void Start()
        {
            canvasController = CanvasController.Instance;
            
            inputActions = CanvasController.Instance.InputActions;
            
            select1Handler = ctx => SwitchCraftingType(mItemButton, CraftingTreeNodeType.Item);
            select2Handler = ctx => SwitchCraftingType(mTransmutationButton, CraftingTreeNodeType.Transmutation);
            select3Handler = ctx => SwitchCraftingType(mProcessorButton, CraftingTreeNodeType.Processor);

            inputActions.InventoryNavigation.Select1.performed += select1Handler;
            inputActions.InventoryNavigation.Select2.performed += select2Handler;
            inputActions.InventoryNavigation.Select3.performed += select3Handler;
            
            inputActions.InventoryNavigation.Enable();
        }

        public void OnDestroy()
        {
            inputActions.InventoryNavigation.Select1.performed -= select1Handler;
            inputActions.InventoryNavigation.Select2.performed -= select2Handler;
            inputActions.InventoryNavigation.Select3.performed -= select3Handler;
            inputActions.InventoryNavigation.Disable();
        }
        
        private void SwitchCraftingType(Button button, CraftingTreeNodeType type)
        {
            if (generateNodeType == type || canvasController.IsTyping) return;
            generateNodeType = type;
            if (currentHighlightButton)
            {
                currentHighlightButton.GetComponent<Image>().color = button.GetComponent<Image>().color;
            }
            currentHighlightButton = button;
            currentHighlightButton.GetComponent<Image>().color = highlightColor;
            craftingTreeGenerator.SetType(type);
        }

        public void Initialize(CraftingTreeGeneratorUI generatorUI, CraftingTreeNodeNetwork craftingTreeNodeNetwork, CraftingTreeGenerator treeGenerator, List<ITreeGenerationListener> listeners, string recipeTreeName)
        {
            this.recipeTreeName = recipeTreeName;
            craftingTreeGenerationStatusListeners = listeners;
            this.network = craftingTreeNodeNetwork;
            craftingTreeGenerator = treeGenerator;
            void InitializeNodeButton(Button button, CraftingTreeNodeType type)
            {
                if (generateNodeType == type)
                {
                    button.GetComponent<Image>().color = highlightColor;
                    currentHighlightButton = button;
                    treeGenerator.SetType(type);
                }
                button.onClick.AddListener(() =>
                {
                    SwitchCraftingType(button, type);
                });
                ToolTipUIDisplayer toolTipUIDisplayer = button.AddComponent<ToolTipUIDisplayer>();
                toolTipUIDisplayer.SetMessage($"Switches Node Placement to '{GlobalHelper.AddSpaces(type.ToString())}'");
            }

            InitializeNodeButton(mItemButton, CraftingTreeNodeType.Item);
            InitializeNodeButton(mTransmutationButton, CraftingTreeNodeType.Transmutation);
            InitializeNodeButton(mProcessorButton, CraftingTreeNodeType.Processor);
            mTransmutationEfficiencyDropDown.options = GlobalHelper.EnumToDropDown<TransmutationEfficency>();
            mTransmutationEfficiencyDropDown.value = (int)craftingTreeNodeNetwork.TransmutationEfficency;
            mTransmutationEfficiencyDropDown.onValueChanged.AddListener(UpdateEffiency);
            
            mGenerateButton.onClick.AddListener(GenerateRecipes);
            mDeleteButton.onClick.AddListener(DeleteRecipes);
            InitializeStatusIcon();
            SetInteractablity();

            return;
            void InitializeStatusIcon()
            {
                ToolTipUIDisplayer toolTipUIDisplayer = mStatusIcon.AddComponent<ToolTipUIDisplayer>();
                toolTipUIDisplayer.SetAction(MessageAction);
                SetStatusIconColor();
                return;
                
                string MessageAction()
                {
                    return craftingTreeNodeNetwork.HasGeneratedRecipes() 
                        ? "Tree Recipes Generated\nTree is Un-interactable" 
                        : "Tree Recipes Not Generated\nTree is Interactable";
                }
            }

            void UpdateEffiency(int value)
            {
                craftingTreeNodeNetwork.TransmutationEfficency = (TransmutationEfficency)value;
                generatorUI.Rebuild();
            }
        }
        
        private void SetInteractablity()
        {
            bool generated = network.HasGeneratedRecipes();
            mTransmutationEfficiencyDropDown.interactable = !generated;
            foreach (var listener in craftingTreeGenerationStatusListeners)
            {
                listener.OnStatusChange(generated);
            }
            mDeleteButton.interactable = generated;
            mGenerateButton.interactable = !generated;
        }

        private void SetStatusIconColor()
        {
            mStatusIcon.GetComponent<Image>().color = network.HasGeneratedRecipes() ? Color.cyan : Color.green;
        } 

        private void GenerateRecipes()
        {
#if UNITY_EDITOR
            int generateCount = 0;
            int modifyCount = 0;
            int deleteCount = 0;
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            Dictionary<int, CraftingTreeGeneratorNode> nodeDictionary = new Dictionary<int, CraftingTreeGeneratorNode>();
            foreach (var node in network.Nodes)
            {
                nodeDictionary[node.GetId()] = node;
            }
            
            foreach (var node in network.Nodes)
            {
                if (node == null || node.NodeType != CraftingTreeNodeType.Processor) continue;
                ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                string assetPath = AssetDatabase.GUIDToAssetPath(processorNodeData.ProcessorGuid);
                RecipeProcessor recipeProcessor = AssetDatabase.LoadAssetAtPath<RecipeProcessor>(assetPath);
                if (!recipeProcessor) continue;
                
                int mode = processorNodeData.Mode;
                if (mode >= recipeProcessor.RecipeCollections.Count)
                {
                    Debug.LogWarning($"Recipe Processor {recipeProcessor.name} does not have a mode '{mode}'");
                    continue;
                }
                
                string recipePath = AssetDatabase.GUIDToAssetPath(processorNodeData.RecipeGuid);
                RecipeObject recipeObject = AssetDatabase.LoadAssetAtPath<RecipeObject>(recipePath);
                
                List<RecipeObject> recipes = recipeProcessor.RecipeCollections[mode].RecipeCollection.Recipes;
                if (recipeObject && !RecipeUtils.CurrentValid(recipeObject, recipeProcessor.RecipeType))
                {
                    if (recipes.Contains(recipeObject))
                    {
                        recipes.Remove(recipeObject);
                    }
                    UnityEditor.AssetDatabase.DeleteAsset(recipePath);
                    deleteCount++;
                    recipeObject = null;
                }

                bool newRecipe = false;
                if (!recipeObject)
                {
                    recipeObject = RecipeUtils.GetNewRecipeObject(recipeProcessor.RecipeType, null);
                    if (recipeObject is not ItemRecipeObject)
                    {
                        Debug.LogWarning($"Generated invalid recipe {recipeObject.name}");
                        GameObject.Destroy(recipeObject);
                        continue;
                    }
                    newRecipe = true;
                    generateCount++;
                }

                if (newRecipe)
                {
                    var recipeModeCollection = recipeProcessor.RecipeCollections[mode].RecipeCollection;
                    string collectionPath = AssetDatabase.GetAssetPath(recipeModeCollection);
                    string folder = Path.GetDirectoryName(collectionPath);
                    string randomSuffix = Guid.NewGuid().ToString("N"); // "N" removes hyphens
                    string recipeName = recipeTreeName + "_GENERATED_" + randomSuffix;
                    recipeObject.name = recipeName;
                    AssetDatabase.CreateAsset(recipeObject, Path.Combine(folder, recipeName + ".asset"));
                    recipeModeCollection.Recipes.Add(recipeObject);
                    string recipeObjectPath = AssetDatabase.GetAssetPath(recipeObject);
                    string recipeGuid = AssetDatabase.AssetPathToGUID(recipeObjectPath);
                    processorNodeData.RecipeGuid = recipeGuid;
                }
                else
                {
                    modifyCount++;
                }

                switch (recipeProcessor.RecipeType)
                {
                    case RecipeType.Item:
                        break;
                    case RecipeType.Passive:
                        PassiveItemRecipeObject passiveItemRecipeObject = (PassiveItemRecipeObject)recipeObject;
                        PassiveRecipeMetaData passiveRecipeMetaData = (PassiveRecipeMetaData)processorNodeData.RecipeData;
                        passiveItemRecipeObject.Seconds = passiveRecipeMetaData.Seconds;
                        break;
                    case RecipeType.Generator:
                        GeneratorItemRecipeObject generatorItemRecipeObject = (GeneratorItemRecipeObject)recipeObject;
                        GeneratorItemRecipeMetaData generatorRecipeMetaData = (GeneratorItemRecipeMetaData)processorNodeData.RecipeData;
                        generatorItemRecipeObject.Seconds = generatorRecipeMetaData.Seconds;
                        generatorItemRecipeObject.EnergyPerTick = generatorRecipeMetaData.EnergyPerTick;
                        break;
                    case RecipeType.Machine:
                        ItemEnergyRecipeObject machineRecipeObject = (ItemEnergyRecipeObject)recipeObject;
                        ItemEnergyRecipeMetaData machineRecipeMetaData = (ItemEnergyRecipeMetaData)processorNodeData.RecipeData;
                        machineRecipeObject.MinimumEnergyPerTick = machineRecipeMetaData.MinimumEnergyPerTick;
                        machineRecipeObject.TotalInputEnergy = machineRecipeMetaData.TotalInputEnergy;
                        break;
                    case RecipeType.Burner:
                        BurnerRecipeObject burnerRecipeObject = (BurnerRecipeObject)recipeObject;
                        BurnerRecipeMetaData burnerRecipeMetaData = (BurnerRecipeMetaData)processorNodeData.RecipeData;
                        burnerRecipeObject.Seconds = burnerRecipeMetaData.Seconds;
                        burnerRecipeObject.PassiveSpeed = burnerRecipeMetaData.PassiveSpeed;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(recipeProcessor.RecipeType), $"Unknown recipe type: {recipeProcessor.RecipeType}");
                }
                    
                
                ItemRecipeObject itemRecipeObject = (ItemRecipeObject)recipeObject;
                itemRecipeObject.Inputs.Clear();
                itemRecipeObject.Outputs.Clear();
                List<int> inputIds = node.NetworkData.InputIds;
                foreach (int inputId in inputIds)
                {
                    var inputNode = nodeDictionary.GetValueOrDefault(inputId);
                    if (inputNode == null) continue;
                    SerializedItemSlot serializedItemSlot = GetSerializedItemSlot(inputNode);
                    if (serializedItemSlot == null) continue;
                    ItemObject itemObject = itemRegistry.GetItemObject(serializedItemSlot.id);
                    if (!itemObject) continue;
                    EditorItemSlot editorItemSlot = new EditorItemSlot(serializedItemSlot.id, serializedItemSlot.amount);
                    itemRecipeObject.Inputs.Add(editorItemSlot);
                }
                
                int currentId = node.GetId();
                foreach (var otherNode in network.Nodes)
                {
                    if (otherNode == null || !otherNode.NetworkData.InputIds.Contains(currentId)) continue;
                    SerializedItemSlot serializedItemSlot = GetSerializedItemSlot(otherNode);
                    if (serializedItemSlot == null) continue;
                    
                    ItemObject itemObject = itemRegistry.GetItemObject(serializedItemSlot.id);
                    if (!itemObject) continue;
                    float chance = 1;
                    if (otherNode.NodeType == CraftingTreeNodeType.Item)
                    {
                        ItemNodeData itemNodeData = (ItemNodeData)otherNode.NodeData;
                        chance = itemNodeData.Odds;
                    }

                    RandomEditorItemSlot editorItemSlot = new RandomEditorItemSlot(itemObject.id, serializedItemSlot.amount, chance);
                    itemRecipeObject.Outputs.Add(editorItemSlot);
                }
                
                EditorUtility.SetDirty(itemRecipeObject);
                AssetDatabase.SaveAssetIfDirty(itemRecipeObject);
            }
            AssetDatabase.Refresh();
            Debug.Log($"Generated {generateCount} new recipes, modified {modifyCount} recipes & deleted {deleteCount} recipes.");
            SetStatusIconColor();
            SetInteractablity();

            return;
            SerializedItemSlot GetSerializedItemSlot(CraftingTreeGeneratorNode node)
            {
                if (node.NodeType == CraftingTreeNodeType.Item)
                {
                    ItemNodeData itemNodeData = (ItemNodeData)node.NodeData;
                    return itemNodeData.SerializedItemSlot;
                }

                if (node.NodeType == CraftingTreeNodeType.Transmutation)
                {
                    TransmutationNodeData transmutationNodeData = (TransmutationNodeData)node.NodeData;
                    ItemSlot itemSlot = transmutationNodeData.GetItemSlot(network.TransmutationEfficency);
                    return new SerializedItemSlot(itemSlot?.itemObject?.id,itemSlot?.amount ?? 0, null);
                }
                return null;
            }
#endif
        }

        private void DeleteRecipes()
        {
#if UNITY_EDITOR
            int deleteCount = 0;
            foreach (var node in network.Nodes)
            {
                if (node.NodeType != CraftingTreeNodeType.Processor) continue;
                ProcessorNodeData processorNodeData = (ProcessorNodeData)node.NodeData;
                if (processorNodeData.RecipeGuid == null) continue;
                
                string assetPath = AssetDatabase.GUIDToAssetPath(processorNodeData.ProcessorGuid);
                RecipeProcessor recipeProcessor = AssetDatabase.LoadAssetAtPath<RecipeProcessor>(assetPath);
                if (recipeProcessor == null) continue;
                
                string recipePath = AssetDatabase.GUIDToAssetPath(processorNodeData.RecipeGuid);
                RecipeObject recipeObject = AssetDatabase.LoadAssetAtPath<RecipeObject>(recipePath);
                if (!recipeObject) continue;
                
                int mode = processorNodeData.Mode;
                List<RecipeObject> recipes = recipeProcessor.RecipeCollections[mode].RecipeCollection.Recipes;
                if (mode >= recipeProcessor.RecipeCollections.Count)
                {
                    Debug.LogWarning($"Recipe Processor {recipeProcessor.name} does not have a mode '{mode}'");
                    continue;
                }
                if (recipeObject)
                {
                    if (recipes.Contains(recipeObject))
                    {
                        recipes.Remove(recipeObject);
                    }
                    UnityEditor.AssetDatabase.DeleteAsset(recipePath);
                    processorNodeData.RecipeGuid = null;
                    deleteCount++;
                }
            }
            Debug.Log($"Deleted {deleteCount} recipes");
            AssetDatabase.Refresh();
            SetStatusIconColor();
            SetInteractablity();
#endif
        }
        

    }
}
