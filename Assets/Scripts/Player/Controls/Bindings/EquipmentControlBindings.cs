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
                new ControlBinding(ControlConsts.CHANGE_TOOL, KeyCode.Mouse2),
                new ControlBinding(ControlConsts.TOOL_SELECTOR, KeyCode.G),
                new ControlBinding(ControlConsts.TOOL_OPTIONS, KeyCode.G),
            };
        }
    }
}
