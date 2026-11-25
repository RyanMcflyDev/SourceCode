using UnityEngine;

// opens when byte steps on it, an enemy swapped into the shaft fills it for good
// byte left in the shaft too long dies, there's no other way out
public class TrapDoor : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private GameObject lid;
    [SerializeField] private Material filledMaterial;

    [Header("Detection")]
    [SerializeField] private BoxCollider stepArea;
    [SerializeField] private BoxCollider shaftArea;
    [SerializeField] private LayerMask detectLayers;

    [Header("Sound")]
    // optional, played when byte's step opens the lid
    [SerializeField] private RandomSound openSound;

    [Header("Timing")]
    [SerializeField] private float timeToDie = 5f;

    private Renderer[] lidRenderers;
    private Material closedMaterial;
    private bool isOpen;
    private bool isFilled;
    private float timeInShaft;

    private void Awake()
    {
        lidRenderers = lid.GetComponentsInChildren<Renderer>();
        closedMaterial = lidRenderers[0].sharedMaterial;
    }

    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    private void OnPlayerDied(string cause)
    {
        isOpen = false;
        isFilled = false;
        timeInShaft = 0f;
        lid.SetActive(true);
        SetLidMaterial(closedMaterial);
    }

    private void FixedUpdate()
    {
        if (isFilled) return;

        if (!isOpen)
        {
            if (FindInArea(stepArea, "Player") != null)
            {
                isOpen = true;
                lid.SetActive(false);
                if (openSound != null) openSound.Play();
            }
            return;
        }

        Collider enemy = FindInArea(shaftArea, "Enemy");
        if (enemy != null)
        {
            Fill(enemy.gameObject);
            return;
        }

        Collider player = FindInArea(shaftArea, "Player");
        if (player == null)
        {
            timeInShaft = 0f;
            return;
        }

        timeInShaft += Time.fixedDeltaTime;
        if (timeInShaft >= timeToDie && player.TryGetComponent(out IKillable killable))
            killable.Kill("trap door");
    }

    private void Fill(GameObject enemy)
    {
        isFilled = true;
        lid.SetActive(true);
        SetLidMaterial(filledMaterial);

        // switched off not destroyed so the room reset brings it back
        enemy.SetActive(false);
    }

    private void SetLidMaterial(Material material)
    {
        foreach (Renderer plank in lidRenderers) plank.sharedMaterial = material;
    }

    private Collider FindInArea(BoxCollider area, string tag)
    {
        foreach (Collider hit in Physics.OverlapBox(area.bounds.center, area.bounds.extents, Quaternion.identity, detectLayers, QueryTriggerInteraction.Ignore))
            if (hit.CompareTag(tag)) return hit;

        return null;
    }
}
