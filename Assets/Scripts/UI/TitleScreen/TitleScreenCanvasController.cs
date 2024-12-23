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

        public override void EmptyListen()
        {
            
        }

        public override void ListenKeyPresses()
        {
            
        }
    }
}
