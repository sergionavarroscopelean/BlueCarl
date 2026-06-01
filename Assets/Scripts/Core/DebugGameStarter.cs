using UnityEngine;
using DungeonArchitect.Core;
using DungeonArchitect.Data;
using DungeonArchitect.Systems;

namespace DungeonArchitect.Core
{
    /// <summary>
    /// Temporary debug driver — bypasses menus so the dungeon loop is playable.
    /// Controls:
    ///   Space      — request a room draft (from Exploring state)
    ///   1 / 2 / 3  — select draft option N (enters Placement state)
    ///   Left-click — place selected room on a highlighted cell
    ///   Right-click — cancel placement and re-open draft
    /// Remove this component once real UI exists.
    /// </summary>
    public class DebugGameStarter : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private ClassData startClass;
        [SerializeField] private DeckData startDeck;
        [SerializeField] private RoomData startingRoom;

        [Header("References")]
        [SerializeField] private GridInputHandler gridInputHandler;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("DebugGameStarter: GameManager not found.");
                return;
            }
            if (startClass == null) { Debug.LogError("DebugGameStarter: startClass is not assigned."); return; }
            if (startDeck == null) { Debug.LogError("DebugGameStarter: startDeck is not assigned."); return; }
            if (startingRoom == null) { Debug.LogError("DebugGameStarter: startingRoom is not assigned."); return; }

            GameManager.Instance.StartNewRun(startClass, startDeck);
            GameManager.Instance.Grid.PlaceStartingRoom(startingRoom);

            Debug.Log($"[Debug] State={GameManager.Instance.CurrentState}  Deck={startDeck.name} ({startDeck.rooms?.Count ?? 0} rooms)");
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (gm.CurrentState == GameState.Exploring && Input.GetKeyDown(KeyCode.Space))
            {
                gm.RequestRoomDraft();
                var draft = gm.Draft.CurrentDraft;
                Debug.Log($"[Debug] Draft generated: {draft.Count} rooms  State={gm.CurrentState}");
                for (int i = 0; i < draft.Count; i++)
                    Debug.Log($"  [{i + 1}] {draft[i].roomName}  doors={string.Join(",", draft[i].doors)}");
            }

            if (gm.CurrentState == GameState.RoomDraft)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) SelectAndBeginPlacement(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) SelectAndBeginPlacement(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) SelectAndBeginPlacement(2);
            }
        }

        private void SelectAndBeginPlacement(int index)
        {
            var draft = GameManager.Instance.Draft;
            if (index >= draft.CurrentDraft.Count) return;

            draft.SelectRoom(index);

            if (gridInputHandler != null)
                gridInputHandler.BeginPlacement(draft.SelectedRoom);
        }
    }
}
