using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.Ports.UI {
    public abstract class ConduitPortUIController : MonoBehaviour, IConduitPortUIController
    {
        protected IConduit conduit;
        protected IConduitPort port;
        public virtual void initalize(IConduit conduit)
        {
            this.conduit = conduit;
            this.port = conduit.getPort();
        }
    }
}

