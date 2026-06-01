using UnityEngine;
using DungeonArchitect.Systems;

namespace DungeonArchitect.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GameObject gameManagerPrefab;

        private void Awake()
        {
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
        }
    }
}
