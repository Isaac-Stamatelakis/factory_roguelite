using Conduits;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

namespace Conduit.Port.UI
{
    public class ConduitPortConnectionUI<TConduitPortData> : MonoBehaviour 
        where TConduitPortData : ConduitPortData
    {
        [SerializeField] private Toggle toggleActiveButton;
        [SerializeField] private Button colorButton;
        [SerializeField] private Image disabledCover;

        public void Display(IPortConduit conduit, TConduitPortData portData)
        {
            if (portData == null)
            {
                disabledCover.gameObject.SetActive(true);
                return;
            }
            toggleActiveButton.onValueChanged.AddListener((bool state) =>
            {
                portData.Enabled = state;
                conduit.GetConduitSystem().Rebuild(); // TODO make this more efficent
            });
        }
    }
}
