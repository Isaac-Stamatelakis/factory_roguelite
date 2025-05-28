using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Recipe.Objects.Restrictions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using World.Cave.Registry;

namespace TileEntity {
    /// <summary>
    /// TileEntityObjects which implement this interface will have their UIElements automatically loaded and unloaded by the TileEntityAssetRegistry.
    /// Further their instances will instantiate their UIElement when right-clicked.
    /// </summary>
    public interface IUITileEntity {
        public AssetReference GetUIAssetReference();
    }

    /// <summary>
    /// TileEntityObjects which implement this interface will have their Prefabs automatically loaded and unloaded by the TileEntityAssetRegistry.
    /// </summary>
    public interface IAssetTileEntity
    {
        public AssetReference GetAssetReference();
    }
    
    /// <summary>
    /// TileEntityObjects which implement will have the method 'TickUpdate' called every tick
    /// </summary>
    public interface ITickableTileEntity : ISoftLoadableTileEntity
    {
        public void TickUpdate();
    }
    
    /// <summary>
    /// TileEntityObjects which implement will have the method 'PlaceInitialize' called the first time they are placed
    /// <remarks>The method 'PlaceInitialize' is also called if an error occurs during a call of 'Deserialize' from the interface ISeriazableTileEntity</remarks>
    /// </summary>
    public interface IPlaceInitializable
    {
        public void PlaceInitialize();
    }

    /// <summary>
    /// TileEntityObjects which implement this interface can be their instances climbed by the player
    /// </summary>
    public interface IClimableTileEntity {
        public float GetSpeed();
    }

    /// <summary>
    /// TileEntities which implement this interface can not be clicked if the method 'CanRightClick' returns false
    /// </summary>
    public interface IConditionalRightClickableTileEntity
    {
        public bool CanRightClick();
    }
    /// <summary>
    /// TileEntities which implement this interface can not be clicked if the system is locked.
    /// </summary>
    public interface ILockUnInteractableRightClickTileEntity
    {
        
    }
    /// <summary>
    /// TileEntities which implement this interface will have the method 'OnLeftClick' called if 'CanBreak' returns false and the player left clicks them.
    /// </summary>
    public interface ILeftClickableTileEntity {
        public void OnLeftClick();
        public bool CanInteract();
        public bool CanBreak();
    }
    /// <summary>
    /// TileEntities which implement this interface will have the method OnRightClick called when the player right clicks them.
    /// <remarks>TileEntities whose object implements the interface 'IUITileEntity' will instantiate a UIElement when clicked, unless LShift is held</remarks>
    /// </summary>
    public interface IRightClickableTileEntity
    {
        public void OnRightClick();
    }

    public interface IStopPlayerRightClickableTileEntity : IRightClickableTileEntity
    {
        
    }
    
    /// <summary>
    /// TileEntities which implement this interface will have the method Serialize saving any data, and the method
    /// 'Unserialize' called when created.
    /// </summary>
    public interface ISerializableTileEntity {
        public string Serialize();
        public void Unserialize(string data);
    }

    /// <summary>
    /// TileEntities which implement this interface will have the method 'Load' called when the player is within load range of them,
    /// and the method 'Unload' called when unloaded after the player is far away from them.
    /// </summary>
    public interface ILoadableTileEntity {
        public void Load();
        public void Unload();
    }

    /// <summary>
    /// TileEntities which implement this interface will have the method 'OnBreak' called when the TileEntities tile is broken
    /// </summary>
    public interface IBreakActionTileEntity {
        public void OnBreak();
    }
    /// <summary>
    /// TileEntities which implement this interface will have the method 'AssembleMultiBlock' called during the MultiBlock assembly phase of system loading.
    /// </summary>
    public interface IMultiBlockTileEntity {
        public void AssembleMultiBlock();
    }
    
    /// <summary>
    /// TileEntities which implement this interface will have the method 'OnCaveRegistryLoaded' called when the CaveRegistry finishes async loading
    /// </summary>
    public interface IOnCaveRegistryLoadActionTileEntity
    {
        public void OnCaveRegistryLoaded(CaveRegistry caveRegistry);
    }
    

    /// <summary>
    /// Implemented by other blueprint interfaces
    /// </summary>
    public interface IBluePrintModifiedTileEntity
    {
        
    }

    /// <summary>
    /// TileEntities which implement this interface will have the method 'OnPlaceInitialize' called when copy pasted through compact machine blueprinting
    /// </summary>
    public interface IBluePrintPlaceInitializedTileEntity : IBluePrintModifiedTileEntity, IPlaceInitializable
    {
        
    }

    /// <summary>
    /// TileEntities which implement this interface will have the method 'OnPlaceInitialize' called when copied through compact machine blueprinting
    /// </summary>
    public interface IOnBluePrintActionTileEntity : IBluePrintModifiedTileEntity
    {
        public void OnBluePrint();
    }
    
    /// <summary>
    /// A tile entity that inherients this interface will only be added to the conduit system if it is currently loaded by the player.
    /// It will also not be added to tickable tile entities unless the system is loaded by the player.
    /// <example>Useful for doors, lamps, etc which do not require conduit interaction unless the player is near</example>
    /// <remarks>Most TileEntities are soft loadable by default when tickable or conduit interactable, this interface provides a way to cancel it out</remarks>
    /// </summary>
    public interface IOverrideSoftLoadTileEntity
    {
    
    }
    /// <summary>
    /// TileEntities which implement this interface will be loaded when their ClosedChunkSystem is not loaded.
    /// <remarks>A TileEntity which implements 'ISystemLoadedConduitPortTileEntity' will override this interface and cause the TileEntity to not be loaded unless their system is loaded</remarks>
    /// </summary>
    public interface ISoftLoadableTileEntity : ITileEntityInstance {

    }

    public interface IRecipeRestrictionTileEntity
    {
        public void AddPassRestriction(BooleanRecipeRestriction recipeRestriction);
        public void RemoveRestriction(BooleanRecipeRestriction recipeRestriction);
    }

    /// <summary>
    /// TileEntities which implement this interface will show a text preview when the player hovers over them.
    /// <remarks>Holding LShift will hide this text preview</remarks>
    /// </summary>
    public interface IWorldToolTipTileEntity : ITileEntityInstance
    {
        public string GetTextPreview();
    }
    /// <summary>
    /// TileEntities which implement are interactable through quick stack, give all, and take all inventory utils from player
    /// </summary>
    public interface ISingleSolidInventoryTileEntity : ITileEntityInstance
    {
        public List<ItemSlot> GetInventory();
    }
    /// <summary>
    /// Similar to ISingleSolidInventory but with for tile entities with more complex inventories.
    /// TileEntities which implement are interactable through quick stack, give all, and take all inventory utils from player
    /// </summary>
    public interface IMultiSolidItemStorageTileEntity : ITileEntityInstance
    {
        public List<ItemSlot> GetInputInventory();
        public List<List<ItemSlot>> GetOutputInventories();
    }

    public interface IInventoryUpdateListener
    {
        public void InventoryUpdate();
    }
    

}
