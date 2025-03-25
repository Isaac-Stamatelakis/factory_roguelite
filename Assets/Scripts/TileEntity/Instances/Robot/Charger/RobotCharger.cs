using System;
using Chunks;
using Conduits;
using Conduits.Ports;
using Player;
using TileEntity.Instances.Storage;
using UnityEngine;

namespace TileEntity.Instances.Robot.Charger
{
    [CreateAssetMenu(fileName = "New Robot Charger", menuName = "Tile Entity/Robot/Charger")]
    public class RobotCharger : TileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new RobotChargerInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class RobotChargerInstance : TileEntityInstance<RobotCharger>, IEnergyPortTileEntityAggregator, ISerializableTileEntity, IPlaceInitializable, ITickableTileEntity, IConduitPortTileEntity
    {
        public EnergyInventory EnergyInventory;
        private PlayerRobot playerRobot;
        public RobotChargerInstance(RobotCharger tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }

        public IEnergyConduitInteractable GetEnergyConduitInteractable()
        {
            return EnergyInventory;
        }

        public string Serialize()
        {
            return EnergyInventory.Energy.ToString();
        }

        public void Unserialize(string data)
        {
            Initialize(System.Convert.ToUInt64(data));
        }

        public void PlaceInitialize()
        {
            Initialize(0);
        }

        private void Initialize(ulong energy)
        {
            EnergyInventory = new EnergyInventory(energy, ulong.MaxValue); // This thing is an energy sink
            playerRobot = PlayerManager.Instance.GetPlayer().PlayerRobot;
        }

        public void TickUpdate()
        {
            playerRobot.GiveEnergy(EnergyInventory.Energy);
        }
    }
}
