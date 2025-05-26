using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Controls.UI
{
    public class ControlSettingUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Transform listTransform;
        [SerializeField] private TextMeshProUGUI headerPrefab;
        [SerializeField] private ControlUIElement controlUIElementPrefab;
        private Dictionary<PlayerControl, ControlUIElement> elementUIDict = new Dictionary<PlayerControl, ControlUIElement>();
        
        public void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                CanvasController.Instance.PopStack();
                //ControlUtils.LoadBindings();
            });
            restoreButton.onClick.AddListener(() =>
            {
                ControlUtils.SetDefault();
                Display();
            });
            Display();
        }
        
        private void Display()
        {
            InputActions inputActions = CanvasController.Instance.InputActions;
            GlobalHelper.DeleteAllChildren(listTransform);
            PlayerControl[] playerControls = Enum.GetValues(typeof(PlayerControl)) as PlayerControl[];
            foreach (PlayerControl playerControl in playerControls)
            {
                ControlUIElement controlUIElement = Instantiate(controlUIElementPrefab, listTransform);
                controlUIElement.Initalize(playerControl,inputActions,this);
                elementUIDict[playerControl] = controlUIElement;
            }
            CheckConflicts();
        }
        
        public void CheckConflicts()
        {
            HashSet<PlayerControl> conflicts = ControlUtils.GetConflictingBindings();
            foreach (var (control, controlUIElement) in elementUIDict)
            {
                controlUIElement.HighlightConflictState(conflicts.Contains(control));
            }
        }
        public void OnDestroy()
        {
            PlayerScript playerScript = PlayerManager.Instance?.GetPlayer();
            if (!playerScript) return;
            playerScript.PlayerUIContainer.IndicatorManager.SyncKeyCodes(false);
        }
    }
}
