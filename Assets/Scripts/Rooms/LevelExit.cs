using UnityEngine;
using UnityEngine.SceneManagement;

// loads the next level when byte walks in
[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour
{
    [SerializeField] private string nextScene;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) SceneManager.LoadScene(nextScene);
    }
}
