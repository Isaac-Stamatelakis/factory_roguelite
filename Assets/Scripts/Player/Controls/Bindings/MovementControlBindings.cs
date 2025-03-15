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
                new(PlayerControl.Jump, new List<KeyCode>{KeyCode.Space}),
                new(PlayerControl.MoveLeft, new List<KeyCode>{KeyCode.A}),
                new(PlayerControl.MoveRight, new List<KeyCode>{KeyCode.D}),
                new(PlayerControl.MoveUp, new List<KeyCode>{KeyCode.W}),
                new (PlayerControl.MoveDown, new List<KeyCode>{KeyCode.S}),
                new (PlayerControl.Recall, new List<KeyCode>{KeyCode.R}),
                new (PlayerControl.Teleport, new List<KeyCode>{KeyCode.Z})
            };
        }
    }
}
