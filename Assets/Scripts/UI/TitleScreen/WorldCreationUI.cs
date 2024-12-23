using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldModule;

namespace UI.TitleScreen
{
    public class WorldCreationUI : MonoBehaviour
    {
        [SerializeField] private Button createButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_InputField nameInputField;
        private Action<string> createCallback;
        // Start is called before the first frame update
        public void Initalize(Action<string> callback)
        {
            this.createCallback = callback;
        }
        public void Start()
        {
            exitButton.onClick.AddListener(CanvasController.Instance.PopStack);
            createButton.onClick.AddListener(() =>
            {
                StartCoroutine(CreateWorldClick());
            });
        }

        private IEnumerator CreateWorldClick()
        {
            if (nameInputField.text.Length == 0)
            {
                yield break;
            }

            string worldName = nameInputField.text.ToLower().Replace(" ","_");
            while (true)
            {
                string worldPath = WorldLoadUtils.GetWorldPath(worldName);
                Debug.Log(worldPath);
                int count = 0;
                if (!Directory.Exists(worldPath))
                {
                    break;
                }

                count++;
                worldName = $"{worldName}_{count}";
            }
            yield return StartCoroutine(WorldCreation.CreateWorld(worldName));
            createCallback?.Invoke(worldName);
            CanvasController.Instance.PopStack();
        }
    }
}
