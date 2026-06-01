using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DungeonArchitect.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button progressionButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject classSelectPanel;
        [SerializeField] private GameObject deckSelectPanel;

        private void Start()
        {
            playButton.onClick.AddListener(OnPlayClicked);
            deckBuilderButton.onClick.AddListener(OnDeckBuilderClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            ShowMainPanel();
        }

        private void OnPlayClicked()
        {
            mainPanel.SetActive(false);
            classSelectPanel.SetActive(true);
        }

        private void OnDeckBuilderClicked()
        {
            SceneManager.LoadScene("DeckBuilding");
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }

        private void ShowMainPanel()
        {
            mainPanel.SetActive(true);
            classSelectPanel.SetActive(false);
            deckSelectPanel.SetActive(false);
        }
    }
}
