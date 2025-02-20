using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using TileEntity.Instances.Matrix;
using Conduits.Systems;
using Conduits.Ports;

public interface IConduitPortTileEntity : ISoftLoadableTileEntity {
    public ConduitPortLayout GetConduitPortLayout();
}


/// <summary>
/// A tile entity that inherients this interface will only be added to the conduit system if it is currently loaded by the player
/// <example>Useful for doors, lamps, etc which do not require conduit interaction unless the player is near</example>
/// </summary>
public interface ISystemLoadedConduitPortTileEntity : IConduitPortTileEntity
{
    
}

public interface IConduitPortTileEntityAggregator : IConduitPortTileEntity
{
    public IConduitInteractable GetConduitInteractable(ConduitType conduitType);
}
public interface IConduitInteractable
{
    
}

public interface IRefreshOnItemExtractTileEntity
{
    public void RefreshOnExtraction();
}
public interface IEnergyConduitInteractable : IConduitInteractable{
    /// <summary>
    /// Inputs energy into a container
    /// </summary>
    /// <param name="energy"></param>
    /// <param name="portPosition"></param>
    /// <returns>Energy taken</returns>
    public ulong InsertEnergy(ulong energy, Vector2Int portPosition);
    public ref ulong GetEnergy(Vector2Int portPosition);
}
public interface ISignalConduitInteractable : IConduitInteractable{
    public bool ExtractSignal(Vector2Int portPosition);
    public void InsertSignal(bool active,Vector2Int portPosition);
}


public interface IMatrixConduitInteractable : IConduitInteractable {
    public void SyncToController(ItemMatrixControllerInstance matrixController);
    public void SyncToSystem(MatrixConduitSystem matrixConduitSystem);
    public void RemoveFromSystem();
}