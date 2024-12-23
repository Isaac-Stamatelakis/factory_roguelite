using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using PlayerModule;

namespace DevTools.Structures {
    public class StructureSceneBackButton : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            SceneManager.LoadScene("DevTools");
            PlayerContainer.reset();
        }
    }
}

