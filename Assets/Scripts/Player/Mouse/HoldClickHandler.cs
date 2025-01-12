using Robot.Tool;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Player.Mouse
{
    public interface IPlayerClickHandler
    {
        public void BeginClickHold(Vector2 mousePosition);
        public void TerminateClickHold();
        public void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey);
        public bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time);
    }
    public class HoldClickHandler
    {
        private float counter = 0f;
        private readonly IPlayerClickHandler clickHandler;
        private readonly int mouseIndex;
        private MouseButtonKey mouseButtonKey;
        private bool active;
        private float lastUse;
        public HoldClickHandler(IPlayerClickHandler clickHandler, MouseButtonKey mouseButtonKey)
        {
            this.clickHandler = clickHandler;
            this.mouseIndex = (int)mouseButtonKey;
            lastUse = Time.time;
        }

        public void Tick(Vector2 mousePosition)
        {
            if (!active)
            {
                clickHandler.BeginClickHold(mousePosition);
                active = true;
                float timeSinceLastUse = Time.time - lastUse;
                counter += timeSinceLastUse;
                if (counter <= 0)
                {
                    clickHandler.ClickUpdate(mousePosition,mouseButtonKey);
                    counter = 0;
                }
                
                return;
            }
            
            if (DevMode.Instance.noBreakCooldown)
            {
                clickHandler.HoldClickUpdate(mousePosition, mouseButtonKey, int.MaxValue);
                return;
            }
            counter += Time.deltaTime;
            if (clickHandler.HoldClickUpdate(mousePosition,mouseButtonKey, counter))
            {
                counter = 0f;
            }
        }

        public void Terminate()
        {
            clickHandler.TerminateClickHold();
            lastUse = Time.time;
            active = false;
        }
    }
}