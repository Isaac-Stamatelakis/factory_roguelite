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
        private int defaultCameraCullingMask;
        private Color defaultCameraColor;

        /// <summary>
        /// This coroutine must be seperated from CaveSelectController since the player can destroy a Coroutine started in a UserInterface by pressing Escape 
        /// </summary>
        public void StartTeleportIntoCaveRoutine(Canvas parentCanvas, CaveObject caveObject, Action teleportAction)
        {
            defaultCameraCullingMask = Camera.main.cullingMask;
            int playerLayer = LayerMask.NameToLayer("Player");
            gameObject.layer = playerLayer;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.layer = playerLayer;
            }
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
            Camera.main.cullingMask = defaultCameraCullingMask;
            
        }

        private IEnumerator LoadParticles()
        {
            Camera mainCamera = Camera.main;
            float width = mainCamera.orthographicSize * 2;

            void ApplySize(ParticleSystem system, float cover)
            {
                var shape = system.shape;
                shape.radius = cover*width;
            }
            ApplySize(outer,1.1f);
            ApplySize(mid,0.5f);
            ApplySize(inner,0.125f);
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
            yield return new WaitForSeconds(2f);
            mainCamera.cullingMask = 1 << LayerMask.NameToLayer("Player");
        }

        private void StartFadeParticlesRoutine()
        {
            void TurnOffLooping(ParticleSystem particles)
            {
                var main = particles.main;
                main.loop = false;
            }

            var outMain = outer.main;
            outMain.simulationSpeed *= 3 / 4f; // Makes outer particles fade last
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
