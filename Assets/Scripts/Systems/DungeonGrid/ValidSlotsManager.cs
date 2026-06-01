using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Core;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class ValidSlotsManager : MonoBehaviour
    {
        [SerializeField] private DungeonGridManager gridManager;
        [SerializeField] private GameObject emptySlotPrefab;

        private readonly List<GameObject> activeSlots = new List<GameObject>();

        private void Start()
        {
            gridManager.OnValidPlacementsChanged += RefreshSlots;
            GameManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (gridManager != null) gridManager.OnValidPlacementsChanged -= RefreshSlots;
            if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void RefreshSlots(IReadOnlyList<Vector2Int> positions)
        {
            ClearSlots();
            if (GameManager.Instance.CurrentState != GameState.Exploring) return;

            foreach (var pos in positions)
            {
                var worldPos = gridManager.GridToWorld(pos);
                var go = Instantiate(emptySlotPrefab, worldPos, Quaternion.identity, transform);
                var indicator = go.GetComponent<EmptySlotIndicator>();
                if (indicator != null)
                    indicator.Init(pos, this);
                activeSlots.Add(go);
            }
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Exploring)
                RefreshSlots(gridManager.ValidPlacements);
            else
                ClearSlots();
        }

        public void OnSlotClicked(Vector2Int gridPos)
        {
            GameManager.Instance.SetPendingPlacementSlot(gridPos);
            GameManager.Instance.RequestRoomDraft();
        }

        private void ClearSlots()
        {
            foreach (var go in activeSlots)
                if (go != null) Destroy(go);
            activeSlots.Clear();
        }
    }
}
