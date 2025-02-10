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
                new (ControlConsts.OPEN_CONDUIT_OPTIONS, new List<KeyCode>{KeyCode.N}),
                new (ControlConsts.SWITCH_CONDUIT_PLACMENT_MODE, new List<KeyCode>{KeyCode.H}),
                new (ControlConsts.TERMINATE_CONDUIT_GROUP, new List<KeyCode>{KeyCode.H, KeyCode.LeftShift}),
                new (ControlConsts.SWITCH_CONDUIT_PORT_VIEW, new List<KeyCode>{KeyCode.J}),
            };
        }
    }
}
