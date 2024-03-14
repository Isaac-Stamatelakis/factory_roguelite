using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule {
    public interface IFormulaCraftingItem {
        public string getFormula();
    }
    [CreateAssetMenu(fileName ="New Material Item",menuName="Item/Instances/Crafting/Formula")]
    public class FormulaCraftingItem : CraftingItem, IFormulaCraftingItem {
        [Header("Chemical Formula for the Item")]
        [SerializeField] private string formula;

        public string getFormula()
        {
            return formula;
        }
    }
}
