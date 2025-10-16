using UnityEngine;
using UnityEngine.UI;

public class menu : MonoBehaviour
{
    [SerializeField] private Button StartButton;
    [SerializeField] private Button ExitButton;

    private void Start()
    {
        StartButton.onClick.AddListener(StartGame);
        ExitButton.onClick.AddListener(ExitGame);
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private void StartGame()
    {
        
        SceneLoader.LoadScene(SceneLoader.SceneIndex.mainIndex);
    }
}
