using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Transmutable {
    public interface ITransmutableItem
    {
        public TransmutableItemMaterial getMaterial();
        public TransmutableItemState getState();
        public void setState(TransmutableItemState state);
        public void setMaterial(TransmutableItemMaterial material);
    }

}