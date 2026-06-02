using UnityEngine;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class StairManager : MonoBehaviour
    {
        [Header("Stair Generation Settings")]
        [SerializeField] private int baseMinDepth = 10;
        [SerializeField] private int depthIncreasePerFloor = 2;
        [SerializeField] private float chancePerRoomAfterMin = 0.15f;
        [SerializeField] private int guaranteedSpawnRoom = 20;
        [SerializeField] private RoomData stairRoomData;

        [Header("Current State")]
        [SerializeField] private int currentMinDepth;
        [SerializeField] private bool stairSpawned;
        [SerializeField] private float currentSpawnChance;

        private float relicBonus;

        public bool StairSpawned => stairSpawned;
        public float CurrentSpawnChance => currentSpawnChance;
        public RoomData StairRoomData => stairRoomData;

        public event System.Action OnStairAvailable;

        public void Initialize(int floor)
        {
            currentMinDepth = baseMinDepth + (floor - 1) * depthIncreasePerFloor;
            stairSpawned = false;
            currentSpawnChance = 0f;
        }

        public void OnRoomPlaced(int roomsPlaced)
        {
            if (stairSpawned) return;

            if (roomsPlaced < currentMinDepth)
            {
                currentSpawnChance = 0f;
                return;
            }

            if (roomsPlaced >= guaranteedSpawnRoom)
            {
                currentSpawnChance = 1f;
                TriggerStairSpawn();
                return;
            }

            int roomsBeyondMin = roomsPlaced - currentMinDepth;
            currentSpawnChance = (roomsBeyondMin + 1) * chancePerRoomAfterMin + relicBonus;
            currentSpawnChance = Mathf.Min(currentSpawnChance, 1f);

            if (Random.value <= currentSpawnChance)
            {
                TriggerStairSpawn();
            }
        }

        public bool ShouldRoomBeStair()
        {
            return stairSpawned;
        }

        public void SetRelicBonus(float bonus)
        {
            relicBonus = bonus;
        }

        private void TriggerStairSpawn()
        {
            stairSpawned = true;
            OnStairAvailable?.Invoke();
        }
    }
}
