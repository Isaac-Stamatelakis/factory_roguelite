using System;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.UI;

namespace Conduit.Placement.LoadOut
{
    public class ConduitDropDownColorerUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        public void Start()
        {
            LoadOutConduitType loadOutConduitType = (LoadOutConduitType)(transform.GetSiblingIndex() - 1);
            ConduitType conduitType = loadOutConduitType.ToConduitType();
            image.color = ConduitPortFactory.GetConduitPortColor(conduitType);
        }
    }
}
