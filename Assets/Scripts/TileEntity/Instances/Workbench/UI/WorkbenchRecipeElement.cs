using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench
{
    public class WorkbenchRecipeElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private ItemSlotUI itemSlotUI;
    }
}
