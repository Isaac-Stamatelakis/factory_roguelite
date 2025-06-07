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
            h += 0.05f;
            h %= 1;
            Color darkColor = 0.2f*Color.HSVToRGB(h, s, v);
            darkColor.a = 1;
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
            fill = Mathf.Clamp(fill,0, 1f);
            mMask.rectTransform.anchorMax = new Vector2(fill, mMask.rectTransform.anchorMax.y);
        }
    }
}
