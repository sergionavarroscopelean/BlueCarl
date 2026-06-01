using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class CombatManager : MonoBehaviour
    {
        [Header("Combat State")]
        [SerializeField] private bool inCombat;
        [SerializeField] private int turnCount;

        private List<EnemyInstance> activeEnemies = new List<EnemyInstance>();
        private int playerDamageBase = 10;

        public bool InCombat => inCombat;
        public IReadOnlyList<EnemyInstance> ActiveEnemies => activeEnemies;

        public event System.Action OnCombatStarted;
        public event System.Action<CombatResult> OnCombatEnded;
        public event System.Action<EnemyInstance, int> OnEnemyDamaged;
        public event System.Action<EnemyInstance> OnEnemyDefeated;
        public event System.Action<int> OnPlayerDamaged;

        public void StartCombat(EnemyEncounter encounter)
        {
            if (encounter == null || encounter.enemies == null) return;

            inCombat = true;
            turnCount = 0;
            activeEnemies.Clear();

            foreach (var enemyData in encounter.enemies)
            {
                activeEnemies.Add(new EnemyInstance(enemyData));
            }

            OnCombatStarted?.Invoke();
        }

        public void PlayerAttack(int targetIndex)
        {
            if (!inCombat) return;
            if (targetIndex < 0 || targetIndex >= activeEnemies.Count) return;

            var target = activeEnemies[targetIndex];
            int damage = CalculatePlayerDamage();
            int actualDamage = Mathf.Max(1, damage - target.Armor);

            target.TakeDamage(actualDamage);
            OnEnemyDamaged?.Invoke(target, actualDamage);

            if (target.CurrentHP <= 0)
            {
                OnEnemyDefeated?.Invoke(target);
                activeEnemies.RemoveAt(targetIndex);
            }

            if (activeEnemies.Count == 0)
            {
                EndCombat(true);
                return;
            }

            EnemyTurn();
        }

        private void EnemyTurn()
        {
            turnCount++;
            var resources = GameManager.Instance.Resources;

            foreach (var enemy in activeEnemies)
            {
                int damage = enemy.Damage;
                resources.TakeDamage(damage);
                OnPlayerDamaged?.Invoke(damage);
            }

            if (resources.CurrentHP <= 0)
            {
                EndCombat(false);
            }
        }

        private void EndCombat(bool playerWon)
        {
            inCombat = false;
            var result = new CombatResult
            {
                PlayerWon = playerWon,
                TurnsElapsed = turnCount,
                GoldEarned = 0,
                XPEarned = 0
            };

            if (playerWon)
            {
                var resources = GameManager.Instance.Resources;
                foreach (var enemy in activeEnemies)
                {
                    result.GoldEarned += enemy.Data.goldReward;
                    result.XPEarned += enemy.Data.xpReward;
                }
                // Note: enemies already removed, so we calculate from defeat events
            }

            int timeCost = Mathf.Max(1, turnCount);
            GameManager.Instance.Resources.SpendTime(timeCost);

            activeEnemies.Clear();
            OnCombatEnded?.Invoke(result);
        }

        private int CalculatePlayerDamage()
        {
            float multiplier = 1f;
            return Mathf.RoundToInt(playerDamageBase * multiplier);
        }

        public void SetPlayerBaseDamage(int damage)
        {
            playerDamageBase = damage;
        }
    }

    public class EnemyInstance
    {
        public EnemyData Data { get; private set; }
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public int Damage { get; private set; }
        public int Armor { get; private set; }

        public EnemyInstance(EnemyData data)
        {
            Data = data;
            MaxHP = data.maxHP;
            CurrentHP = MaxHP;
            Damage = data.damage;
            Armor = data.armor;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
        }
    }

    public struct CombatResult
    {
        public bool PlayerWon;
        public int TurnsElapsed;
        public int GoldEarned;
        public int XPEarned;
    }
}
