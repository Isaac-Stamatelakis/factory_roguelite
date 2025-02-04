using System.Collections.Generic;
using UnityEngine;

namespace Player.Controls.Bindings
{
    public class MiscBindings : ControlBindingCollection
    {
        public const string HIDE_UI = "hide_ui";
        protected override List<ControlBinding> GetBindings()
        {
            return new List<ControlBinding>
            {
                new (HIDE_UI, new List<KeyCode>{KeyCode.F1}),
            };
        }
    }
}
