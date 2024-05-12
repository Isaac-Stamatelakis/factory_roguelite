using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Mobs {
    public class SerializedMobData
    {
        public string id;
        public Dictionary<string,string> componentData;
        public SerializedMobData(string id, Dictionary<string, string> componentData)
        {
            this.id = id;
            this.componentData = componentData;
        }
    }
}

