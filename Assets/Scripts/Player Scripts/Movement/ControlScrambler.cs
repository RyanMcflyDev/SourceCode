using UnityEngine;
using UnityEngine.Events;

// Scrambles WASD after the player has taken enough damage.
// Doesn't know anything about health: whatever handles damage just calls RegisterDamage(amount).
public class ControlScrambler : MonoBehaviour, IMoveInputModifier
{
    // Directions in clockwise order. Order matters: rotations and mirrors are done with index maths.
    private const int Up = 0, Right = 1, Down = 2, Left = 3;
    private static readonly Vector2[] DirectionVectors = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
    private static readonly string[] DirectionNames = { "Up", "Right", "Down", "Left" };

    [Header("Trigger")]
    [Tooltip("Damage that has to build up before the controls scramble.")]
    [SerializeField, Min(0.01f)] private float _damageThreshold = 30f;

    [Header("Scramble")]
    [Tooltip("On: opposite keys stay opposite (W and S always end up on the same axis). " +
             "Gives rotations, flips, inversions and swaps. Off: any random shuffle of the four keys, " +
             "which can put W and S at right angles. Messier, see notes.")]
    [SerializeField] private bool _keepOppositesPaired = true;

    [Tooltip("Tick to keep controls scrambled until Restore() is called or the next scramble rolls. Untick to use the duration below.")]
    [SerializeField] private bool _permanent = false;

    [Tooltip("Seconds before controls go back to normal. Ignored when Permanent is ticked.")]
    [SerializeField, Min(0.1f)] private float _scrambleDuration = 8f;

    [Header("Events")]
    [Tooltip("Fires on scramble with a readable mapping, e.g. \"W > Left, A > Down, ...\". Hook UI, sound or screen shake here.")]
    public UnityEvent<string> OnScrambled;
    public UnityEvent OnRestored;

    // _map[keyDirection] = direction the player actually moves. Identity = normal controls.
    private readonly int[] _map = { Up, Right, Down, Left };
    private float _damageTaken;
    private float _restoreTimer;

    public bool IsScrambled { get; private set; }
    public float DamageTowardsScramble => _damageTaken;
    public float DamageThreshold => _damageThreshold;
    // Seconds left before controls restore (0 when not scrambled or permanent). Handy for a UI countdown.
    public float TimeRemaining => IsScrambled && !_permanent ? Mathf.Max(_restoreTimer, 0f) : 0f;

    private void Update()
    {
        if (!IsScrambled || _permanent) return;

        _restoreTimer -= Time.deltaTime;
        if (_restoreTimer <= 0f) Restore();
    }

    // Call this from the health / damage code whenever the player gets hit.
    public void RegisterDamage(float amount)
    {
        if (amount <= 0f) return;

        _damageTaken += amount;
        if (_damageTaken < _damageThreshold) return;

        _damageTaken = 0f;
        Scramble();
    }

    [ContextMenu("Scramble Now")]
    public void Scramble()
    {
        int[] previous = (int[])_map.Clone();

        // Re-roll until it's actually different from normal AND from the last scramble,
        // so a hit never "does nothing". At least 6 valid results exist, so this ends fast.
        do
        {
            if (_keepOppositesPaired) RollSquareSymmetry();
            else RollAnyShuffle();
        }
        while (MapEquals(IdentityMap) || MapEquals(previous));

        IsScrambled = true;
        _restoreTimer = _scrambleDuration;
        OnScrambled?.Invoke(DescribeMapping());
    }

    [ContextMenu("Restore Controls")]
    public void Restore()
    {
        for (int i = 0; i < _map.Length; i++) _map[i] = i;
        IsScrambled = false;
        OnRestored?.Invoke();
    }

    public Vector2 ModifyMoveInput(Vector2 input)
    {
        if (!IsScrambled) return input;

        // Split the stick/WASD vector into how hard each direction is held, then send each
        // one to its new direction. Works the same for keyboard and analogue sticks.
        Vector2 result =
            DirectionVectors[_map[Up]]    * Mathf.Max(input.y, 0f) +
            DirectionVectors[_map[Right]] * Mathf.Max(input.x, 0f) +
            DirectionVectors[_map[Down]]  * Mathf.Max(-input.y, 0f) +
            DirectionVectors[_map[Left]]  * Mathf.Max(-input.x, 0f);

        return Vector2.ClampMagnitude(result, 1f);
    }

    public string DescribeMapping()
    {
        return $"W > {DirectionNames[_map[Up]]}, A > {DirectionNames[_map[Left]]}, " +
               $"S > {DirectionNames[_map[Down]]}, D > {DirectionNames[_map[Right]]}";
    }

    // The 8 ways to turn/flip a square: rotate 0/90/180/270, optionally mirrored.
    // Covers "W becomes left", full inversion, left/right swap only, up/down swap only, and combos.
    private void RollSquareSymmetry()
    {
        int rotation = Random.Range(0, 4);
        bool mirror = Random.value < 0.5f;

        for (int dir = 0; dir < 4; dir++)
        {
            int flipped = mirror ? (4 - dir) % 4 : dir;
            _map[dir] = (flipped + rotation) % 4;
        }
    }

    // Any of the 24 orderings of the four directions (Fisher-Yates shuffle).
    private void RollAnyShuffle()
    {
        for (int i = 0; i < _map.Length; i++) _map[i] = i;
        for (int i = _map.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_map[i], _map[j]) = (_map[j], _map[i]);
        }
    }

    private static readonly int[] IdentityMap = { Up, Right, Down, Left };

    private bool MapEquals(int[] other)
    {
        for (int i = 0; i < _map.Length; i++)
            if (_map[i] != other[i]) return false;
        return true;
    }
}
