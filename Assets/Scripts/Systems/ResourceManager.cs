using UnityEngine;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class ResourceManager : MonoBehaviour
    {
        [Header("Current Values")]
        [SerializeField] private int currentHP;
        [SerializeField] private int maxHP;
        [SerializeField] private int currentTime;
        [SerializeField] private int maxTime;
        [SerializeField] private int gold;
        [SerializeField] private int keys;
        [SerializeField] private int gems;
        [SerializeField] private int xp;

        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public int CurrentTime => currentTime;
        public int MaxTime => maxTime;
        public int Gold => gold;
        public int Keys => keys;
        public int Gems => gems;
        public int XP => xp;

        public event System.Action<int, int> OnHPChanged;
        public event System.Action<int, int> OnTimeChanged;
        public event System.Action<int> OnGoldChanged;
        public event System.Action<int> OnKeysChanged;
        public event System.Action<int> OnGemsChanged;

        public void Initialize(ClassData classData)
        {
            maxHP = classData.startingHP;
            currentHP = maxHP;
            maxTime = classData.startingTime;
            currentTime = maxTime;
            gold = classData.startingGold;
            keys = classData.startingKeys;
            gems = classData.startingGems;
            xp = 0;

            NotifyAll();
        }

        public void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(0, currentHP - amount);
            OnHPChanged?.Invoke(currentHP, maxHP);
        }

        public void RestoreHP(int amount)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            OnHPChanged?.Invoke(currentHP, maxHP);
        }

        public void RestoreHPPercent(float percent)
        {
            int amount = Mathf.RoundToInt(maxHP * percent);
            RestoreHP(amount);
        }

        public void SpendTime(int amount)
        {
            currentTime = Mathf.Max(0, currentTime - amount);
            OnTimeChanged?.Invoke(currentTime, maxTime);
        }

        public void RestoreTime(int amount)
        {
            currentTime = Mathf.Min(maxTime, currentTime + amount);
            OnTimeChanged?.Invoke(currentTime, maxTime);
        }

        public void AddGold(int amount)
        {
            gold += amount;
            OnGoldChanged?.Invoke(gold);
        }

        public bool SpendGold(int amount)
        {
            if (gold < amount) return false;
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }

        public void AddKeys(int amount)
        {
            keys += amount;
            OnKeysChanged?.Invoke(keys);
        }

        public bool SpendKey()
        {
            if (keys <= 0) return false;
            keys--;
            OnKeysChanged?.Invoke(keys);
            return true;
        }

        public void AddGems(int amount)
        {
            gems += amount;
            OnGemsChanged?.Invoke(gems);
        }

        public bool SpendGems(int amount)
        {
            if (gems < amount) return false;
            gems -= amount;
            OnGemsChanged?.Invoke(gems);
            return true;
        }

        public void AddXP(int amount)
        {
            xp += amount;
        }

        public void IncreaseMaxHP(int amount)
        {
            maxHP += amount;
            currentHP += amount;
            OnHPChanged?.Invoke(currentHP, maxHP);
        }

        public void IncreaseMaxTime(int amount)
        {
            maxTime += amount;
            OnTimeChanged?.Invoke(currentTime, maxTime);
        }

        private void NotifyAll()
        {
            OnHPChanged?.Invoke(currentHP, maxHP);
            OnTimeChanged?.Invoke(currentTime, maxTime);
            OnGoldChanged?.Invoke(gold);
            OnKeysChanged?.Invoke(keys);
            OnGemsChanged?.Invoke(gems);
        }
    }
}
