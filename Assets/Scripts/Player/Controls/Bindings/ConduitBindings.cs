using System.Collections.Generic;
using UnityEngine;

namespace Player.Controls.Bindings
{
    public class ConduitControlBindings : ControlBindingCollection
    {
       
        protected override List<ControlBinding> GetBindings()
        {
            return new List<ControlBinding>
            {
                new (PlayerControl.OpenConduitOptions, new List<KeyCode>{KeyCode.N}),
                new (PlayerControl.TerminateConduitGroup, new List<KeyCode>{KeyCode.H, KeyCode.LeftShift}),
                new (PlayerControl.SwitchConduitPortView, new List<KeyCode>{KeyCode.J}),
                new (PlayerControl.ChangeConduitViewMode, new List<KeyCode>{KeyCode.K}),
            };
        }
    }
}
