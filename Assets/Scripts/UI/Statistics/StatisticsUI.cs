using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming
namespace UI.Statistics
{
    public enum PlayerStatistic
    {
        Play_Time = 0,
        Distance_Traveled = 1,
        Caves_Explored = 2,
        Flight_Time = 3,
        Teleportations = 4,
        Largest_Compact_Machine_Depth = 5,
        Teleportations_Into_Compact_Machines = 6
    }

    public static class PlayerStatisticsUtils
    {
        public static void VerifyStaticsCollection(PlayerStatisticCollection playerStatisticCollection)
        {
            playerStatisticCollection.DiscreteValues ??= new Dictionary<PlayerStatistic, int>();
            playerStatisticCollection.ContinuousValues ??= new Dictionary<PlayerStatistic, float>();
            HashSet<PlayerStatistic> continuousValues = new HashSet<PlayerStatistic>
            {
                PlayerStatistic.Play_Time,
                PlayerStatistic.Distance_Traveled,
                PlayerStatistic.Flight_Time
            };
            PlayerStatistic[] playerStatistics = Enum.GetValues(typeof(PlayerStatistic)) as PlayerStatistic[];
            foreach (PlayerStatistic playerStatistic in playerStatistics)
            {
                bool continuous = continuousValues.Contains(playerStatistic);
                if (continuous)
                {
                    playerStatisticCollection.ContinuousValues.TryAdd(playerStatistic, 0);
                    playerStatisticCollection.DiscreteValues.Remove(playerStatistic);
                }
                else
                {
                    playerStatisticCollection.DiscreteValues.TryAdd(playerStatistic, 0);
                    playerStatisticCollection.ContinuousValues.Remove(playerStatistic);
                }
                
            }
        }
    }

    public class PlayerStatisticCollection
    {
        public Dictionary<PlayerStatistic, int> DiscreteValues;
        public Dictionary<PlayerStatistic, float> ContinuousValues;
        public PlayerStatisticCollection()
        {
            DiscreteValues = new Dictionary<PlayerStatistic, int>();
            ContinuousValues = new Dictionary<PlayerStatistic, float>();
        }
    }
    public class StatisticsUI : MonoBehaviour
    {
        [SerializeField] private Button mBackButton;
        [SerializeField] VerticalLayoutGroup mTextLayoutGroup;
        [SerializeField] private TextMeshProUGUI mTextPrefab;

        public void Start()
        {
            mBackButton.onClick.AddListener(CanvasController.Instance.PopStack);
            PlayerStatisticCollection playerStatisticCollection = PlayerManager.Instance.GetPlayer().PlayerStatisticCollection;
            PlayerStatistic[] playerStatistics = Enum.GetValues(typeof(PlayerStatistic)) as PlayerStatistic[];
            
            for (int i = 0; i < playerStatistics.Length; i++)
            {
                PlayerStatistic playerStatistic = playerStatistics[i];
                string statString = playerStatistic.ToString().Replace("_", " ");
                TextMeshProUGUI statText = Instantiate(mTextPrefab, mTextLayoutGroup.transform);
                if (playerStatisticCollection.DiscreteValues.TryGetValue(playerStatistic, out int discreteValue))
                {
                    statText.text = $"{statString}:{discreteValue}";
                    continue;
                }
                if (playerStatisticCollection.ContinuousValues.TryGetValue(playerStatistic, out float continuousValue))
                {
                    statText.text = $"{statString}:{continuousValue:F1}";
                    continue;
                }
            }
        }
    }
}
