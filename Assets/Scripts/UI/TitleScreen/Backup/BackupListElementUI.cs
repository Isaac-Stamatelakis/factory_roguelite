using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.BackUp;

namespace UI.TitleScreen.Backup
{
    public class BackupListElementUI : MonoBehaviour
    {
        [SerializeField] private Button mButton;
        [SerializeField] private TextMeshProUGUI dDateText;
        [SerializeField] private TextMeshProUGUI mTimeText;

        public void Display(string date, string time, int index, Action<int> callback)
        {
            dDateText.text = date;
            mTimeText.text = time;
            mButton.onClick.AddListener(() =>
            {
                callback(index);
            });
        }
    }
}
