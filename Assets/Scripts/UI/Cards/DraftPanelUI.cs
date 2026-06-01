using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;
using DungeonArchitect.Systems;

namespace DungeonArchitect.UI
{
    public class DraftPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject roomCardPrefab;
        [SerializeField] private GameObject panelRoot;

        private List<RoomCardUI> activeCards = new List<RoomCardUI>();
        private RoomDraftManager draftManager;

        private void Start()
        {
            draftManager = GameManager.Instance.Draft;
            draftManager.OnDraftGenerated += ShowDraft;
            draftManager.OnDraftCleared += HidePanel;

            GameManager.Instance.OnStateChanged += OnGameStateChanged;
            HidePanel();
        }

        private void OnDestroy()
        {
            if (draftManager != null)
            {
                draftManager.OnDraftGenerated -= ShowDraft;
                draftManager.OnDraftCleared -= HidePanel;
            }
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void ShowDraft(List<RoomData> rooms)
        {
            ClearCards();
            panelRoot.SetActive(true);

            foreach (var room in rooms)
            {
                var cardGO = Instantiate(roomCardPrefab, cardContainer);
                var card = cardGO.GetComponent<RoomCardUI>();
                card.Setup(room);
                card.OnCardClicked += OnCardSelected;
                activeCards.Add(card);
            }
        }

        private void HidePanel()
        {
            ClearCards();
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void OnCardSelected(RoomCardUI card)
        {
            foreach (var c in activeCards)
                c.SetSelected(false);

            card.SetSelected(true);
            draftManager.SelectRoom(card.Data);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.RoomDraft)
                HidePanel();
        }

        private void ClearCards()
        {
            foreach (var card in activeCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            activeCards.Clear();
        }
    }
}
