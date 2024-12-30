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

public interface IConduitPortTileEntityAggregator : IConduitPortTileEntity
{
    public IConduitInteractable GetConduitInteractable(ConduitType conduitType);
}
public interface IConduitInteractable
{
    
}
public interface IEnergyConduitInteractable : IConduitInteractable{
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