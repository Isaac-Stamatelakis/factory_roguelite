using System.Collections.Generic;
using UnityEngine;

namespace Player.Controls.Bindings
{
    public class MiscBindings : ControlBindingCollection
    {
       
        protected override List<ControlBinding> GetBindings()
        {
            return new List<ControlBinding>
            {
                new (ControlConsts.HIDE_UI, new List<KeyCode>{KeyCode.F1}),
                new (ControlConsts.HIDE_JEI, new List<KeyCode>{KeyCode.F2}),
            };
        }
    }
}
