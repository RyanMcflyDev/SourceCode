using UnityEngine;

[CreateAssetMenu(fileName = "Footsteps", menuName = "Source Code/Footstep Set")]
public class FootstepSet : ScriptableObject
{
    [field: SerializeField] public AudioClip[] WalkClips { get; private set; }
    [field: SerializeField] public AudioClip[] RunClips { get; private set; }
}
