using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreen : MonoBehaviour
{
    public GameObject endScreenPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI rankText;

    public static EndScreen Instance;

    void Awake()
    {
        Instance = this;
        endScreenPanel.SetActive(false);
    }

    public void ShowEndScreen(int score, string rank)
    {
        endScreenPanel.SetActive(true);
        scoreText.text = "Poin: " + score;
        rankText.text = "Kategori: " + rank;
        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}