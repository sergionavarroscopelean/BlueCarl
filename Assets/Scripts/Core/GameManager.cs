using UnityEngine;
using DungeonArchitect.Data;
using DungeonArchitect.Systems;

namespace DungeonArchitect.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private int roomsPlacedThisFloor;

        [Header("References")]
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private DungeonGridManager gridManager;
        [SerializeField] private RoomDraftManager draftManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private StairManager stairManager;
        [SerializeField] private ProgressionManager progressionManager;

        [Header("UI")]
        [SerializeField] private GameObject menuCanvas;

        [Header("Configuration")]
        [SerializeField] private ClassData selectedClass;
        [SerializeField] private int bossEveryXFloors = 3;
        [SerializeField] private int totalFloors = 8;

        [Header("Debug")]
        [SerializeField] private bool debugAutoStart = true;
        [SerializeField] private RoomData debugStartingRoom;

        private Vector2Int? pendingPlacementSlot;

        public GameState CurrentState => currentState;
        public int CurrentFloor => currentFloor;
        public int RoomsPlacedThisFloor => roomsPlacedThisFloor;
        public ResourceManager Resources => resourceManager;
        public DungeonGridManager Grid => gridManager;
        public RoomDraftManager Draft => draftManager;
        public DeckManager Deck => deckManager;
        public CombatManager Combat => combatManager;
        public StairManager Stairs => stairManager;
        public ProgressionManager Progression => progressionManager;

        public event System.Action<GameState> OnStateChanged;
        public event System.Action<int> OnFloorChanged;
        public event System.Action OnGameOver;
        public event System.Action OnVictory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!debugAutoStart) return;
            StartNewRun(selectedClass, deckManager.sourceDeck);
            if (debugStartingRoom != null)
                gridManager.PlaceStartingRoom(debugStartingRoom);
        }


        public void OnClickStartRun()
        {
            if (menuCanvas != null)
                menuCanvas.SetActive(false);

            StartNewRun(selectedClass, deckManager.sourceDeck);
            if (debugStartingRoom != null)
                gridManager.PlaceStartingRoom(debugStartingRoom);
        }
        
        public void StartNewRun(ClassData classData, DeckData deck)
        {
            selectedClass = classData;
            currentFloor = 1;
            roomsPlacedThisFloor = 0;

            resourceManager.Initialize(classData);
            deckManager.Initialize(deck);
            gridManager.InitializeFloor();
            stairManager.Initialize(currentFloor);

            ChangeState(GameState.Exploring);
            OnFloorChanged?.Invoke(currentFloor);
        }

        public void ChangeState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void OnRoomPlaced()
        {
            roomsPlacedThisFloor++;
            stairManager.OnRoomPlaced(roomsPlacedThisFloor);
        }

        public void OnStairFound()
        {
            DescendFloor();
        }

        private void DescendFloor()
        {
            currentFloor++;
            roomsPlacedThisFloor = 0;

            if (currentFloor > totalFloors)
            {
                ChangeState(GameState.Victory);
                OnVictory?.Invoke();
                return;
            }

            bool isBossFloor = currentFloor % bossEveryXFloors == 0;
            if (isBossFloor)
            {
                ChangeState(GameState.BossEncounter);
                return;
            }

            gridManager.InitializeFloor();
            stairManager.Initialize(currentFloor);
            deckManager.ReshuffleDeck();
            ChangeState(GameState.Exploring);
            OnFloorChanged?.Invoke(currentFloor);
        }

        public void OnPlayerDeath()
        {
            ChangeState(GameState.GameOver);
            OnGameOver?.Invoke();
            progressionManager.EndRun(currentFloor, roomsPlacedThisFloor);
        }

        public void RequestRoomDraft()
        {
            if (currentState != GameState.Exploring) return;
            ChangeState(GameState.RoomDraft);
            draftManager.GenerateDraft();
        }

        public void SetPendingPlacementSlot(Vector2Int gridPos)
        {
            pendingPlacementSlot = gridPos;
        }

        public void OnRoomSelected(RoomData room)
        {
            if (currentState != GameState.RoomDraft) return;

            if (pendingPlacementSlot.HasValue)
            {
                gridManager.PlaceRoom(room, pendingPlacementSlot.Value);
                gridManager.MovePlayerTo(pendingPlacementSlot.Value);
                pendingPlacementSlot = null;
                OnRoomPlaced();
                OnRoomResolved();
            }
            else
            {
                ChangeState(GameState.RoomPlacement);
            }
        }

        public void OnRoomResolved()
        {
            if (resourceManager.CurrentHP <= 0 || resourceManager.CurrentTime <= 0)
            {
                OnPlayerDeath();
                return;
            }
            ChangeState(GameState.Exploring);
        }
    }
}
