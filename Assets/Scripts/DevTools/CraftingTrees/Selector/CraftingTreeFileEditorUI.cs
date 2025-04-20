using System;
using System.IO;
using DevTools.CraftingTrees.Network;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.Selector
{
    public class CraftingTreeFileEditorUI : MonoBehaviour
    {
        [SerializeField] private Button mBackButton;
        [SerializeField] private TMP_InputField mInputField;
        [SerializeField] private TextMeshProUGUI mDeletionStatusText;
        [SerializeField] private Button mDeleteButton;
        private string newFileName;
        private string initialFilePath;
        private bool fileNameChanged;
        private Action onChange;
        public void Display(string filePath, Action onChangeCallback)
        {
            SerializedCraftingTreeNodeNetwork serializedCraftingTreeNodeNetwork = GlobalHelper.DeserializeCompressedJson<SerializedCraftingTreeNodeNetwork>(filePath);
            CraftingTreeNodeNetwork craftingTreeNodeNetwork = SerializedCraftingTreeNodeNetworkUtils.DeserializeNodeNetwork(serializedCraftingTreeNodeNetwork);
            string initialFileName = Path.GetFileNameWithoutExtension(filePath);
            this.initialFilePath = filePath;
            this.onChange = onChangeCallback;
            mInputField.text = initialFileName;
            mInputField.onValueChanged.AddListener(OnNameChange);
            
            mBackButton.onClick.AddListener(CanvasController.Instance.PopStack);
            
            mDeleteButton.onClick.AddListener(OnDelete);
            bool canDelete = !craftingTreeNodeNetwork.HasGeneratedRecipes();
            mDeletionStatusText.text = canDelete
                ? "Crafting tree has no generated recipes and can be deleted"
                : "Crafting tree has generated recipes and cannot be deleted";
            mDeleteButton.interactable = canDelete;
            return;
            
            void OnNameChange(string text)
            {
                fileNameChanged = text != initialFileName;
                newFileName = text;
            }

            void OnDelete()
            {
                if (craftingTreeNodeNetwork.HasGeneratedRecipes()) return;
                
                if (!File.Exists(filePath)) return;
                
                File.Delete(filePath);
                fileNameChanged = false;
                CanvasController.Instance.PopStack();
                onChange.Invoke();
            }
        }

        public void OnDestroy()
        {
            if (!fileNameChanged) return;
            string initialFolder = Path.GetDirectoryName(initialFilePath);
            if (initialFolder == null) return;
            string extension = Path.GetExtension(initialFilePath);
            string newFilePath = Path.Combine(initialFolder, newFileName + extension);
            if (File.Exists(newFilePath)) return;
            File.Move(initialFilePath,newFilePath);
            onChange.Invoke();
        }
    }
}
