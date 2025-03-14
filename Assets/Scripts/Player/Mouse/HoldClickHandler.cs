using Robot.Tool;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Player.Mouse
{
    public interface IPlayerClickHandler
    {
        public void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public void TerminateClickHold();
        public void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time);
    }

    public interface IAcceleratedClickHandler
    {
        public float GetSpeedMultiplier();
    }
    public class HoldClickHandler
    {
        private float counter = 0f;
        private readonly IPlayerClickHandler clickHandler;
        private MouseButtonKey mouseButtonKey;
        private bool active;
        private float lastUse;
        private IAcceleratedClickHandler acceleratedClickHandler;
        public HoldClickHandler(IPlayerClickHandler clickHandler, MouseButtonKey mouseButtonKey)
        {
            this.clickHandler = clickHandler;
            this.mouseButtonKey = mouseButtonKey;
            lastUse = Time.time;
            acceleratedClickHandler = clickHandler as IAcceleratedClickHandler;
        }

        public void Tick(Vector2 mousePosition, bool blockDestruction)
        {
            if (blockDestruction && clickHandler is IDestructiveTool) return; 
            if (!active)
            {
                clickHandler.BeginClickHold(mousePosition, mouseButtonKey);
                active = true;
                float timeSinceLastUse = Time.time - lastUse;
                counter -= timeSinceLastUse;
                if (!(counter <= 0)) return;
                
                if (Input.GetMouseButtonDown((int)mouseButtonKey)) clickHandler.ClickUpdate(mousePosition,mouseButtonKey);
                counter = 0;
                return;
            }
            
            if (DevMode.Instance.noBreakCooldown)
            {
                clickHandler.HoldClickUpdate(mousePosition, mouseButtonKey, int.MaxValue);
                counter = 0;
                return;
            }

            float multipler = acceleratedClickHandler?.GetSpeedMultiplier() ?? 1;
            counter += multipler * Time.deltaTime;
            if (clickHandler.HoldClickUpdate(mousePosition,mouseButtonKey, counter))
            {
                counter = 0f;
            }
        }

        public void Terminate()
        {
            if (!active) return;
            clickHandler.TerminateClickHold();
            lastUse = Time.time;
            active = false;
        }
    }
}