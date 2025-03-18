using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Recipe.Viewer
{
    public class RecipeRequirementUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textElementPrefab;
        [SerializeField] private VerticalLayoutGroup mCostList;
        [SerializeField] private VerticalLayoutGroup mRequirementList;
        public void Display(List<string> costs, List<string> requirements)
        {
            GlobalHelper.deleteAllChildren(mCostList.transform);
            GlobalHelper.deleteAllChildren(mRequirementList.transform);
            foreach (string cost in costs)
            {
                var textElement = Instantiate(textElementPrefab, mCostList.transform);
                textElement.text = cost;
            }

            mRequirementList.gameObject.SetActive(requirements.Count > 0);
            foreach (string requirement in requirements)
            {
                var textElement = Instantiate(textElementPrefab, mRequirementList.transform);
                textElement.text = requirement;
            }
        }
    }
}
