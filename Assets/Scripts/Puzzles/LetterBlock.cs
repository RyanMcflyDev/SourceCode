using UnityEngine;

// a swappable block that counts as one letter in a letter lock
public class LetterBlock : MonoBehaviour
{
    [SerializeField] private string letter;

    public string Letter => letter;
}
