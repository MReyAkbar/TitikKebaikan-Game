using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenHowToPlay()
    {
        // nanti bisa buka panel How To Play
        Debug.Log("How To Play dibuka");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}