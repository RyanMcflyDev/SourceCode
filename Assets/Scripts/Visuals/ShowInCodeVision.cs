using UnityEngine;

// shows one set of objects in code vision and another set out of it
public class ShowInCodeVision : MonoBehaviour
{
    [SerializeField] private GameObject[] codeVisionOnly;
    [SerializeField] private GameObject[] normalOnly;

    private void OnEnable()
    {
        GameEvents.CodeVisionChanged += Apply;
        Apply(GameEvents.CodeVisionActive);
    }

    private void OnDisable() => GameEvents.CodeVisionChanged -= Apply;

    private void Apply(bool codeVisionOn)
    {
        foreach (GameObject item in codeVisionOnly) item.SetActive(codeVisionOn);
        foreach (GameObject item in normalOnly) item.SetActive(!codeVisionOn);
    }
}
