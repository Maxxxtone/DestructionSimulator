using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private LoadingScreen _loadingScreen;
    public void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    public void LoadSceneWithLoadingScreen(int index)
    {
        StartCoroutine(LoadSceneAsync(index));
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        _loadingScreen.gameObject.SetActive(true); 
        while(!operation.isDone)
        {
            var progress = Mathf.Clamp01(operation.progress / 0.9f);
            _loadingScreen.SetProgress(progress);
            yield return null;
        }
    }
}
