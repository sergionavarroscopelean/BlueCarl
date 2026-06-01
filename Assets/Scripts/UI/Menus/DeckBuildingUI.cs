using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DungeonArchitect.Data;

namespace DungeonArchitect.UI
{
    public class DeckBuildingUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform availableRoomsContainer;
        [SerializeField] private Transform deckContainer;
        [SerializeField] private GameObject roomCardPrefab;
        [SerializeField] private Button startRunButton;
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI deckBreakdownText;

        [Header("Available Rooms")]
        [SerializeField] private List<RoomData> allAvailableRooms = new List<RoomData>();

        private List<RoomData> currentDeck = new List<RoomData>();
        private const int MaxDeckSize = 30;

        public event System.Action<List<RoomData>> OnDeckConfirmed;

        private void Start()
        {
            startRunButton.onClick.AddListener(ConfirmDeck);
            UpdateUI();
            PopulateAvailableRooms();
        }

        public void AddToDeck(RoomData room)
        {
            if (currentDeck.Count >= MaxDeckSize) return;
            currentDeck.Add(room);
            UpdateUI();
        }

        public void RemoveFromDeck(RoomData room)
        {
            currentDeck.Remove(room);
            UpdateUI();
        }

        public void LoadPresetDeck(DeckData preset)
        {
            currentDeck.Clear();
            foreach (var room in preset.rooms)
                currentDeck.Add(room);
            UpdateUI();
        }

        private void ConfirmDeck()
        {
            if (currentDeck.Count < MaxDeckSize) return;
            OnDeckConfirmed?.Invoke(currentDeck);
        }

        private void PopulateAvailableRooms()
        {
            foreach (Transform child in availableRoomsContainer)
                Destroy(child.gameObject);

            foreach (var room in allAvailableRooms)
            {
                var cardGO = Instantiate(roomCardPrefab, availableRoomsContainer);
                var card = cardGO.GetComponent<RoomCardUI>();
                card.Setup(room);
                card.OnCardClicked += (c) => AddToDeck(c.Data);
            }
        }

        private void UpdateUI()
        {
            if (deckCountText != null)
                deckCountText.text = $"{currentDeck.Count}/{MaxDeckSize}";

            if (deckBreakdownText != null)
                deckBreakdownText.text = GetDeckBreakdown();

            if (startRunButton != null)
                startRunButton.interactable = currentDeck.Count == MaxDeckSize;
        }

        private string GetDeckBreakdown()
        {
            var counts = new Dictionary<RoomType, int>();
            foreach (var room in currentDeck)
            {
                if (!counts.ContainsKey(room.roomType))
                    counts[room.roomType] = 0;
                counts[room.roomType]++;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var kvp in counts)
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            return sb.ToString();
        }
    }
}
