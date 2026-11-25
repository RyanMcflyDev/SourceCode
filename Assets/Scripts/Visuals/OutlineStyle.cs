using UnityEngine;

// optional, put on a swappable to change how SelectionOutline draws round it
// e.g. a thinner outline on enemies so it doesn't drown out their red
public class OutlineStyle : MonoBehaviour
{
    [SerializeField] private float width = 0.03f;
    [SerializeField] private bool overrideColour;
    [ColorUsage(false, true)]
    [SerializeField] private Color colour = Color.white;

    public float Width => width;
    public bool OverrideColour => overrideColour;
    public Color Colour => colour;
}
