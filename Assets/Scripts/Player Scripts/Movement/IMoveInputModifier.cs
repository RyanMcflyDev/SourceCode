using UnityEngine;

// Anything on the player that wants to bend the raw move input (scrambled controls,
// slippery floors, stuns, etc.) implements this. PlayerMove runs every one it finds,
// so new effects never need PlayerMove itself to change again.
public interface IMoveInputModifier
{
    Vector2 ModifyMoveInput(Vector2 input);
}
