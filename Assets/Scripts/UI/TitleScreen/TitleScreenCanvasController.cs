using System;
using UnityEngine;

namespace UI.TitleScreen
{
    public class TitleScreenCanvasController : CanvasController
    {
        [SerializeField] private TitleScreenUI titleScreenUI;

        public void Start()
        {
            DisplayObject(titleScreenUI.gameObject);
        }

        protected override void OnInactiveEscapePress()
        {
            // Do nothing :)
        }

        protected override void OnEscapePress()
        {
            PopStack();
        }
    }
}
