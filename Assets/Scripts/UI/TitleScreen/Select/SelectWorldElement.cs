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
            nameText.text = worldDisplayData.Name;
            createDateText.text = worldDisplayData.CreateTime.ToString("MM/dd/yyyy HH:mm");
            lastAccessDateText.text = worldDisplayData.LastAccessTime.ToString("MM/dd/yyyy HH:mm");
        }

        public void SetHighlight(Color? color)
        {
            panel.color = color ?? defaultColor;
        }
    }
}
