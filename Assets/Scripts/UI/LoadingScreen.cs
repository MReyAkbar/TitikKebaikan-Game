using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public Slider loadingBar;

    void Start()
    {
        StartCoroutine(LoadMainMenu());
    }

    IEnumerator LoadMainMenu()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("MainMenu");
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            loadingBar.value = op.progress;
            yield return null;
        }

        loadingBar.value = 1f;
        yield return new WaitForSeconds(1f);
        op.allowSceneActivation = true;
    }
}