using UnityEngine;
using System.Collections;

public class FirstPersonCam : MonoBehaviour
{
    [Header("Horizontal and Vertical Look Speeds")]
    public float speedH = 2.0f;
    public float speedV = 2.0f;
    private float yaw;
    private float pitch;

	void Update()
    {
        // Editor-only speed
        if (Application.isPlaying)
		{
            speedH = 2;
            speedV = 2;
		}

        if (Cursor.visible)
		{
            Cursor.visible = false;
        }

        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}