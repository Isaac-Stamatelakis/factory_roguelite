using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileRotationIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image tileImage;
        public void Display(int rotation)
        {
            tileImage.transform.rotation = Quaternion.Euler(0, 90*rotation, 0);
        }
    }
}
