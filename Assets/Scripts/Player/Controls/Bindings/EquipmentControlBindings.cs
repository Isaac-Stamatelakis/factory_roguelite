using System.Collections.Generic;
using UnityEngine;

namespace Player.Controls.Bindings
{
    public class EquipmentControlBindings : ControlBindingCollection
    {
        protected override List<ControlBinding> GetBindings()
        {
            return new List<ControlBinding>
            {
                new (PlayerControl.SwitchToolMode, new List<KeyCode>{KeyCode.C}),
                new (PlayerControl.AutoSelect, new List<KeyCode>{KeyCode.P}),
                new (PlayerControl.SwapRobotLoadOut, new List<KeyCode>{KeyCode.T}),
                new (PlayerControl.SwapToolLoadOut, new List<KeyCode>{KeyCode.T,KeyCode.LeftControl}),
            };
        }
    }
}
