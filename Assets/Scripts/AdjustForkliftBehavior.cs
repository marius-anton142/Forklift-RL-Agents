using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustForkliftBehavior : MonoBehaviour
{
    public Vector3 centerOfMass = new Vector3(0, -0.5f, 0);
    public float spring = 25000f;
    public float damper = 3000f;
    public float forwardExtremumSlip = 0.3f;
    public float forwardExtremumValue = 1.0f;
    public float forwardAsymptoteSlip = 0.6f;
    public float forwardAsymptoteValue = 0.7f;
    public float forwardStiffness = 0.8f;
    public float sidewaysExtremumSlip = 0.2f;
    public float sidewaysExtremumValue = 1.0f;
    public float sidewaysAsymptoteSlip = 0.5f;
    public float sidewaysAsymptoteValue = 0.7f;
    public float sidewaysStiffness = 1.0f;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass = centerOfMass;
        }

        WheelCollider[] wheelColliders = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheel in wheelColliders)
        {
            JointSpring suspensionSpring = wheel.suspensionSpring;
            suspensionSpring.spring = spring;
            suspensionSpring.damper = damper;
            wheel.suspensionSpring = suspensionSpring;

            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            forwardFriction.extremumSlip = forwardExtremumSlip;
            forwardFriction.extremumValue = forwardExtremumValue;
            forwardFriction.asymptoteSlip = forwardAsymptoteSlip;
            forwardFriction.asymptoteValue = forwardAsymptoteValue;
            forwardFriction.stiffness = forwardStiffness;
            wheel.forwardFriction = forwardFriction;

            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.extremumSlip = sidewaysExtremumSlip;
            sidewaysFriction.extremumValue = sidewaysExtremumValue;
            sidewaysFriction.asymptoteSlip = sidewaysAsymptoteSlip;
            sidewaysFriction.asymptoteValue = sidewaysAsymptoteValue;
            sidewaysFriction.stiffness = sidewaysStiffness;
            wheel.sidewaysFriction = sidewaysFriction;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMass, 0.1f);
    }
}
