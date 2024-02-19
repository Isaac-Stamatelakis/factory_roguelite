using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using UnityEngine.UI;

namespace ConduitModule.Ports.UI {
    public abstract class ConduitPortUIController<Conduit> : MonoBehaviour where Conduit : IConduit
    {
        [SerializeField] public Button toggleInsertButton;
        [SerializeField] public Button toggleExtractButton;
        public abstract void initalize(Conduit conduit);
    }

}
