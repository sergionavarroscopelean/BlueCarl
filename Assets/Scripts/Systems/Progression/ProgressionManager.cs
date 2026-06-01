using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class ProgressionManager : MonoBehaviour
    {
        [Header("Meta Progression")]
        [SerializeField] private int totalGoldEarned;
        [SerializeField] private int totalRunsCompleted;
        [SerializeField] private int highestFloorReached;
        [SerializeField] private List<string> unlockedRoomIds = new List<string>();
        [SerializeField] private List<string> unlockedRelicIds = new List<string>();
        [SerializeField] private List<string> unlockedClassIds = new List<string>();

        [Header("Upgrades")]
        [SerializeField] private int startingTimeBonus;
        [SerializeField] private int startingHPBonus;
        [SerializeField] private int startingGoldBonus;

        private const string SaveKey = "MetaProgression";

        public int TotalGoldEarned => totalGoldEarned;
        public int TotalRuns => totalRunsCompleted;
        public int HighestFloor => highestFloorReached;
        public int StartingTimeBonus => startingTimeBonus;
        public int StartingHPBonus => startingHPBonus;

        public event System.Action<string> OnRoomUnlocked;
        public event System.Action<string> OnRelicUnlocked;
        public event System.Action<string> OnClassUnlocked;

        public void Initialize()
        {
            LoadProgress();
        }

        public void EndRun(int floorReached, int goldCollected)
        {
            totalRunsCompleted++;
            totalGoldEarned += goldCollected;
            if (floorReached > highestFloorReached)
                highestFloorReached = floorReached;

            SaveProgress();
        }

        public bool IsRoomUnlocked(string roomId)
        {
            return unlockedRoomIds.Contains(roomId);
        }

        public void UnlockRoom(string roomId)
        {
            if (unlockedRoomIds.Contains(roomId)) return;
            unlockedRoomIds.Add(roomId);
            OnRoomUnlocked?.Invoke(roomId);
            SaveProgress();
        }

        public bool IsRelicUnlocked(string relicId)
        {
            return unlockedRelicIds.Contains(relicId);
        }

        public void UnlockRelic(string relicId)
        {
            if (unlockedRelicIds.Contains(relicId)) return;
            unlockedRelicIds.Add(relicId);
            OnRelicUnlocked?.Invoke(relicId);
            SaveProgress();
        }

        public bool IsClassUnlocked(string classId)
        {
            return unlockedClassIds.Contains(classId);
        }

        public void UnlockClass(string classId)
        {
            if (unlockedClassIds.Contains(classId)) return;
            unlockedClassIds.Add(classId);
            OnClassUnlocked?.Invoke(classId);
            SaveProgress();
        }

        public void UpgradeStartingTime(int amount)
        {
            startingTimeBonus += amount;
            SaveProgress();
        }

        public void UpgradeStartingHP(int amount)
        {
            startingHPBonus += amount;
            SaveProgress();
        }

        public void UpgradeStartingGold(int amount)
        {
            startingGoldBonus += amount;
            SaveProgress();
        }

        private void SaveProgress()
        {
            var data = new ProgressionSaveData
            {
                totalGoldEarned = totalGoldEarned,
                totalRunsCompleted = totalRunsCompleted,
                highestFloorReached = highestFloorReached,
                unlockedRoomIds = unlockedRoomIds,
                unlockedRelicIds = unlockedRelicIds,
                unlockedClassIds = unlockedClassIds,
                startingTimeBonus = startingTimeBonus,
                startingHPBonus = startingHPBonus,
                startingGoldBonus = startingGoldBonus
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;

            string json = PlayerPrefs.GetString(SaveKey);
            var data = JsonUtility.FromJson<ProgressionSaveData>(json);

            totalGoldEarned = data.totalGoldEarned;
            totalRunsCompleted = data.totalRunsCompleted;
            highestFloorReached = data.highestFloorReached;
            unlockedRoomIds = data.unlockedRoomIds ?? new List<string>();
            unlockedRelicIds = data.unlockedRelicIds ?? new List<string>();
            unlockedClassIds = data.unlockedClassIds ?? new List<string>();
            startingTimeBonus = data.startingTimeBonus;
            startingHPBonus = data.startingHPBonus;
            startingGoldBonus = data.startingGoldBonus;
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            totalGoldEarned = 0;
            totalRunsCompleted = 0;
            highestFloorReached = 0;
            unlockedRoomIds.Clear();
            unlockedRelicIds.Clear();
            unlockedClassIds.Clear();
            startingTimeBonus = 0;
            startingHPBonus = 0;
            startingGoldBonus = 0;
        }
    }

    [System.Serializable]
    public class ProgressionSaveData
    {
        public int totalGoldEarned;
        public int totalRunsCompleted;
        public int highestFloorReached;
        public List<string> unlockedRoomIds;
        public List<string> unlockedRelicIds;
        public List<string> unlockedClassIds;
        public int startingTimeBonus;
        public int startingHPBonus;
        public int startingGoldBonus;
    }
}
