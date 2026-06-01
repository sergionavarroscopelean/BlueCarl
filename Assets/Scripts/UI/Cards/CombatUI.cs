using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DungeonArchitect.Core;
using DungeonArchitect.Systems;

namespace DungeonArchitect.UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject combatPanel;

        [Header("Enemy Display")]
        [SerializeField] private Transform enemyContainer;
        [SerializeField] private GameObject enemyUIPrefab;

        [Header("Player Actions")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button useItemButton;
        [SerializeField] private Button fleeButton;

        [Header("Combat Log")]
        [SerializeField] private TextMeshProUGUI combatLogText;

        private CombatManager combatManager;
        private List<GameObject> enemyUIElements = new List<GameObject>();
        private int selectedTarget;

        private void Start()
        {
            combatManager = GameManager.Instance.Combat;
            combatManager.OnCombatStarted += ShowCombat;
            combatManager.OnCombatEnded += OnCombatEnded;
            combatManager.OnEnemyDamaged += OnEnemyDamaged;
            combatManager.OnPlayerDamaged += OnPlayerDamaged;

            attackButton.onClick.AddListener(OnAttackClicked);
            if (fleeButton != null) fleeButton.onClick.AddListener(OnFleeClicked);

            combatPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (combatManager != null)
            {
                combatManager.OnCombatStarted -= ShowCombat;
                combatManager.OnCombatEnded -= OnCombatEnded;
                combatManager.OnEnemyDamaged -= OnEnemyDamaged;
                combatManager.OnPlayerDamaged -= OnPlayerDamaged;
            }
        }

        private void ShowCombat()
        {
            combatPanel.SetActive(true);
            selectedTarget = 0;
            RefreshEnemyDisplay();
            if (combatLogText != null) combatLogText.text = "Combat begins!";
        }

        private void OnCombatEnded(CombatResult result)
        {
            if (result.PlayerWon)
            {
                if (combatLogText != null)
                    combatLogText.text = $"Victory! +{result.GoldEarned}G +{result.XPEarned}XP";

                var resources = GameManager.Instance.Resources;
                resources.AddGold(result.GoldEarned);
                resources.AddXP(result.XPEarned);
            }
            else
            {
                if (combatLogText != null)
                    combatLogText.text = "Defeated...";
            }

            Invoke(nameof(HideCombat), 1.5f);
        }

        private void HideCombat()
        {
            combatPanel.SetActive(false);
            ClearEnemyDisplay();
            GameManager.Instance.OnRoomResolved();
        }

        private void OnAttackClicked()
        {
            combatManager.PlayerAttack(selectedTarget);
            RefreshEnemyDisplay();
        }

        private void OnFleeClicked()
        {
            GameManager.Instance.Resources.SpendTime(3);
            combatPanel.SetActive(false);
            ClearEnemyDisplay();
            GameManager.Instance.OnRoomResolved();
        }

        private void OnEnemyDamaged(EnemyInstance enemy, int damage)
        {
            if (combatLogText != null)
                combatLogText.text = $"Hit {enemy.Data.enemyName} for {damage} damage!";
            RefreshEnemyDisplay();
        }

        private void OnPlayerDamaged(int damage)
        {
            if (combatLogText != null)
                combatLogText.text += $"\nTook {damage} damage!";
        }

        private void RefreshEnemyDisplay()
        {
            ClearEnemyDisplay();

            if (!combatManager.InCombat) return;

            for (int i = 0; i < combatManager.ActiveEnemies.Count; i++)
            {
                var enemy = combatManager.ActiveEnemies[i];
                if (enemyUIPrefab != null)
                {
                    var go = Instantiate(enemyUIPrefab, enemyContainer);
                    var nameText = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (nameText != null)
                        nameText.text = $"{enemy.Data.enemyName}\n{enemy.CurrentHP}/{enemy.MaxHP}";
                    enemyUIElements.Add(go);
                }
            }
        }

        private void ClearEnemyDisplay()
        {
            foreach (var go in enemyUIElements)
                if (go != null) Destroy(go);
            enemyUIElements.Clear();
        }
    }
}
