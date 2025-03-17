using System.Collections.Generic;
using UnityEngine;

namespace Player.Controls.Bindings
{
    public class MiscBindings : ControlBindingCollection
    {
       
        protected override List<ControlBinding> GetBindings()
        {
            return new List<ControlBinding>
            {
                new (PlayerControl.HideUI, new List<KeyCode>{KeyCode.F1}),
                new (PlayerControl.HideJEI, new List<KeyCode>{KeyCode.F2}),
            };
        }
    }
}
