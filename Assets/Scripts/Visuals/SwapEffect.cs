using UnityEngine;

// on a swap byte breaks into strands of code where he stood, streams across as a band of data and the strands rebuild him where he lands
public class SwapEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] LineRenderer stream;
    [SerializeField] LineRenderer streakTemplate;
    [SerializeField] ParticleSystem fragments;

    [Header("Shape")]
    [SerializeField] float duration = 0.45f;
    [SerializeField] float bodyHeight = 2f;
    [SerializeField] float footOffset = 1f;
    [SerializeField] float width = 1f;
    [SerializeField] float arcHeight = 0.8f;
    [SerializeField] float packetLength = 0.5f;
    [SerializeField] float scrollSpeed = 6f;

    [Header("Streaks")]
    [SerializeField] int streaksPerEnd = 12;
    [SerializeField] float radius = 0.4f;
    [SerializeField] float streakWidth = 0.12f;
    [SerializeField] float maxDelay = 0.4f;

    [Header("Fragments")]
    [SerializeField] int fragmentCount = 40;

    const int Points = 20;
    Material streamMaterial, leaveMaterial, arriveMaterial;
    LineRenderer[] leaveStreaks, arriveStreaks;
    Vector3[] leaveOffsets, arriveOffsets;
    float[] leaveDelays, arriveDelays, leaveHeights, arriveHeights;
    Vector3 lastPosition, from, to;
    float startTime = -1f;

    void Awake()
    {
        if (player == null) player = transform.parent;
        stream.positionCount = Points;
        streamMaterial = stream.material;
        leaveMaterial = new Material(streakTemplate.sharedMaterial);
        arriveMaterial = new Material(streakTemplate.sharedMaterial);
        leaveStreaks = MakeStreaks("Leave Streak", leaveMaterial);
        arriveStreaks = MakeStreaks("Arrive Streak", arriveMaterial);
        leaveOffsets = new Vector3[streaksPerEnd];
        arriveOffsets = new Vector3[streaksPerEnd];
        leaveDelays = new float[streaksPerEnd];
        arriveDelays = new float[streaksPerEnd];
        leaveHeights = new float[streaksPerEnd];
        arriveHeights = new float[streaksPerEnd];
        streakTemplate.enabled = false;
        Hide();
    }

    LineRenderer[] MakeStreaks(string label, Material material)
    {
        var streaks = new LineRenderer[streaksPerEnd];
        for (int i = 0; i < streaksPerEnd; i++)
        {
            streaks[i] = Instantiate(streakTemplate, transform);
            streaks[i].name = label + " " + (i + 1);
            streaks[i].sharedMaterial = material;
            streaks[i].positionCount = 2;
            streaks[i].widthMultiplier = streakWidth;
        }
        return streaks;
    }

    void OnEnable()
    {
        lastPosition = player.position;
        GameEvents.PlayerSwapped += Play;
    }

    void OnDisable()
    {
        GameEvents.PlayerSwapped -= Play;
        Hide();
        startTime = -1f;
    }

    void Play(Transform target)
    {
        // lastPosition is still from before the swap moved him
        from = lastPosition - Vector3.up * footOffset;
        to = player.position - Vector3.up * footOffset;
        Scatter(leaveOffsets, leaveDelays, leaveHeights);
        Scatter(arriveOffsets, arriveDelays, arriveHeights);
        startTime = Time.time;
        stream.widthMultiplier = width * 0.8f;
        stream.enabled = true;
        SetEnabled(leaveStreaks, true);
        SetEnabled(arriveStreaks, true);
        Burst(from);
        Burst(to);
    }

    void Scatter(Vector3[] offsets, float[] delays, float[] heights)
    {
        for (int i = 0; i < streaksPerEnd; i++)
        {
            Vector2 spot = Random.insideUnitCircle * radius;
            offsets[i] = new Vector3(spot.x, 0f, spot.y);
            delays[i] = Random.Range(0f, maxDelay);
            heights[i] = bodyHeight * Random.Range(0.7f, 1.1f);
        }
    }

    void Burst(Vector3 feet)
    {
        fragments.transform.position = feet + Vector3.up * bodyHeight * 0.5f;
        fragments.Emit(fragmentCount);
    }

    void Update()
    {
        if (startTime < 0f) return;
        float t = (Time.time - startTime) / duration;
        if (t >= 1f)
        {
            Hide();
            startTime = -1f;
            return;
        }

        for (int i = 0; i < streaksPerEnd; i++)
        {
            // each strand drains away from the bottom up at its own moment
            float leave = Mathf.Clamp01((t - leaveDelays[i]) / (1f - leaveDelays[i]));
            SetStreak(leaveStreaks[i], from + leaveOffsets[i], leave * leaveHeights[i], leaveHeights[i]);
            // and each one builds up from the floor at its own moment
            float arrive = Mathf.Clamp01((t - arriveDelays[i]) / (1f - arriveDelays[i]));
            SetStreak(arriveStreaks[i], to + arriveOffsets[i], 0f, arrive * arriveHeights[i]);
        }

        float head = Mathf.Clamp01(t * (1f + packetLength));
        float tail = Mathf.Clamp01(head - packetLength);
        for (int i = 0; i < Points; i++)
            stream.SetPosition(i, ArcPoint(Mathf.Lerp(tail, head, i / (Points - 1f))));

        float scroll = Time.time * scrollSpeed;
        streamMaterial.mainTextureOffset = new Vector2(-scroll, 0f);
        leaveMaterial.mainTextureOffset = new Vector2(-scroll, 0f);
        arriveMaterial.mainTextureOffset = new Vector2(scroll, 0f);
    }

    void LateUpdate()
    {
        lastPosition = player.position;
    }

    void SetStreak(LineRenderer streak, Vector3 foot, float bottom, float top)
    {
        streak.SetPosition(0, foot + Vector3.up * bottom);
        streak.SetPosition(1, foot + Vector3.up * top);
    }

    Vector3 ArcPoint(float p)
    {
        Vector3 middle = Vector3.up * bodyHeight * 0.5f;
        return Vector3.Lerp(from + middle, to + middle, p) + Vector3.up * Mathf.Sin(p * Mathf.PI) * arcHeight;
    }

    void SetEnabled(LineRenderer[] streaks, bool on)
    {
        foreach (var streak in streaks) streak.enabled = on;
    }

    void Hide()
    {
        stream.enabled = false;
        SetEnabled(leaveStreaks, false);
        SetEnabled(arriveStreaks, false);
    }
}
