using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityMachineProperties : MonoBehaviour
{
    [SerializeField] protected AssemblyInstruction assemblyInstruction;
    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }
    [System.Serializable]
    protected class AssemblyInstruction {
        public uint recipe;
    }
}
