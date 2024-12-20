using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using TileEntityModule.Instances.Matrix;
using Conduits.Systems;
using Conduits.Ports;

public interface IConduitInteractable : ISoftLoadableTileEntity {
    public ConduitPortLayout getConduitPortLayout();
}
public interface IItemConduitInteractable : IConduitInteractable {
    
}
public interface ISolidItemConduitInteractable : IItemConduitInteractable {
    public ItemSlot extractSolidItem(Vector2Int portPosition);
    public void insertSolidItem(ItemSlot itemSlot,Vector2Int portPosition);
}

public interface IEnergyConduitInteractable : IConduitInteractable {
    public int insertEnergy(int energy, Vector2Int portPosition);
    public ref int getEnergy(Vector2Int portPosition);
}
public interface ISignalConduitInteractable : IConduitInteractable {
    public bool extractSignal(Vector2Int portPosition);
    public void insertSignal(bool active,Vector2Int portPosition);
}
public interface IFluidConduitInteractable : IItemConduitInteractable {
    public ItemSlot extractFluidItem(Vector2Int portPosition);
    public void insertFluidItem(ItemSlot itemSlot,Vector2Int portPosition);
}

public interface IMatrixConduitInteractable : IConduitInteractable {
    public void syncToController(ItemMatrixControllerInstance matrixController);
    public void syncToSystem(MatrixConduitSystem matrixConduitSystem);
    public void removeFromSystem();
}