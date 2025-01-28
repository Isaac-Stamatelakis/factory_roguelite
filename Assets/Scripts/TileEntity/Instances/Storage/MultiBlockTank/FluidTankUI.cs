using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Storage.MultiBlockTank
{
    public class FluidTankUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTitle;
        [SerializeField] private TextMeshProUGUI mCurrentStorage;
        [SerializeField] private TextMeshProUGUI mMaxStorage;
        [SerializeField] private TextMeshProUGUI mPercentFill;
        [SerializeField] private Scrollbar scrollbar;
    }
}
