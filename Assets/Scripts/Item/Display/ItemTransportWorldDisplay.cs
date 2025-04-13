using System;
using System.Collections.Generic;
using System.Linq;
using Item.Slot;
using JetBrains.Annotations;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Item.Display
{
    public class ItemTransportWorldDisplay : MonoBehaviour
    {
        private List<ItemWorldDisplay> worldDisplays;
        private Vector2 target;
        private Transform movingTarget;
        private float speed;
        
        public void DisplayTransport(Vector2 origin, Vector2 target, List<ItemSlot> itemSlots, int maxSample)
        {
            int sampleSize = Mathf.Min(maxSample, itemSlots.Count);
            var sample = itemSlots.OrderBy(x => UnityEngine.Random.Range(0,1f)).Take(sampleSize).ToList();
            DisplayTransport(origin, target, sample);
        }
        public void DisplayTransport(Vector2 origin, Vector2 target, List<ItemSlot> itemSlots)
        {
            speed = 2;
            this.target = target;
            transform.position = origin;
            worldDisplays = new List<ItemWorldDisplay>();
            const float DEFAULT_RADIUS = 0.1f;
            
            for (var index = 0; index < itemSlots.Count; index++)
            {
                var itemSlot = itemSlots[index];
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                GameObject itemObject = new GameObject($"Travel:{itemSlot.itemObject.name}");
                itemObject.transform.SetParent(transform, false);
                if (itemSlots.Count > 1)
                {
                    float theta = 2 * index * Mathf.PI / itemSlots.Count;
                    itemObject.transform.localPosition = DEFAULT_RADIUS * new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                    
                }
                
                ItemWorldDisplay itemWorldDisplay = itemObject.AddComponent<ItemWorldDisplay>();
                itemWorldDisplay.Display(itemSlot);
                worldDisplays.Add(itemWorldDisplay);
            }
        }

        public void SetMovingTarget(Transform movingTarget)
        {
            this.movingTarget = movingTarget;
        }

        public void Update()
        {
            const float ACCERLATION = 25f;
            speed += ACCERLATION * Time.deltaTime;
            if (movingTarget) target = movingTarget.position;
            transform.position = Vector2.MoveTowards(transform.position,target,speed*Time.deltaTime);
            Vector2 dif = (Vector2)transform.position - target;
            if (dif.magnitude < 0.01f) Destroy(gameObject);
        }
    }
}
