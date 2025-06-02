using UnityEngine;

namespace Item.Transmutation.ShaderOptions
{
    public abstract class TransmutationShaderOptionObject : ScriptableObject
    {
        public abstract void Apply(Material material);
    }
}
