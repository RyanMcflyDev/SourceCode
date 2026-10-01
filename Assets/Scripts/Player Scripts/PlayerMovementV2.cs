using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovementV2 : MonoBehaviour
{
    [Header("References")]
    public GameObject SelectionPrefab;
    public Transform PlayerModel;
    public Transform Camera;

    private Vector3 aimDirection = Vector3.forward;

    private Transform SelectedObject1 = null;
    private Transform SelectedObject2 = null;
    private KnifeAmount knifeAmount = KnifeAmount.Both;

    void Update()
    {
        if (Mouse.current == null) return;

        UpdateAim();
        SwapControls();
    }

    void UpdateAim()
    {
        Ray ray = Camera.GetComponent<UnityEngine.Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 targetPoint = hit.point;
            targetPoint.y = transform.position.y;

            Vector3 toTarget = targetPoint - transform.position;
            if (toTarget.sqrMagnitude > 0.01f)
            {
                aimDirection = toTarget.normalized;
                float targetYaw = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
                PlayerModel.localRotation = Quaternion.Euler(0f, targetYaw, 0f);
            }
        }
    }

    void SwapControls()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray knifeRay = Camera.GetComponent<UnityEngine.Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(knifeRay, out RaycastHit cameraHit, 100f)) return;
            if (cameraHit.collider.gameObject.layer != 3) return;

            // Second LOS ray from player to confirm no obstruction
            Vector3 dirToTarget = (cameraHit.point - transform.position).normalized;
            bool hasLOS = !Physics.Raycast(
                transform.position,
                dirToTarget,
                out RaycastHit losHit,
                Vector3.Distance(transform.position, cameraHit.point)
            ) || losHit.collider == cameraHit.collider;

            if (!hasLOS)
            {
                Debug.Log("[PlayerMovement] No line of sight to target, knife blocked.");
                return;
            }

            if (knifeAmount == KnifeAmount.Both)
            {
                SelectedObject1 = cameraHit.collider.gameObject.transform;
                SelectedObject2 = transform;
                knifeAmount = KnifeAmount.One;

                var prefab = Instantiate(SelectionPrefab);
                prefab.transform.parent = SelectedObject1;
                prefab.transform.localPosition = new Vector3(0, 1.5f, 0);
            }
            else if (knifeAmount == KnifeAmount.One && SelectedObject1 != cameraHit.collider.gameObject.transform)
            {
                SelectedObject2 = cameraHit.collider.gameObject.transform;
                knifeAmount = KnifeAmount.Neither;

                var prefab = Instantiate(SelectionPrefab);
                prefab.transform.parent = SelectedObject2;
                prefab.transform.localPosition = new Vector3(0, 1.5f, 0);
            }
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (knifeAmount == KnifeAmount.One)
            {
                Swap(SelectedObject1, SelectedObject1, transform, PlayerModel);
                SelectedObject1 = null;
                SelectedObject2 = null;
            }
            else if (knifeAmount == KnifeAmount.Neither)
            {
                Swap(SelectedObject1, SelectedObject1, SelectedObject2, SelectedObject2);
                SelectedObject1 = null;
                SelectedObject2 = null;
            }
        }
    }

    void Swap(Transform Obj1, Transform Obj1Model, Transform Obj2, Transform Obj2Model)
    {
        if (!Obj1 || !Obj2) return;

        Vector3 tempPos = Obj1.position;
        Quaternion tempRot = Obj1Model.rotation;

        MoveSwappable(Obj1, Obj2.position);
        Obj1Model.rotation = Obj2Model.rotation;

        MoveSwappable(Obj2, tempPos);
        Obj2Model.rotation = tempRot;

        knifeAmount = KnifeAmount.Both;

        try { Destroy(Obj1.GetComponentInChildren<ParticleSystem>().gameObject); } catch { }
        try { Destroy(Obj2.GetComponentInChildren<ParticleSystem>().gameObject); } catch { }

        Obj1.GetComponent<ITeleportable>()?.OnTeleported();
        Obj2.GetComponent<ITeleportable>()?.OnTeleported();
    }

    void MoveSwappable(Transform obj, Vector3 targetPos)
    {
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // Check if the destination actually has a NavMesh nearby
            bool hasNavMesh = NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 2f, NavMesh.AllAreas);

            if (hasNavMesh)
            {
                // Valid NavMesh at destination: Warp keeps the agent happy on cross-floor swaps
                agent.Warp(hit.position);
            }
            else
            {
                // No NavMesh (void, off map): disable agent and drop them with physics
                agent.enabled = false;
                obj.position = targetPos;

                Rigidbody agentRb = obj.GetComponent<Rigidbody>();
                if (agentRb != null)
                {
                    agentRb.isKinematic = false;
                    agentRb.linearVelocity = Vector3.zero;
                }
            }
        }
        else
        {
            obj.position = targetPos;
        }
    }
}

public enum KnifeAmount
{
    Both,
    One,
    Neither
}
