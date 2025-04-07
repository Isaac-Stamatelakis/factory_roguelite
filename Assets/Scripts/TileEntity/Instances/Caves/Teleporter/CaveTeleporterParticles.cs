using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity.Instances.Caves.Teleporter
{
    public class CaveTeleporterParticles : MonoBehaviour
    {
        [SerializeField] private float simulationAccerlation = 0.5f;
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private float innerSpeedUpOnFinish = 1.35f;
        [SerializeField] private ParticleSystem outer;
        [SerializeField] private ParticleSystem mid;
        [SerializeField] private ParticleSystem inner;
        private List<ParticleSystem> particleSystems = new();
        private bool loaded;
        
        public IEnumerator LoadParticles()
        {
            outer.Play();
            particleSystems.Insert(0,outer);
            yield return new WaitForSeconds(delay);
            particleSystems.Insert(0,mid);
            mid.Play();
            yield return new WaitForSeconds(delay);
            particleSystems.Insert(0,inner);
            inner.Play();
            yield return new WaitForSeconds(delay);
        }

        public void StartFadeParticlesRoutine(Action callback)
        {
            void TurnOffLooping(ParticleSystem particles)
            {
                var main = particles.main;
                main.loop = false;
            }
            TurnOffLooping(outer);
            TurnOffLooping(mid);
            TurnOffLooping(inner);
            var innerMain = inner.main;
            innerMain.simulationSpeed *= innerSpeedUpOnFinish;
            StartCoroutine(CallFadeParticleCallback(callback));
            callback.Invoke();
        }

        private IEnumerator CallFadeParticleCallback(Action callback)
        {
            yield return new WaitForSeconds(5f);
            
            GameObject.Destroy(gameObject);
        }

        public void Update()
        {
            float dif = Time.deltaTime * simulationAccerlation;
            const float BASE = 0.5f;
            const float INCREASE = 0.25f;
            for (int i = 0; i < particleSystems.Count; i++)
            {
                var main = particleSystems[i].main;
                main.simulationSpeed += dif*(BASE+INCREASE*i);
            }
        }
    }
}
