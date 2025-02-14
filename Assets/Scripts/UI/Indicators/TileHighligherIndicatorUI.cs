using Tiles;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class TileHighligherIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image tileImage;
        public void Display(bool active)
        {
            tileImage.color = active ? Color.blue : Color.gray;
        }
    }
}
