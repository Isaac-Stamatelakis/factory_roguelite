using System;
using System.Collections;
using UnityEngine;

namespace TileEntity.Instances.Caves.Teleporter
{
    public class CaveTeleporterParticles : MonoBehaviour
    {
        [SerializeField] private float simulationAccerlation = 0.5f;
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private ParticleSystem outer;
        [SerializeField] private ParticleSystem mid;
        [SerializeField] private ParticleSystem inner;
        private ParticleSystem.MainModule outerMain;
        private ParticleSystem.MainModule midMain;
        private ParticleSystem.MainModule innerMain;
        public IEnumerator LoadParticles()
        {
            outer.Play();
            outerMain = outer.main;
            yield return new WaitForSeconds(delay);
            mid.Play();
            midMain = mid.main;
            yield return new WaitForSeconds(delay);
            inner.Play();
            innerMain = inner.main;
            yield return new WaitForSeconds(delay);
        }

        public void Update()
        {
            float dif = Time.deltaTime * simulationAccerlation;
            if (outer.isPlaying) outerMain.simulationSpeed += dif;
            if (mid.isPlaying) midMain.simulationSpeed += dif;
            if (inner.isPlaying) inner.Play();
        }
    }
}
