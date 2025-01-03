using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;
using Newtonsoft.Json;

namespace TileEntity.Instances.Signal {
    [CreateAssetMenu(fileName = "E~New Clock", menuName = "Tile Entity/Signal/Clock")]
    public class SignalClock : TileEntityObject, IManagedUITileEntity
    {
        public TileEntityUIManager UIManager;
        public int ActiveDuration = 5;
        public int DefaultTime = 25;
        public int MinTime = 5;
        public int MaxTime = 1000;
        public ConduitPortLayout ConduitLayout;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignalClockInstance(this,tilePosition,tileItem,chunk);
        }

        public TileEntityUIManager getUIManager()
        {
            return UIManager;
        }
    }

    public class SignalClockInstance : TileEntityInstance<SignalClock>, IPlaceInitializable, IConduitPortTileEntity, ISignalConduitInteractable, ISerializableTileEntity, ITickableTileEntity, IRightClickableTileEntity
    {
        public ClockData ClockData;
        public SignalClockInstance(SignalClock tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return ClockData.Counter < TileEntityObject.ActiveDuration;
        }
        

        public void InsertSignal(bool signal, Vector2Int portPosition)
        {
            
        }

        public void onRightClick()
        {
            TileEntityObject.UIManager.display<SignalClockInstance,SignalClockUI>(this);
        }
        
        public string serialize()
        {
            return JsonConvert.SerializeObject(ClockData);
        }

        public void tickUpdate()
        {
            if (!ClockData.Active) {
                return;
            }
            ClockData.Counter++;
            if (ClockData.Counter > ClockData.Time) {
                ClockData.Counter = 0;
            }
        }

        public void unserialize(string data)
        {
            ClockData = JsonConvert.DeserializeObject<ClockData>(data);
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public void PlaceInitialize()
        {
            ClockData = new ClockData(TileEntityObject.DefaultTime,0,true);
        }
    }

    public class ClockData {
        public int Time;
        public int Counter;
        public bool Active;

        public ClockData(int time, int counter, bool active)
        {
            Time = time;
            Counter = counter;
            this.Active = active;
        }
    }
}

