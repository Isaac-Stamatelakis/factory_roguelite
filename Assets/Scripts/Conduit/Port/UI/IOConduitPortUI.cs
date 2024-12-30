using Conduits;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace Conduit.Port.UI
{
    public class IOConduitPortUI : MonoBehaviour
    {
        [SerializeField] private ConduitPortConnectionUI inputConnectionUI;
        [SerializeField] private ConduitPortConnectionUI outputConnectionUI;

        public void Display(IOConduitPort ioConduitPort, IPortConduit conduit)
        {
            if (ioConduitPort == null) return;
            inputConnectionUI.Display(conduit,ioConduitPort.GetPortData(PortConnectionType.Input),PortConnectionType.Input);
            outputConnectionUI.Display(conduit,ioConduitPort.GetPortData(PortConnectionType.Output),PortConnectionType.Output);
        }
    }
}
