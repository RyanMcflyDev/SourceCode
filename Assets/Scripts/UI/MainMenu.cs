using UnityEngine;
using UnityEngine.SceneManagement;

// the main menu buttons call these
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string firstScene = "introduction";
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject controlsPanel;

    private void Start()
    {
        // coming back from the pause menu, make sure time is running
        Time.timeScale = 1f;
        ShowMain();
    }

    public void Play() => SceneManager.LoadScene(firstScene);

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
