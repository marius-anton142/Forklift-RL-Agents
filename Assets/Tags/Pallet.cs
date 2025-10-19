using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pallet : MonoBehaviour
{
    private bool isTouchingWall = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.GetComponent<Wall>() != null)
        {
            isTouchingWall = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.GetComponent<Wall>() != null)
        {
            isTouchingWall = false;
        }
    }

    public bool IsTouchingWall()
    {
        return isTouchingWall;
    }

    public void setIsTouchingWall(bool value)
    {
        isTouchingWall = value;
    }
}
