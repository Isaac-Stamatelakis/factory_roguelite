using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevTools.Structures {
    public static class StructureGeneratorOptionFactory
    {
        public static StructureGenerationOption createOption(StructureGenOptionType option, string id) {
            switch (option) {
                case StructureGenOptionType.Empty:
                    return null;
                case StructureGenOptionType.Fill:
                    return new FillStructureOption(id);
                case StructureGenOptionType.Border:
                    return new BorderStructureOption(id);
                default:
                    return null;
            }
        }
    }
}

