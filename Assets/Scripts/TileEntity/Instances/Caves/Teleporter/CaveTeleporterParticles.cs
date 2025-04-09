using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule.Caves;

namespace TileEntity.Instances.Caves.Teleporter
{
    public class CaveTeleporterParticles : MonoBehaviour
    {
        [SerializeField] private float simulationAccerlation = 0.5f;
        [SerializeField] private float simulationAccerlationFalloff = 0.1f;
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private ParticleSystem outer;
        [SerializeField] private ParticleSystem mid;
        [SerializeField] private ParticleSystem inner;
        private List<ParticleSystem> particleSystems = new();
        private bool loaded;

        /// <summary>
        /// This coroutine must be seperated from CaveSelectController since the player can destroy a Coroutine started in a UserInterface by pressing Escape 
        /// </summary>
        public void StartTeleportIntoCaveRoutine(Canvas parentCanvas, CaveObject caveObject, Action teleportAction)
        {
            StartCoroutine(TeleportIntoCave(parentCanvas, caveObject, teleportAction));
        }

        private IEnumerator TeleportIntoCave(Canvas parentCanvas, CaveObject caveObject, Action teleportAction)
        {
            bool restoreCanvas = parentCanvas.enabled;
            if (parentCanvas.enabled) parentCanvas.enabled = false;
            yield return StartCoroutine(LoadParticles());
            yield return StartCoroutine(CaveUtils.LoadCave(caveObject, CaveUtils.GenerateAndTeleportToCave));
            teleportAction.Invoke();
            if (restoreCanvas) parentCanvas.enabled = true;
            StartFadeParticlesRoutine();
            
        }

        private IEnumerator LoadParticles()
        {
            var wait = new WaitForSeconds(delay);
            outer.Play();
            particleSystems.Add(outer);
            yield return wait;
            particleSystems.Add(mid);
            mid.Play();
            yield return wait;
            particleSystems.Add(inner);
            inner.Play();
            yield return wait;
        }

        private void StartFadeParticlesRoutine()
        {
            void TurnOffLooping(ParticleSystem particles)
            {
                var main = particles.main;
                main.loop = false;
            }
            TurnOffLooping(outer);
            TurnOffLooping(mid);
            inner.gameObject.SetActive(false); // Having inner fade instantly looks nicer
            StartCoroutine(DestroySelf());
        }

        private IEnumerator DestroySelf()
        {
            yield return new WaitForSeconds(3f);
            
            GameObject.Destroy(gameObject);
        }

        public void Update()
        {
            float dif = Time.deltaTime * simulationAccerlation;
            for (int i = 0; i < particleSystems.Count; i++)
            {
                var main = particleSystems[i].main;
                main.simulationSpeed += dif*(1-simulationAccerlationFalloff*i);
            }
        }
    }
}
