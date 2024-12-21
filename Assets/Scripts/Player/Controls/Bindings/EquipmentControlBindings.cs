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
                new ControlBinding(ControlConsts.CHANGE_TOOL, new List<KeyCode>{KeyCode.Mouse2}),
                new ControlBinding(ControlConsts.TOOL_SELECTOR,new List<KeyCode>{KeyCode.F}),
                new ControlBinding(ControlConsts.TOOL_OPTIONS, new List<KeyCode>{KeyCode.F, KeyCode.LeftShift}),
            };
        }
    }
}
