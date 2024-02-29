using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace ConduitModule.ConduitSystemModule {
    public class EnergyConduitSystem : ConduitSystem<EnergyConduitInputPort, EnergyConduitOutputPort>
    {
        public EnergyConduitSystem(string id) : base(id)
        {
        }

        public override void addPort(IConduit conduit)
        {
            IConduitPort conduitPort = conduit.getPort();
            if (conduitPort == null) {
                return;
            }
            if (conduitPort is not EnergyConduitPort energyConduitPort) {
                Debug.LogError("Energy Conduit System recieved non energy conduit port");
                return;
            }
            object input = energyConduitPort.getInputPort();
            if (input != null) {
                addInputPort((EnergyConduitInputPort) energyConduitPort.getInputPort());
            } 
            object output = energyConduitPort.GetOutputPort();
            if (output != null) {
                addOutputPort((EnergyConduitOutputPort) energyConduitPort.GetOutputPort());
            }
        }

        public override bool iterateTickUpdate(EnergyConduitOutputPort outputPort, List<EnergyConduitInputPort> inputPort)
        {
            /*
            int toInsert = outputPort.extract();
            if (toInsert == 0) {
                return true;
            }
            int amount = Mathf.Min(toInsert,outputPort);
            ItemSlot tempItemSlot = new ItemSlot(itemObject: toInsert.itemObject, amount:amount,nbt: toInsert.nbt);
            foreach (ItemConduitInputPort<Interactable,Filter> itemConduitInputPort in inputPorts) {
                if (itemConduitInputPort.TileEntity.Equals(outputPort.TileEntity)) {
                    continue;
                }
                itemConduitInputPort.insert(tempItemSlot);
                if (tempItemSlot.amount == 0) {
                    break;
                } else if (toInsert.amount < 0) {
                    Debug.LogError("Something went wrong when inserting items. Got negative amount '" + tempItemSlot.amount + "'");
                    break;
                }
            }
            toInsert.amount -= amount-tempItemSlot.amount;
            if (toInsert.amount <= 0) {
                toInsert.itemObject = null;
                if (toInsert.amount < 0) {
                    Debug.LogError("Negative amount something went wrong inserting item conduit system");
                }
            }
            */
            return true;
        }

        protected override void addInputPortPostProcessing(EnergyConduitInputPort inputPort)
        {
            // No post processing
        }
    }
}

