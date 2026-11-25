using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// research test rooms only: researcher keys, run stats and the finish trigger
// F1 F2 F3 bullet time preset, F5 restart the run, F6 load the other room, F9 next participant
[RequireComponent(typeof(Collider))]
public class ResearchRun : MonoBehaviour
{
    [SerializeField] private string otherScene;
    [SerializeField] private string logFile = "research_log.csv";

    // static so it carries over between rooms
    private static int participant = 1;

    private CodeVision codeVision;
    private PlayerLock playerLock;

    private float startTime;
    private float bulletStart;
    private float bulletSeconds;
    private int activations;
    private int deaths;
    private int restarts;
    private bool finished;
    private string message;
    private float messageUntil;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void Awake()
    {
        codeVision = FindAnyObjectByType<CodeVision>();
        playerLock = FindAnyObjectByType<PlayerLock>();
        startTime = Time.unscaledTime;
        ShowMessage(StatusText() + "\nReach the green exit", 5f);
    }

    private void OnEnable()
    {
        GameEvents.PlayerDied += OnPlayerDied;
        GameEvents.CodeVisionChanged += OnCodeVisionChanged;
    }

    private void OnDisable()
    {
        GameEvents.PlayerDied -= OnPlayerDied;
        GameEvents.CodeVisionChanged -= OnCodeVisionChanged;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.f1Key.wasPressedThisFrame) SetPreset(0);
        if (keyboard.f2Key.wasPressedThisFrame) SetPreset(1);
        if (keyboard.f3Key.wasPressedThisFrame) SetPreset(2);
        if (keyboard.f5Key.wasPressedThisFrame) LoadScene(SceneManager.GetActiveScene().name);
        if (keyboard.f6Key.wasPressedThisFrame && otherScene != "") LoadScene(otherScene);
        if (keyboard.f9Key.wasPressedThisFrame)
        {
            participant++;
            ShowMessage(StatusText(), 3f);
        }
    }

    private void SetPreset(int index)
    {
        if (codeVision == null || index >= codeVision.PresetCount) return;
        CodeVision.Preset = index;
        // a preset change means a fresh run, so restart straight away
        LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void OnPlayerDied(string cause)
    {
        if (cause == "restart") restarts++;
        else deaths++;
    }

    private void OnCodeVisionChanged(bool active)
    {
        if (active)
        {
            activations++;
            bulletStart = Time.unscaledTime;
        }
        else bulletSeconds += Time.unscaledTime - bulletStart;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished || !other.CompareTag("Player")) return;
        finished = true;

        if (codeVision != null && codeVision.IsActive) bulletSeconds += Time.unscaledTime - bulletStart;
        float runTime = Time.unscaledTime - startTime;
        if (playerLock != null) playerLock.Lock();
        Time.timeScale = 1f;

        WriteLog(runTime);
        ShowMessage("Run complete\nTime " + runTime.ToString("0.0") + " s   Deaths " + deaths + "\nF5 run again   F6 other room", 9999f);
    }

    private void WriteLog(float runTime)
    {
        string path = Path.Combine(Application.persistentDataPath, logFile);
        if (!File.Exists(path))
            File.WriteAllText(path, "date,participant,room,preset,duration,run time,deaths,restarts,activations,bullet time seconds\n");

        string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "," + participant + "," + SceneManager.GetActiveScene().name + ","
            + (CodeVision.Preset + 1) + "," + codeVision.CurrentDuration + "," + runTime.ToString("0.00") + ","
            + deaths + "," + restarts + "," + activations + "," + bulletSeconds.ToString("0.00") + "\n";
        File.AppendAllText(path, line);
        Debug.Log("Research log written to " + path);
    }

    private string StatusText() => "Participant " + participant + "   Preset " + (CodeVision.Preset + 1);

    private void ShowMessage(string text, float seconds)
    {
        message = text;
        messageUntil = Time.unscaledTime + seconds;
    }

    // plain text bottom left, only shown briefly so the participant isn't watching it
    private void OnGUI()
    {
        if (Time.unscaledTime > messageUntil) return;
        GUI.Label(new Rect(12, Screen.height - 70, 600, 60), message);
    }
}
