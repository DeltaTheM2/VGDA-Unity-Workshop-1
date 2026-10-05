// Checkpoint copy written by Tools/extract-lesson-scripts.sh from Docs/INSTRUCTOR_GUIDE.md. Edit the guide, not this file.
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorkshopCheckpoints
{
    [AddComponentMenu("")]
    public class GameManager : MonoBehaviour
    {
        // Singleton: any script can reach the one GameManager through GameManager.Instance.
        public static GameManager Instance;

        [Header("UI")]
        [Tooltip("The text that shows the score. Drag Canvas > ScoreText here.")]
        [SerializeField] private TMP_Text scoreText;

        [Header("Winning")]
        [Tooltip("The scene to load when every coin is collected. It must be listed in File > Build Profiles.")]
        [SerializeField] private string winSceneName = "Win";

        private int totalCoins = 0;
        private int collectedCoins = 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Coin[] coinsInLevel = FindObjectsByType<Coin>(FindObjectsSortMode.None);
            totalCoins = coinsInLevel.Length;
            UpdateScoreText();
        }

        public void AddCoin()
        {
            collectedCoins = collectedCoins + 1;
            UpdateScoreText();

            if (collectedCoins >= totalCoins)
            {
                SceneManager.LoadScene(winSceneName);
            }
        }

        private void UpdateScoreText()
        {
            scoreText.text = "Coins: " + collectedCoins + " / " + totalCoins;
        }
    }
}
