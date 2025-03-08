using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TitleScreen.Select
{
    public class SelectWorldElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI createDateText;
        [SerializeField] private TextMeshProUGUI lastAccessDateText;
        private Action<int> selectCallback;
        private int index;
        private Color defaultColor;
        private Image panel;
        public void Initalize(int initIndex, Action<int> callback, WorldDisplayData worldDisplayData)
        {
            panel = GetComponent<Image>();
            GetComponent<Button>().onClick.AddListener(() =>
            {
                callback(initIndex);
            });
            defaultColor = panel.color;
            this.selectCallback = callback;
            this.index = initIndex;
            DisplayData(worldDisplayData);
        }

        public void DisplayData(WorldDisplayData worldDisplayData)
        {
            const string CREATION_PREFIX = "Created:";
            const string LAST_PLAYED_PREFIX = "Last Played:";
            if (worldDisplayData.Corrupted || worldDisplayData.CreateTime == null)
            {
                createDateText.text = $"{CREATION_PREFIX}?";
            }
            else
            {
                createDateText.text = $"{CREATION_PREFIX}{worldDisplayData.CreateTime.Value:MM/dd/yyyy}";
            }

            if (worldDisplayData.Corrupted || worldDisplayData.LastAccessTime == null)
            {
                lastAccessDateText.text = $"{LAST_PLAYED_PREFIX}?";
            }
            else
            {
                lastAccessDateText.text = $"{LAST_PLAYED_PREFIX}{worldDisplayData.LastAccessTime.Value:MM/dd/yyyy HH:mm}";
            }
            nameText.text = worldDisplayData.Name.Replace("_"," ");
        }

        public void SetHighlight(Color? color)
        {
            panel.color = color ?? defaultColor;
        }
    }
}
