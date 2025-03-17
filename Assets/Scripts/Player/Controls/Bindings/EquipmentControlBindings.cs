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
                new (PlayerControl.ChangeTool, new List<KeyCode>{KeyCode.Mouse2}),
                new (PlayerControl.ToolSelector,new List<KeyCode>{KeyCode.F}),
                new (PlayerControl.ToolOptions, new List<KeyCode>{KeyCode.F, KeyCode.LeftShift}),
                new (PlayerControl.SwapRobotLoadOut, new List<KeyCode>{}),
                new (PlayerControl.SwapToolLoadOut, new List<KeyCode>{}),
            };
        }
    }
}
