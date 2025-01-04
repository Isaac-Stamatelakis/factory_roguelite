using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Recipe.Viewer
{
    public class RecipeRequirementUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textElementPrefab;

        public void Display(List<string> formattedRequirements)
        {
            GlobalHelper.deleteAllChildren(transform);
            foreach (string formattedRequirement in formattedRequirements)
            {
                var textElement = Instantiate(textElementPrefab, transform);
                textElement.text = formattedRequirement;
            }
        }
    }
}
