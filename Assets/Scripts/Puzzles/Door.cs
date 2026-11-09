using UnityEngine;

// plates call Open and Close, the blocker stops byte and enemies straight away while the leaves swing
public class Door : MonoBehaviour
{
    [SerializeField] private GameObject blocker;
    [SerializeField] private bool startsOpen;

    [Header("Leaves")]
    // the right leaf is optional and swings the opposite way to the left one
    [SerializeField] private Transform leftLeaf;
    [SerializeField] private Transform rightLeaf;
    // flip the sign to swing the leaves the other way through the doorway
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float swingSpeed = 270f;

    private Quaternion leftClosed;
    private Quaternion rightClosed;
    private bool isOpen;

    private void Awake()
    {
        if (leftLeaf != null) leftClosed = leftLeaf.localRotation;
        if (rightLeaf != null) rightClosed = rightLeaf.localRotation;
        SetOpen(startsOpen);
    }

    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    private void SetOpen(bool open)
    {
        isOpen = open;
        blocker.SetActive(!open);
    }

    private void Update()
    {
        float angle = isOpen ? openAngle : 0f;
        Swing(leftLeaf, leftClosed, angle);
        Swing(rightLeaf, rightClosed, -angle);
    }

    private void Swing(Transform leaf, Quaternion closed, float angle)
    {
        if (leaf == null) return;
        Quaternion target = closed * Quaternion.Euler(0f, angle, 0f);
        leaf.localRotation = Quaternion.RotateTowards(leaf.localRotation, target, swingSpeed * Time.deltaTime);
    }
}
