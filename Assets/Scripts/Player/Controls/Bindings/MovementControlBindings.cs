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
                new ControlBinding(ControlConsts.JUMP, new List<KeyCode>{KeyCode.Space}),
                new ControlBinding(ControlConsts.MOVE_LEFT, new List<KeyCode>{KeyCode.A}),
                new ControlBinding(ControlConsts.MOVE_RIGHT, new List<KeyCode>{KeyCode.D}),
                new ControlBinding(ControlConsts.MOVE_UP, new List<KeyCode>{KeyCode.W}),
                new ControlBinding(ControlConsts.MOVE_DOWN, new List<KeyCode>{KeyCode.S})
            };
        }
    }
}
