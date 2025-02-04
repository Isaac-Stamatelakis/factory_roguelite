using Player.Controls;
using UnityEngine;

namespace UI.Controller
{
    public class CanvasToggleController : MonoBehaviour
    {
        [SerializeField] private GameObject jei;
        void Update()
        {
            if (Input.GetKeyDown(ControlUtils.GetPrefKeyCode(ControlConsts.HIDE_JEI)))
            {
                jei.gameObject.SetActive(!jei.activeInHierarchy);
            }
        }
    }
}
