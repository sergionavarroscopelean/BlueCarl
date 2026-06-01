using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonArchitect.Core;
using DungeonArchitect.Systems;

namespace DungeonArchitect.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Resource Bars")]
        [SerializeField] private Slider hpBar;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Slider timeBar;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Resource Counters")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI keysText;
        [SerializeField] private TextMeshProUGUI gemsText;

        [Header("Floor Info")]
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI deckCountText;

        private ResourceManager resources;

        private void Start()
        {
            resources = GameManager.Instance.Resources;
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (resources == null) return;

            resources.OnHPChanged += UpdateHP;
            resources.OnTimeChanged += UpdateTime;
            resources.OnGoldChanged += UpdateGold;
            resources.OnKeysChanged += UpdateKeys;
            resources.OnGemsChanged += UpdateGems;

            GameManager.Instance.OnFloorChanged += UpdateFloor;
            GameManager.Instance.Deck.OnDrawPileChanged += UpdateDeckCount;
        }

        private void UnsubscribeFromEvents()
        {
            if (resources == null) return;

            resources.OnHPChanged -= UpdateHP;
            resources.OnTimeChanged -= UpdateTime;
            resources.OnGoldChanged -= UpdateGold;
            resources.OnKeysChanged -= UpdateKeys;
            resources.OnGemsChanged -= UpdateGems;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFloorChanged -= UpdateFloor;
                GameManager.Instance.Deck.OnDrawPileChanged -= UpdateDeckCount;
            }
        }

        private void UpdateHP(int current, int max)
        {
            if (hpBar != null) hpBar.value = (float)current / max;
            if (hpText != null) hpText.text = $"{current}/{max}";
        }

        private void UpdateTime(int current, int max)
        {
            if (timeBar != null) timeBar.value = (float)current / max;
            if (timeText != null) timeText.text = $"{current}/{max}";
        }

        private void UpdateGold(int amount)
        {
            if (goldText != null) goldText.text = amount.ToString();
        }

        private void UpdateKeys(int amount)
        {
            if (keysText != null) keysText.text = amount.ToString();
        }

        private void UpdateGems(int amount)
        {
            if (gemsText != null) gemsText.text = amount.ToString();
        }

        private void UpdateFloor(int floor)
        {
            if (floorText != null) floorText.text = $"Floor {floor}";
        }

        private void UpdateDeckCount(int count)
        {
            if (deckCountText != null) deckCountText.text = $"Deck: {count}";
        }
    }
}
