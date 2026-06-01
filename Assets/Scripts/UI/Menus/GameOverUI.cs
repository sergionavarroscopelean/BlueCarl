using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DungeonArchitect.Core;

namespace DungeonArchitect.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI floorReachedText;
        [SerializeField] private TextMeshProUGUI goldEarnedText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnVictory += ShowVictory;

            retryButton.onClick.AddListener(OnRetry);
            mainMenuButton.onClick.AddListener(OnMainMenu);

            panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver -= ShowGameOver;
                GameManager.Instance.OnVictory -= ShowVictory;
            }
        }

        private void ShowGameOver()
        {
            panelRoot.SetActive(true);
            titleText.text = "DEFEATED";
            floorReachedText.text = $"Floor Reached: {GameManager.Instance.CurrentFloor}";
            goldEarnedText.text = $"Gold Earned: {GameManager.Instance.Resources.Gold}";
        }

        private void ShowVictory()
        {
            panelRoot.SetActive(true);
            titleText.text = "VICTORY!";
            floorReachedText.text = $"All Floors Cleared!";
            goldEarnedText.text = $"Gold Earned: {GameManager.Instance.Resources.Gold}";
        }

        private void OnRetry()
        {
            SceneManager.LoadScene("Gameplay");
        }

        private void OnMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
