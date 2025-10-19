using UnityEngine;

public class UISmoothDamp : MonoBehaviour
{
    public Vector3 leftPosition;
    public Vector3 rightPosition;
    public float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 previousTargetPosition;

    void Start()
    {
        previousTargetPosition = gameObject.transform.localPosition;
    }

    void Update()
    {
        Vector3 targetPosition;

        if (Input.mousePosition.x > Screen.width * 0.6f)
        {
            targetPosition = leftPosition;
        }
        else
        {
            targetPosition = rightPosition;
        }

        if (targetPosition != previousTargetPosition)
        {
            SoundManager.instance.PlayMapSound();
            previousTargetPosition = targetPosition;
        }

        gameObject.transform.localPosition = Vector3.SmoothDamp(gameObject.transform.localPosition, targetPosition, ref velocity, smoothTime);
    }
}
