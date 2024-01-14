using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTileEntityClickController : MonoBehaviour
{
    [SerializeField] protected List<AssemblyInstruction> assemblyInstructions;
    protected List<OnClick> onClicks = new List<OnClick>();
    public virtual void Start() {
        if (assemblyInstructions == null) {
            return;
        }
        foreach (AssemblyInstruction assemblyInstruction in assemblyInstructions) {
            switch (assemblyInstruction.type) {
                case 0:
                    onClicks.Add(new OnClickOpenGui(assemblyInstruction.instruction));
                    break;
                case 1:
                    break;
            }
        }
    }

    public void activeClick() {
        foreach (OnClick onClick in onClicks) {
            onClick.clicked(gameObject);
        }
    }
    [System.Serializable]
    protected class AssemblyInstruction {
        [Tooltip("0:GUI\n1:Action")]
        public uint type;
        [Tooltip("0:PrefabPath\n1:ActionType")]
        public string instruction;
    }
    
}
