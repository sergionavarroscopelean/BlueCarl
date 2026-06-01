using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class RelicManager : MonoBehaviour
    {
        [SerializeField] private List<RelicData> activeRelics = new List<RelicData>();
        private const int MaxRelics = 10;

        public IReadOnlyList<RelicData> ActiveRelics => activeRelics;

        public event System.Action<RelicData> OnRelicAcquired;
        public event System.Action<RelicData> OnRelicRemoved;

        public bool AddRelic(RelicData relic)
        {
            if (activeRelics.Count >= MaxRelics) return false;

            activeRelics.Add(relic);
            ApplyRelicEffects(relic);
            OnRelicAcquired?.Invoke(relic);
            return true;
        }

        public void RemoveRelic(RelicData relic)
        {
            if (!activeRelics.Contains(relic)) return;
            RemoveRelicEffects(relic);
            activeRelics.Remove(relic);
            OnRelicRemoved?.Invoke(relic);
        }

        public void ClearAllRelics()
        {
            activeRelics.Clear();
        }

        public float GetTotalGoldMultiplier()
        {
            float multiplier = 1f;
            foreach (var relic in activeRelics)
                multiplier *= relic.goldMultiplier;
            return multiplier;
        }

        public float GetTotalStairBonus()
        {
            float bonus = 0f;
            foreach (var relic in activeRelics)
                bonus += relic.stairChanceBonus;
            return bonus;
        }

        public float GetTotalCombatMultiplier()
        {
            float multiplier = 1f;
            foreach (var relic in activeRelics)
                multiplier *= relic.combatDamageMultiplier;
            return multiplier;
        }

        private void ApplyRelicEffects(RelicData relic)
        {
            var resources = GameManager.Instance.Resources;

            if (relic.maxHPBonus > 0)
                resources.IncreaseMaxHP(relic.maxHPBonus);

            if (relic.maxTimeBonus > 0)
                resources.IncreaseMaxTime(relic.maxTimeBonus);

            if (relic.stairChanceBonus > 0)
                GameManager.Instance.Stairs.SetRelicBonus(GetTotalStairBonus());
        }

        private void RemoveRelicEffects(RelicData relic)
        {
            if (relic.stairChanceBonus > 0)
                GameManager.Instance.Stairs.SetRelicBonus(GetTotalStairBonus() - relic.stairChanceBonus);
        }
    }
}
