using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public enum SceneIndex
    {
        mainIndex=1,
    }
    public static void LoadScene(string sceneName,LoadSceneMode mode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneName, mode);
    }
    public static void LoadScene(int sceneIndex,LoadSceneMode mode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneIndex, mode);
    }
    public static void LoadScene(SceneIndex sceneIndex,LoadSceneMode mode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene((int)sceneIndex, mode);
    }
    
}
