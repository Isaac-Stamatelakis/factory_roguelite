using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Mobs {
    public interface ISerializableMobComponent
    {
        public string serialize();
        public void deseralize(string data);
    }
}

