using UnityEngine;
using UnityEngine.Events;

// each slot locks its block in once it holds the right letter, solved when every slot is locked
public class LetterLock : MonoBehaviour
{
    [SerializeField] private string word = "OPEN";
    // one area and one plate per letter, in the same order as the word
    [SerializeField] private BoxCollider[] slots;
    [SerializeField] private Renderer[] slotPlates;
    [SerializeField] private Material lockedMaterial;
    [SerializeField] private LayerMask blockLayers;
    [SerializeField] private UnityEvent onSolved;

    private Transform[] lockedBlocks;
    private Vector3[] lockedPositions;
    private int lockedCount;

    private void Awake()
    {
        lockedBlocks = new Transform[slots.Length];
        lockedPositions = new Vector3[slots.Length];
    }

    private void OnEnable() => GameEvents.PlayerRespawned += PutBlocksBack;
    private void OnDisable() => GameEvents.PlayerRespawned -= PutBlocksBack;

    // checked every physics step like the plates, swaps teleport blocks without trigger events
    private void FixedUpdate()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (lockedBlocks[i] != null) continue;

            LetterBlock block = BlockIn(slots[i]);
            if (block != null && block.Letter == word[i].ToString()) LockIn(i, block.transform);
        }
    }

    private LetterBlock BlockIn(BoxCollider slot)
    {
        foreach (Collider hit in Physics.OverlapBox(slot.bounds.center, slot.bounds.extents, Quaternion.identity, blockLayers, QueryTriggerInteraction.Ignore))
        {
            LetterBlock block = hit.GetComponentInParent<LetterBlock>();
            if (block != null) return block;
        }
        return null;
    }

    private void LockIn(int index, Transform block)
    {
        lockedBlocks[index] = block;
        lockedPositions[index] = block.position;
        slotPlates[index].sharedMaterial = lockedMaterial;

        // off the swappable layer so it can't be swapped back out
        foreach (Transform part in block.GetComponentsInChildren<Transform>())
            part.gameObject.layer = 0;

        lockedCount++;
        if (lockedCount == slots.Length) onSolved.Invoke();
    }

    // the room reset sends every block back to its start, locked ones go back in their slots
    private void PutBlocksBack()
    {
        for (int i = 0; i < lockedBlocks.Length; i++)
        {
            if (lockedBlocks[i] != null) lockedBlocks[i].position = lockedPositions[i];
        }
    }
}
