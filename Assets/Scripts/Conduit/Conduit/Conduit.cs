using Conduits.Systems;
using Items;
using Tiles;
using UnityEngine;

namespace Conduits
{
    public abstract class Conduit<TConduitItem> : IConduit where TConduitItem : ConduitItem
    {
        protected Conduit(int x, int y, TConduitItem conduitItem, int state) {
            this.x = x;
            this.y = y;
            this.conduitItem = conduitItem;
            this.state = state;
        }
        private int x;
        private int y;
        private int state;
        private readonly TConduitItem conduitItem;
        private IConduitSystem conduitSystem;
        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public Vector2Int GetPosition()
        {
            return new Vector2Int(x, y);
        }

        public void SetX(int val)
        {
            x = val;
        }

        public void SetY(int val)
        {
            y = val;
        }

        public ConduitItem GetConduitItem()
        {
            return conduitItem;
        }

        public string GetId()
        {
            return conduitItem?.id;
        }

        public void SetConduitSystem(IConduitSystem newConduitSystem)
        {
            this.conduitSystem = newConduitSystem;
        }

        public IConduitSystem GetConduitSystem()
        {
            return conduitSystem;
        }

        public int GetActivatedState()
        {
            int returnState = state;
            if (conduitSystem != null && conduitSystem.IsActive())
            {
                returnState += (int)ConduitDirectionState.Active;
            }
            return returnState;
        }

        public int GetState()
        {
            return state;
        }

        public void SetState(int state)
        {
            this.state = state;
        }

        public void AddStateDirection(ConduitDirectionState directionState)
        {
            state += (int)directionState;
        }

        public void RemoveStateDirection(ConduitDirectionState directionState)
        {
            state -= (int)directionState;
        }

        public bool ConnectsDirection(ConduitDirectionState directionState)
        {
            return (state & (int) directionState) != 0;
        }
    }
}