using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkliftControl : MonoBehaviour
{
    public bool liftRaise = false;

    [Header("Setting")]
    [SerializeField] private float motorTorque = 100;
    [SerializeField] private float brakeForce = 30;
    [SerializeField] private float maxSteerAngle = 45;

    [Header("Lift")]
    [SerializeField] private Transform lift;
    [SerializeField] private float speedLift;
    [SerializeField] private float maxDownLift = 2.4f;
    [SerializeField] private float maxUpLift = 9.5f;
    [SerializeField] private float packageLift;

    [Header("Collider")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("Transform")]
    [SerializeField] private Transform frontWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("Input Smoothing")]
    [SerializeField] private float accelerationSpeed = 0.1f;

    [SerializeField] private float steeringSmoothness = 5f;
    private float targetSteerAngle = 0f;

    private float horizontalInput;
    private float verticalInput;
    private bool isBrake;

    private float brakeTorque;
    private float steerAngle;

    private bool isLiftDown;
    private bool isLiftUp;

    private bool isSteeringActive = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            MoveLiftToHeight(packageLift);
        }
    }

    private void FixedUpdate()
    {
        //GetInput();
        HandleTorque();
        HandleSteering();
        HandleLift();
        UpdateWheelPosition();
    }

    private void GetInput()
    {
        float rawInput = Input.GetAxisRaw("Horizontal") * -1f;
        if (Mathf.Abs(rawInput) > 0.01f)
        {
            targetSteerAngle = maxSteerAngle * rawInput;
            isSteeringActive = true;
        }
        else
        {
            isSteeringActive = false;
        }

        verticalInput = Input.GetAxis("Vertical");
        isBrake = Input.GetKey(KeyCode.Space);

        if (Input.GetKey(KeyCode.Q))
        {
            isLiftUp = true;
            isLiftDown = false;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            isLiftUp = false;
            isLiftDown = true;
        }
        else
        {
            isLiftUp = false;
            isLiftDown = false;
        }
    }

    public void SetInputs(float forwardAmount, float turnAmount, float liftAmount, float brakeAmount)
    {
        float rawInput = turnAmount * -1f;
        if (Mathf.Abs(rawInput) > 0.01f)
        {
            targetSteerAngle = maxSteerAngle * rawInput;
            isSteeringActive = true;
        }
        else
        {
            isSteeringActive = false;
        }

        verticalInput = forwardAmount;
        isBrake = false;

        if (brakeAmount == 1f) isBrake = true;

        if (liftAmount == 1f)
        {
            isLiftUp = true;
            isLiftDown = false;
        }
        else if (liftAmount == -1f)
        {
            isLiftUp = false;
            isLiftDown = true;
        }
        else
        {
            isLiftUp = false;
            isLiftDown = false;
        }
    }

    private void HandleTorque()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorTorque;
        frontRightWheelCollider.motorTorque = verticalInput * motorTorque;
        rearLeftWheelCollider.motorTorque = verticalInput * motorTorque;
        rearRightWheelCollider.motorTorque = verticalInput * motorTorque;

        brakeTorque = (isBrake) ? brakeForce : 0;

        frontLeftWheelCollider.brakeTorque = brakeTorque;
        frontRightWheelCollider.brakeTorque = brakeTorque;
        rearLeftWheelCollider.brakeTorque = brakeTorque;
        rearRightWheelCollider.brakeTorque = brakeTorque;
    }

    private void HandleSteering()
    {
        if (isSteeringActive)
        {
            steerAngle = Mathf.Lerp(steerAngle, targetSteerAngle, Time.deltaTime * steeringSmoothness);
        }
        rearLeftWheelCollider.steerAngle = steerAngle;
        rearRightWheelCollider.steerAngle = steerAngle;
    }

    public float GetSteeringAngle()
    {
        return rearLeftWheelCollider.steerAngle;
    }

    public float GetMaxDownLift()
    {
        return maxDownLift;
    }

    public float GetCurrentSpeed()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float currentSpeed = rb.velocity.magnitude;
        return currentSpeed;
    }

    public void SetSteeringAngle(float newSteerAngle)
    {
        isSteeringActive = false;
        steerAngle = newSteerAngle;
    }

    public void ResetLift()
    {
        lift.localPosition = new Vector3(lift.localPosition.x, maxDownLift, lift.localPosition.z);
    }

    private void HandleLift()
    {
        float y = lift.localPosition.y;
        if (isLiftUp)
        {
            y += speedLift * Time.deltaTime;
            y = Mathf.Clamp(y, maxDownLift, maxUpLift);

            lift.localPosition = new Vector3(lift.localPosition.x, y, lift.localPosition.z);
        }
        else if (isLiftDown)
        {
            y -= speedLift * Time.deltaTime;
            y = Mathf.Clamp(y, maxDownLift, maxUpLift);

            lift.localPosition = new Vector3(lift.localPosition.x, y, lift.localPosition.z);
        }
    }

    private void UpdateWheelPosition()
    {
        ChangeWheelRotation(frontLeftWheelCollider, frontWheelTransform);
        ChangeWheelPosition(rearLeftWheelCollider, rearLeftWheelTransform);
        ChangeWheelPosition(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void ChangeWheelPosition(WheelCollider wheelCollider, Transform WheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        WheelTransform.position = pos;
        WheelTransform.rotation = rot;
    }

    private void ChangeWheelRotation(WheelCollider wheelCollider, Transform WheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        WheelTransform.rotation = rot;
    }

    public void StopForkliftImmediately()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        float immediateBrakeForce = 10000f;
        frontLeftWheelCollider.brakeTorque = immediateBrakeForce;
        frontRightWheelCollider.brakeTorque = immediateBrakeForce;
        rearLeftWheelCollider.brakeTorque = immediateBrakeForce;
        rearRightWheelCollider.brakeTorque = immediateBrakeForce;

        frontLeftWheelCollider.motorTorque = 0;
        frontRightWheelCollider.motorTorque = 0;
        rearLeftWheelCollider.motorTorque = 0;
        rearRightWheelCollider.motorTorque = 0;

        frontLeftWheelCollider.steerAngle = 0;
        frontRightWheelCollider.steerAngle = 0;

        lift.localPosition = new Vector3(lift.localPosition.x, maxDownLift, lift.localPosition.z);
    }

    public void MoveLiftToHeight(float targetHeight)
    {
        StopCoroutine("AdjustLiftHeight");
        StartCoroutine(AdjustLiftHeight(targetHeight));
    }

    private IEnumerator AdjustLiftHeight(float targetHeight)
    {
        targetHeight = Mathf.Clamp(targetHeight, maxDownLift, maxUpLift);

        liftRaise = true;

        while (Mathf.Abs(lift.localPosition.y - targetHeight) > 0.05f)
        {
            float direction = Mathf.Sign(targetHeight - lift.localPosition.y);
            float newY = lift.localPosition.y + direction * speedLift * Time.deltaTime;
            newY = Mathf.Clamp(newY, maxDownLift, maxUpLift);
            lift.localPosition = new Vector3(lift.localPosition.x, newY, lift.localPosition.z);

            yield return null;
        }

        liftRaise = false;
    }
}