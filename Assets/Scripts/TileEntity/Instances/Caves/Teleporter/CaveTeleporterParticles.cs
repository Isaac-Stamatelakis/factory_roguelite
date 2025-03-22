using System.Collections;
using UnityEngine;

namespace TileEntity.Instances.Caves.Teleporter
{
    public class CaveTeleporterParticles : MonoBehaviour
    {
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private ParticleSystem outer;
        [SerializeField] private ParticleSystem mid;
        [SerializeField] private ParticleSystem inner;
        public IEnumerator LoadParticles()
        {
            outer.Play();
            yield return new WaitForSeconds(delay);
            mid.Play();
            yield return new WaitForSeconds(delay);
            inner.Play();
            yield return new WaitForSeconds(delay);
        }
    }
}
