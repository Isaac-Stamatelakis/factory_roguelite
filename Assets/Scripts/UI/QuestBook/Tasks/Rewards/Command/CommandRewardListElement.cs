using DevTools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.QuestBook.Tasks.Rewards.Command
{
    public class CommandRewardListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
        private int index;
        private QuestBookRewardUI parentUi;

        public void Initialize(QuestBookRewardUI parentUi, QuestBookCommandRewards parentList, int index)
        {
            this.parentUi = parentUi;
            this.index = index;
            mTextElement.text = parentList.CommandRewards[index].Description;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!DevToolUtils.OnDevToolScene) return;
            parentUi.DisplayCommandRewardEditor(index);
        }
    }
}
