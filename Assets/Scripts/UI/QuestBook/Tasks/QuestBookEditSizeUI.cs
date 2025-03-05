using System;
using System.Collections.Generic;
using TMPro;
using UI.QuestBook.Data.Node;
using UnityEngine;
using UnityEngine.UI;

namespace UI.QuestBook.Tasks
{
    public enum QuestBookNodeSize
    {
        Regular,
        Large,
        Huge
    }
    public class QuestBookEditSizeUI : MonoBehaviour
    {
        [SerializeField] private Button mBackButton;
        [SerializeField] private TMP_Dropdown mSizeDropDownMenu;
        public void Initialize(QuestBookNode questBookNode, QuestBookTaskPageUI pageUI)
        {
            List<TMPro.TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (QuestBookNodeSize questBookNodeSize in Enum.GetValues(typeof(QuestBookNodeSize))) {
                options.Add(new TMPro.TMP_Dropdown.OptionData($"{questBookNodeSize}"));
            }
            mSizeDropDownMenu.AddOptions(options);
            mSizeDropDownMenu.value = (int)questBookNode.NodeData.Size;
            mSizeDropDownMenu.onValueChanged.AddListener((value) =>
            {
                questBookNode.NodeData.Size = (QuestBookNodeSize)value;
                pageUI.QuestBookPageUI.Display();
            });
            mBackButton.onClick.AddListener(() =>
            {
                GameObject.Destroy(gameObject);
            });
        }
    }
}
