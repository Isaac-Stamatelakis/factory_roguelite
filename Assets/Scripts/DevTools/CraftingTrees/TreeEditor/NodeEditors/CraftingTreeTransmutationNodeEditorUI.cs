using DevTools.CraftingTrees.Network;
using TMPro;
using UnityEngine;

namespace DevTools.CraftingTrees.TreeEditor.NodeEditors
{
    internal class CraftingTreeTransmutationNodeEditorUI : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown mOutputState;

        public void Display(TransmutationNodeData transmutationNodeData, CraftingTreeGeneratorUI generatorUI)
        {
            mOutputState.options = GlobalHelper.EnumToDropDown<TransmutableItemState>();
            mOutputState.onValueChanged.AddListener((value) =>
            {
                transmutationNodeData.OutputState = (TransmutableItemState)value;
                generatorUI.Rebuild();
            });
        }
    }
}
