using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class HammerTileValues
{
    private static readonly string defaultNatureSlabPath = "Assets/Sprites/Shapes/slab_shapes.png";
    private static readonly string defaultNatureSlantPath = "Assets/Sprites/Shapes/slant_shapes.png";
    private static readonly string perfectSlabPath = "Assets/Sprites/Shapes/perfect_slab.png";
    private static readonly string perfectSlantPath = "Assets/Sprites/Shapes/perfect_slant.png";
    private static readonly string stairPath = "Assets/Sprites/Shapes/stairs.png";
    public Texture2D Texture;
    public Texture2D Slab;
    public Texture2D Slant;
    public Texture2D Stairs;
    public Texture2D NatureSlabs;
    public Texture2D NatureSlants;
    public HammerTileValues()
    {
        
    }
    public async Task load() {
        var loadTasks = new Dictionary<string, Task<Texture2D>>();
        if (NatureSlabs == null) {
            loadTasks["slabs"] = Addressables.LoadAssetAsync<Texture2D>(defaultNatureSlabPath).Task;
        }
        if (NatureSlants == null) {
            loadTasks["slants"] = Addressables.LoadAssetAsync<Texture2D>(defaultNatureSlantPath).Task;
        }
        if (Slab == null) {
            loadTasks["perfectSlab"] = Addressables.LoadAssetAsync<Texture2D>(perfectSlabPath).Task;
        }
        if (Slant == null) {
            loadTasks["perfectSlant"] = Addressables.LoadAssetAsync<Texture2D>(perfectSlantPath).Task;
        }
        if (Stairs == null)
        {
            loadTasks["stairs"] = Addressables.LoadAssetAsync<Texture2D>(stairPath).Task;
        }
        await Task.WhenAll(loadTasks.Values);
        if (loadTasks.ContainsKey("slabs") && NatureSlabs == null) {
            NatureSlabs = await loadTasks["slabs"];
        }
        if (loadTasks.ContainsKey("slants") && NatureSlants == null) {
            NatureSlants = await loadTasks["slants"];
        }
        if (loadTasks.ContainsKey("perfectSlab") && Slab == null) {
            Slab = await loadTasks["perfectSlab"];
        }
        if (loadTasks.ContainsKey("perfectSlant") && Slant == null) {
            Slant = await loadTasks["perfectSlant"];
        }
        if (loadTasks.ContainsKey("stairs") && Stairs == null)
        {
            Stairs = await loadTasks["stairs"];
        }
    }
}
