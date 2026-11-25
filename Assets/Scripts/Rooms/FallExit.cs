using UnityEngine;
using UnityEngine.SceneManagement;

// the floor gives way when byte walks in, once he's fallen out of view the next level loads
[RequireComponent(typeof(Collider))]
public class FallExit : MonoBehaviour
{
    [SerializeField] private GameObject floor;
    [SerializeField] private string nextScene = "Level 2 draft";
    [SerializeField] private float dropSpeed = 8f;

    [Header("Optional pull")]
    // for drops started from the rim, like the intro's code hole: draws byte over the hole so he falls in
    // leave empty for floors byte is already standing on
    [SerializeField] private Transform pullTowards;
    [SerializeField] private float pullSpeed = 3f;

    private Transform player;
    private CharacterController playerController;
    private float floorY;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Drop(other.transform);
    }

    // also called by story sequences that drop byte themselves
    public void Drop(Transform target)
    {
        if (player != null) return;

        player = target;
        playerController = target.GetComponent<CharacterController>();
        floorY = floor.transform.position.y;
        foreach (Collider c in floor.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    private void Update()
    {
        if (player == null) return;

        floor.transform.position += Vector3.down * dropSpeed * Time.deltaTime;
        Pull();

        // only once he's below where the floor was and off the edge of the screen
        Vector3 view = Camera.main.WorldToViewportPoint(player.position);
        bool offScreen = view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f;
        if (player.position.y < floorY - 1f && offScreen)
        {
            enabled = false;
            SceneManager.LoadScene(nextScene);
        }
    }

    // sideways only, gravity from PlayerMove does the falling
    private void Pull()
    {
        if (pullTowards == null) return;

        Vector3 offset = pullTowards.position - player.position;
        offset.y = 0f;
        if (offset.magnitude < 0.05f) return;

        Vector3 step = Vector3.ClampMagnitude(offset, pullSpeed * Time.deltaTime);
        if (playerController != null && playerController.enabled) playerController.Move(step);
        else player.position += step;
    }
}
