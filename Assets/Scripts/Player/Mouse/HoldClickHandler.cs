using Robot.Tool;
using Robot.Tool.Instances;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Player.Mouse
{
    public interface IPlayerClickHandler
    {
        public void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public void TerminateClickHold(MouseButtonKey mouseButtonKey);
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
        public bool IsActive => active;
        private float lastUse;
        public float StoredTime => lastUse;
        private IAcceleratedClickHandler acceleratedClickHandler;
        private bool resetCounterOnClick;
        public HoldClickHandler(IPlayerClickHandler clickHandler, MouseButtonKey mouseButtonKey)
        {
            this.clickHandler = clickHandler;
            this.mouseButtonKey = mouseButtonKey;
            lastUse = Time.time;
            acceleratedClickHandler = clickHandler as IAcceleratedClickHandler;
            resetCounterOnClick = clickHandler is IClickSpammableTool;
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
                if (counter > 0 && !resetCounterOnClick) return;
               
                if (Input.GetMouseButtonDown(mouseButtonKey.ToMouseButton()))
                {
                    clickHandler.ClickUpdate(mousePosition,mouseButtonKey);
                }
                counter = 0;
                return;
            }
            
            if (DevMode.Instance.noBreakCooldown)
            {
                counter = 100000f;
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
            clickHandler.TerminateClickHold(mouseButtonKey);
            lastUse = Time.time;
            active = false;
        }
    }
}