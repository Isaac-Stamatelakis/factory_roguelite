using System;
using TMPro;
using UnityEngine;
using World.Cave.Registry;

namespace TileEntity.Instances.Caves.DimensionalStabilizer
{
    public class DimensionalStabilizerUI : MonoBehaviour, ITileEntityUI
    {
        [SerializeField] private TextMeshProUGUI mMinerAllotmentText;
        [SerializeField] private TextMeshProUGUI mMinerActiveText;
        [SerializeField] private TextMeshProUGUI mMinerCountText;
        [SerializeField] private TextMeshProUGUI mMinimumAllotments;
        private CaveRegistry caveRegistry;
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            caveRegistry = CaveRegistry.Instance;
            Update();
        }

        public void Update()
        {
            if (!caveRegistry) return;
            mMinerAllotmentText.text = $"Current Allotments:{caveRegistry.CurrentAllotments}";
            mMinerActiveText.text = caveRegistry.MinersActive ? "Miners Active" : "Miners Not Active";
            mMinimumAllotments.text = $"Required Allotments:{caveRegistry.RequiredAllotments}";
            mMinerCountText.text = $"Active Miners:{caveRegistry.MinerCount}";
        }
    }
}
