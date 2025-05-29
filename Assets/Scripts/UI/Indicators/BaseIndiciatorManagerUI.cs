using Player.Controls;
using TMPro;
using UI.Indicators.General;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public abstract class BaseIndiciatorManagerUI : MonoBehaviour
    {
        [SerializeField] protected GameObject keyCodePrefab;
        [SerializeField] protected Transform keyCodeContainer;
        [SerializeField] protected Transform indicatorContainer;
        
        public void SyncKeyCodes(bool instantiate)
        {
            if (instantiate)
            {
                GlobalHelper.DeleteAllChildren(keyCodeContainer);
            }

            float baseTextSize = keyCodePrefab.GetComponentInChildren<TextMeshProUGUI>().fontSize;
            int idx = 0;
            for (int i = 0; i < indicatorContainer.childCount; i++)
            {
                GameObject indicatorObject = indicatorContainer.GetChild(i).gameObject;
                if (!indicatorObject.activeInHierarchy) continue;
                
                IKeyCodeIndicator keyCodeIndicator = indicatorObject.GetComponent<IKeyCodeIndicator>();
                PlayerControl? nullableControl = keyCodeIndicator?.GetPlayerControl();
                
                string text = nullableControl.HasValue
                    ? ControlUtils.FormatInputText(nullableControl.Value)
                    : string.Empty;
                
                GameObject keyCodeElement = instantiate ? 
                    Instantiate(keyCodePrefab, keyCodeContainer)
                    : keyCodeContainer.GetChild(idx).gameObject;
                
                if (string.IsNullOrEmpty(text))
                {
                    keyCodeElement.GetComponent<Image>().enabled = false;
                }
                if (keyCodeIndicator is IKeyCodeDescriptionIndicator optionalKeyCodeIndicator)
                {
                    ToolTipUIDisplayer toolTipUIDisplayer = keyCodeElement.AddComponent<ToolTipUIDisplayer>();
                    optionalKeyCodeIndicator.SyncToolTipDisplayer(toolTipUIDisplayer);
                }
                
                TextMeshProUGUI textElement = keyCodeElement.GetComponentInChildren<TextMeshProUGUI>();
                textElement.text = text;
                int longestWord = LongestWord(text);
                textElement.fontSize = baseTextSize - 1.05f*(longestWord -1);
                idx++;
            }

            return;
            int LongestWord(string text)
            {
                if (string.IsNullOrEmpty(text)) return 0;
                int longest = 0;
                int current = 0;
                foreach (char c in text)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        current = 0;
                        continue;
                    }

                    current++;
                    if (current > longest) longest = current;
                }
                return longest;
            }
        }

        public void SetColor(Color color)
        {
            Image[] images = GetComponentsInChildren<Image>();
            foreach (Image image in images)
            {
                image.color = color;
            }
        }
    }
}
