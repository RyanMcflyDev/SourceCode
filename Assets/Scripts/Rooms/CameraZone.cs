using UnityEngine;

// switches this area's camera on while byte is inside, give it a higher priority than the level's main camera
[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    [SerializeField] private GameObject areaCamera;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) areaCamera.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) areaCamera.SetActive(false);
    }
}
