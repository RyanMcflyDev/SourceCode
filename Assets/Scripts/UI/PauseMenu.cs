using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// esc pauses the game, the buttons on the pause panel call the public methods
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private InputAction pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private string menuScene = "Menu";

    private PlayerLock playerLock;
    private bool isPaused;
    // only unlock byte if the pause locked him, so it can't end a dialogue lock early
    private bool lockedByPause;
    // so pausing in bullet time resumes slowed, not at full speed
    private float timeScaleBeforePause = 1f;

    private void Awake()
    {
        playerLock = FindAnyObjectByType<PlayerLock>();
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPause;
    }

    // domain reload is off so the action has to be turned off here
    private void OnDisable()
    {
        pauseAction.performed -= OnPause;
        pauseAction.Disable();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        // esc on the controls page goes back to the pause page first
        if (isPaused && controlsPanel.activeSelf)
        {
            HideControls();
            return;
        }
        SetPaused(!isPaused);
    }

    public void Resume() => SetPaused(false);

    public void ShowControls()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // dying resets the room, so it's the quickest way out of a stuck spot
    public void RestartRoom()
    {
        SetPaused(false);
        PlayerDeath death = FindAnyObjectByType<PlayerDeath>();
        if (death != null) death.Kill("restart");
    }

    public void MainMenu()
    {
        SetPaused(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }

    private void SetPaused(bool paused)
    {
        if (paused && !isPaused) timeScaleBeforePause = Time.timeScale;
        isPaused = paused;
        Time.timeScale = paused ? 0f : timeScaleBeforePause;
        pausePanel.SetActive(paused);
        controlsPanel.SetActive(false);

        // stops clicks on the pause buttons selecting or swapping
        if (playerLock == null) return;
        if (paused && !playerLock.IsLocked)
        {
            playerLock.Lock();
            lockedByPause = true;
        }
        else if (!paused && lockedByPause)
        {
            playerLock.Unlock();
            lockedByPause = false;
        }
    }
}
