using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class DeckManager : MonoBehaviour
    {
        [Header("Current Deck State")]
        [SerializeField] private List<RoomData> drawPile = new List<RoomData>();
        [SerializeField] private List<RoomData> discardPile = new List<RoomData>();

        private DeckData sourceDeck;

        public int DrawPileCount => drawPile.Count;
        public int DiscardPileCount => discardPile.Count;
        public int TotalCardsRemaining => drawPile.Count + discardPile.Count;

        public event System.Action OnDeckShuffled;
        public event System.Action<int> OnDrawPileChanged;

        public void Initialize(DeckData deck)
        {
            sourceDeck = deck;
            drawPile.Clear();
            discardPile.Clear();

            foreach (var room in deck.rooms)
            {
                drawPile.Add(room);
            }

            Shuffle(drawPile);
            OnDrawPileChanged?.Invoke(drawPile.Count);
        }

        public List<RoomData> DrawRooms(int count)
        {
            var drawn = new List<RoomData>();

            for (int i = 0; i < count; i++)
            {
                if (drawPile.Count == 0)
                {
                    if (discardPile.Count == 0) break;
                    ReshuffleDeck();
                }

                if (drawPile.Count > 0)
                {
                    var room = drawPile[0];
                    drawPile.RemoveAt(0);
                    drawn.Add(room);
                }
            }

            OnDrawPileChanged?.Invoke(drawPile.Count);
            return drawn;
        }

        public void DiscardRoom(RoomData room)
        {
            discardPile.Add(room);
        }

        public void DiscardRooms(List<RoomData> rooms)
        {
            foreach (var room in rooms)
                discardPile.Add(room);
        }

        public void ReshuffleDeck()
        {
            foreach (var room in discardPile)
                drawPile.Add(room);
            discardPile.Clear();

            Shuffle(drawPile);
            OnDeckShuffled?.Invoke();
            OnDrawPileChanged?.Invoke(drawPile.Count);
        }

        public void AddRoomToDeck(RoomData room)
        {
            drawPile.Add(room);
            Shuffle(drawPile);
            OnDrawPileChanged?.Invoke(drawPile.Count);
        }

        public void RemoveRoomFromDeck(RoomData room)
        {
            drawPile.Remove(room);
            discardPile.Remove(room);
            OnDrawPileChanged?.Invoke(drawPile.Count);
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
