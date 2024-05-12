using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule;

namespace Entities {
    public interface ISerializableEntity
    {
        public SeralizedEntityData serialize();
    }
}

