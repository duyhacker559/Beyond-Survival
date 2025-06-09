using UnityEngine;
using UnityEngine.SceneManagement;

public class StartController : MonoBehaviour
{
    public OptionsController optionsController;

    public void PlayGame()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OpenOptions()
    {
        optionsController.OpenOptions();
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}