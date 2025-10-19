using UnityEngine;

public class AdjustCenterOfMass : MonoBehaviour
{
    public Vector3 centerOfMass;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMass, 0.1f);
    }
}
