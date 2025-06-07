using UnityEngine;
using UnityEngine.UI;

namespace UI.GeneralUIElements.Sliders
{
    public class GridMaskedSliderUI : MonoBehaviour
    {
        [SerializeField] private Image panelPrefab;
        [SerializeField] private GridLayoutGroup mLightGrid;
        [SerializeField] private GridLayoutGroup mDarkGrid;
        [SerializeField] private RectMask2D mMask;
        public void Initialize(Color color, int size, Vector2Int cellSize)
        {
            InitializeGrid(mLightGrid, color,true);
            Color.RGBToHSV(color, out float h, out float s, out float v);
            h -= 0.3f;
            if (h < 0) h++;
            Color darkColor = Color.HSVToRGB(h, s, v);
            InitializeGrid(mDarkGrid, darkColor,false);
            
            return;
            void InitializeGrid(GridLayoutGroup gridLayoutGroup, Color panelColor, bool light)
            {
                gridLayoutGroup.cellSize = cellSize;
                GlobalHelper.DeleteAllChildren(gridLayoutGroup.transform);
                for (int i = 0; i < size; i++)
                {
                    Image image = Instantiate(panelPrefab, gridLayoutGroup.transform);
                    image.maskable = light;
                    image.color = panelColor;
                }
            }
        }

        public void SetFill(float fill)
        {
            mMask.rectTransform.anchorMax = new Vector2(fill, mMask.rectTransform.anchorMax.y);
        }
    }
}
