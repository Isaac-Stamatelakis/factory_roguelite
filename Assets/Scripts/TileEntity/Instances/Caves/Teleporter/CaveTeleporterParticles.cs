using System;
using System.Collections;
using System.Collections.Generic;
using Dimensions;
using Player;
using TileMaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using WorldModule.Caves;

namespace TileEntity.Instances.Caves.Teleporter
{
    public class CaveTeleporterParticles : MonoBehaviour
    {
        private class TilemapRendererFader
        {
            private struct TilemapColorValue
            {
                public Tilemap Tilemap;
                public Color InitialColor;
                public float FadeRate;

                public TilemapColorValue(Tilemap tilemap, Color initialColor, float fadeRate)
                {
                    Tilemap = tilemap;
                    InitialColor = initialColor;
                    FadeRate = fadeRate;
                }
            }
            private List<TilemapColorValue> tilemapColorValues;
            private float fadeTime;
            private float time;
            private Color fadeColor;
            public TilemapRendererFader(Tilemap[] tileMapRenderers, float fadeTime, Color fadeColor)
            {
                fadeColor.a = 0;
                this.fadeColor = fadeColor;
                this.fadeTime = fadeTime;
                tilemapColorValues = new List<TilemapColorValue>();
                foreach (Tilemap tilemap in tileMapRenderers)
                {
                    if (tilemap.gameObject.tag == "Outline")
                    {
                        tilemap.gameObject.SetActive(false);
                        continue;
                    }

                    IWorldTileMap worldTilemap = tilemap.gameObject.GetComponent<IWorldTileMap>();
                    bool shaderOverlayMap = worldTilemap == null;
                    float fadeRate = shaderOverlayMap ? 1 : 1.5f;
                    tilemapColorValues.Add(new TilemapColorValue(tilemap,tilemap.color,fadeRate));
                }
                
            }

            public void FadeUpdate(float deltaTime)
            {
                time += deltaTime;
                float progress = time / fadeTime;
                foreach (TilemapColorValue tilemapColorValue in tilemapColorValues)
                {
                    Tilemap tilemap = tilemapColorValue.Tilemap;
                    if (!tilemap) continue;
                    tilemapColorValue.Tilemap.color = Color.Lerp(tilemapColorValue.InitialColor,fadeColor,tilemapColorValue.FadeRate*progress);
                }
            }
            
        }
        [SerializeField] private float simulationAccerlation = 0.5f;
        [SerializeField] private float simulationAccerlationFalloff = 0.1f;
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private ParticleSystem outer;
        [SerializeField] private ParticleSystem mid;
        [SerializeField] private ParticleSystem inner;
        private List<ParticleSystem> particleSystems = new();
        private bool loaded;
        private TilemapRendererFader tilemapFader;
        private PlayerScript playerScript;
        
        /// <summary>
        /// This coroutine must be seperated from CaveSelectController since the player can destroy a Coroutine started in a UserInterface by pressing Escape 
        /// </summary>
        public void StartTeleportIntoCaveRoutine(Canvas parentCanvas, CaveObject caveObject, Action teleportAction, PlayerScript playerScript)
        {
            this.playerScript =  playerScript;
            var tileMaps = DimensionManager.Instance.GetComponentsInChildren<Tilemap>();
            tilemapFader = new TilemapRendererFader(tileMaps,5,Color.yellow);
            playerScript.TileViewers.DisableConduitViewers();
            StartCoroutine(TeleportIntoCave(parentCanvas, caveObject, teleportAction));
            playerScript.TileViewers.SetAllViewerState(false);
            playerScript.PlayerRobot.PausePlayer();
        }


        public void FixedUpdate()
        {
            tilemapFader.FadeUpdate(Time.fixedDeltaTime);
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
            playerScript.TileViewers.SetAllViewerState(true);
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
            PlayerAnimationController playerAnimationController = playerScript.PlayerRobot.AnimationController;
            playerAnimationController.PlayAnimation(PlayerAnimation.Air);
            playerAnimationController.ToggleBool(PlayerAnimationState.Walk,false);
            playerAnimationController.ToggleBool(PlayerAnimationState.Air,true);
            playerAnimationController.ToggleBool(PlayerAnimationState.Action,false);
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
