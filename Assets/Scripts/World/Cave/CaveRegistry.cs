using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace WorldModule.Caves {
    public class CaveRegistry
    {
        private static CaveRegistry instance;
        private static Dictionary<string, Cave> idCaveDict;
        private CaveRegistry() {
            idCaveDict = new Dictionary<string, Cave>();
            Cave[] caves = Resources.LoadAll<Cave>("");
            foreach (Cave cave in caves) {
                if (idCaveDict.ContainsKey(cave.Id)) {
                    Debug.LogError("Duplicate ids for caves " + cave.name + " and " + idCaveDict[cave.Id].name);
                    continue;
                }
                idCaveDict[cave.Id] = cave;
            }
        }
        public static CaveRegistry getInstance() {
            if (instance == null) {
                instance = new CaveRegistry();
            }
            return instance;
        }

        public static Cave getCave(string id) {
            getInstance();
            if (idCaveDict.ContainsKey(id)) {
                return idCaveDict[id];
            }
            return null;
        }

        public static Cave[] getAll() {
            getInstance();
            return idCaveDict.Values.ToArray();
        }

    }
}

