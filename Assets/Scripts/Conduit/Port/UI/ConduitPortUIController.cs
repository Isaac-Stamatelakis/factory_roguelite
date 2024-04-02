using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.Ports.UI {
    public abstract class ConduitPortUIController : MonoBehaviour, IConduitPortUIController
    {
        protected IPortConduit conduit;
        protected IConduitPort port;
        public virtual void initalize(IPortConduit conduit)
        {
            this.conduit = conduit;
            this.port = conduit.getPort();
        }
    }
}

