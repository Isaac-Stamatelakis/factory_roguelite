using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using TileEntity.Instances.Matrix;
using Conduits.Systems;
using Conduits.Ports;

/// <summary>
/// TileEntities that implement this interface will be interactable by conduits as defined by their ConduitPortLayout
/// </summary>
public interface IConduitPortTileEntity : ISoftLoadableTileEntity {
    public ConduitPortLayout GetConduitPortLayout();
}

/// <summary>
/// Alternative interface to the individual PortTileEntityAggregators
/// </summary>
public interface IConduitPortTileEntityAggregator : IConduitPortTileEntity
{
    public IConduitInteractable GetConduitInteractable(ConduitType conduitType);
}

public interface IEnergyPortTileEntityAggregator
{
    public IEnergyConduitInteractable GetEnergyConduitInteractable();
}
public interface ISolidItemPortTileEntityAggregator
{
    public IItemConduitInteractable GetSolidItemConduitInteractable();
}

public interface IFluidItemPortTileEntityAggregator
{
    public IItemConduitInteractable GetFluidItemConduitInteractable();
}

public interface ISignalPortTileEntityAggregator
{
    public ISignalConduitInteractable GetSignalConduitInteractable();
}

public interface IMatrixPortTileEntityAggregator : IConduitPortTileEntity
{
    public IMatrixConduitInteractable GetMatrixConduitInteractable();
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
    public ulong GetEnergy(Vector2Int portPosition);
    public void SetEnergy(ulong energy, Vector2Int portPosition);
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