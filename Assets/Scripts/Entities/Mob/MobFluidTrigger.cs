using UnityEngine;

namespace Entities.Mob
{
    public class MobFluidTrigger : MonoBehaviour
    {
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Fluid")
            {
                var vector3 = transform.localPosition;
                vector3.z += 2;
                transform.localPosition = vector3;
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Fluid")
            {
                var vector3 = transform.localPosition;
                vector3.z -= 2;
                transform.localPosition = vector3;
            }
        }
    }
}
