using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RecipeModule;

namespace TileEntityModule.Instances.WorkBenchs {
    public class WorkbenchUIController : MonoBehaviour
    {
        [SerializeField] public GridLayoutGroup recipeLayout;
        [SerializeField] public TextMeshProUGUI text;
        private WorkBench workBench;

        public void display(WorkBench workBench) {
            this.workBench = workBench;
            this.text.text = workBench.name;

        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void displayRecipes(List<WorkbenchRecipe> workbenchRecipes) {
            
        }
    }
}

