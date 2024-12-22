using System.Collections.Generic;
using UI.PauseScreen;
using UnityEngine;

namespace UI
{
    public class MainCanvasController : CanvasController
    {
        [SerializeField] private PauseScreenUI pauseScreenUIPrefab;
        public override void EmptyListen()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DisplayObject(Instantiate(pauseScreenUIPrefab.gameObject));
            }
        }
    }
}
