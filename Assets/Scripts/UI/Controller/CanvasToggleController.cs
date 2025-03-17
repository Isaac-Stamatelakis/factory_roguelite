using Player.Controls;
using UnityEngine;

namespace UI.Controller
{
    public class CanvasToggleController : MonoBehaviour
    {
        [SerializeField] private GameObject jei;
        void Update()
        {
            if (Input.GetKeyDown(ControlUtils.GetPrefKeyCode(PlayerControl.HideJEI)))
            {
                jei.gameObject.SetActive(!jei.activeInHierarchy);
            }
        }
    }
}
