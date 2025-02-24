using System.Collections.Generic;
using UnityEngine;

namespace Player.Controls.Bindings
{
    public class MovementControlBindings : ControlBindingCollection
    {
        protected override List<ControlBinding> GetBindings()
        {
            return new List<ControlBinding>
            {
                new(ControlConsts.JUMP, new List<KeyCode>{KeyCode.Space}),
                new(ControlConsts.MOVE_LEFT, new List<KeyCode>{KeyCode.A}),
                new(ControlConsts.MOVE_RIGHT, new List<KeyCode>{KeyCode.D}),
                new(ControlConsts.MOVE_UP, new List<KeyCode>{KeyCode.W}),
                new (ControlConsts.MOVE_DOWN, new List<KeyCode>{KeyCode.S}),
                new (ControlConsts.RECALL, new List<KeyCode>{KeyCode.R}),
                new (ControlConsts.TELEPORT, new List<KeyCode>{KeyCode.Z})
            };
        }
    }
}
