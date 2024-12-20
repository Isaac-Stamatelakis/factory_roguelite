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
                new ControlBinding(ControlConsts.JUMP, KeyCode.Space),
                new ControlBinding(ControlConsts.MOVE_LEFT, KeyCode.A),
                new ControlBinding(ControlConsts.MOVE_RIGHT, KeyCode.D),
                new ControlBinding(ControlConsts.MOVE_UP, KeyCode.W),
                new ControlBinding(ControlConsts.MOVE_DOWN, KeyCode.S)
            };
        }
    }
}
