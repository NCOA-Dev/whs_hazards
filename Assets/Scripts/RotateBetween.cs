using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBetween : MonoBehaviour
{
    public float speed = 0.3f;
    public float speed2 = 0.3f;
    public float angle = 20f;
    public GameObject secondObj;

    public float delay = 0f;
    private float timer = 0;

    Vector3 pos;
    Vector3 pos2;

    private void Start()
    {
        pos = this.transform.eulerAngles;
        pos2 = secondObj.transform.eulerAngles;

        transform.rotation = Quaternion.Euler(pos.x, pos.y + Mathf.Sin(Time.realtimeSinceStartup * speed) * angle, pos.z);
        secondObj.transform.Rotate(new Vector3(0, 0, pos2.z * speed2 * Time.deltaTime));
    }

    void FixedUpdate()
    {
        if (timer < delay)
		{
            timer += Time.deltaTime;
		}
        else
		{
            transform.rotation = Quaternion.Euler(pos.x, pos.y + Mathf.Sin((Time.realtimeSinceStartup - delay) * speed) * angle, pos.z);
            secondObj.transform.Rotate(new Vector3(0, 0, pos2.z * -speed2 * Time.deltaTime));
        }
    }
}
