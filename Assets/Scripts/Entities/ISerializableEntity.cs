using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

namespace Entities {
    public interface ISerializableEntity
    {
        public SeralizedEntityData serialize();
    }
}

