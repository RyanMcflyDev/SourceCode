using UnityEngine;
using UnityEngine.SceneManagement;

// the floor gives way when byte walks in, once he's fallen out of view the next level loads
[RequireComponent(typeof(Collider))]
public class FallExit : MonoBehaviour
{
    [SerializeField] private GameObject floor;
    [SerializeField] private string nextScene = "Level 2 draft";
    [SerializeField] private float dropSpeed = 8f;

    private Transform player;
    private float floorY;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (player != null || !other.CompareTag("Player")) return;

        player = other.transform;
        floorY = floor.transform.position.y;
        foreach (Collider c in floor.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    private void Update()
    {
        if (player == null) return;

        floor.transform.position += Vector3.down * dropSpeed * Time.deltaTime;

        // only once he's below where the floor was and off the edge of the screen
        Vector3 view = Camera.main.WorldToViewportPoint(player.position);
        bool offScreen = view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f;
        if (player.position.y < floorY - 1f && offScreen)
        {
            enabled = false;
            SceneManager.LoadScene(nextScene);
        }
    }
}
