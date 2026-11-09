using UnityEngine;

// byte can't pick a new swap target while standing in one of these, a target picked before entering still works
[RequireComponent(typeof(Collider))]
public class NoSelectZone : MonoBehaviour
{
    private void Reset() => GetComponent<Collider>().isTrigger = true;
}
