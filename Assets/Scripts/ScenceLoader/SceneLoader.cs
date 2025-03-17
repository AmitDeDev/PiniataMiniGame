using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;
    
    public void OnSwitchSceneButtonClicked()
    {
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (activeSceneIndex == 0)
        {
            StartCoroutine(LoadScene(1));
            return;
        }

        StartCoroutine(LoadScene(0));
    }

    IEnumerator LoadScene(int _sceneIndex)
    {
        transition.SetTrigger("StartCrossfade");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(_sceneIndex);
    }
}
