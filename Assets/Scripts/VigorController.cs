using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VigorController : MonoBehaviour
{
    public float yScale;
    public string color;
    public float angle;
    float dir;

    public Transform bar;

    float speed;

    Material mat;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale -= new Vector3(0.47f, 0, 0.47f);

    }

    public void init(float yScale, string color, float angle)
    {
        float oldSize = this.GetComponent<Renderer>().bounds.size.y;
        Vector3 oldScale = this.transform.localScale;

        this.color = color;

        //Debug.Log(oldSize);
        oldScale.y = yScale * oldScale.y * 0.1f / oldSize;

        this.transform.localScale = oldScale;

        mat = GetComponent<Renderer>().material;
        if (color == "red")
        {
            mat.color = Color.red;
            dir = 1;
        }
        else
        {
            mat.color = Color.blue;
            dir = -1;
        }

        Debug.Log(yScale);
        speed = (float) Math.Pow((double)yScale + 1.3, 2.5) / -250;
        Debug.Log(speed);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += Quaternion.Euler(0, 90, 0) * transform.forward * speed * dir;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Forbiddance")
        {
            if (other.GetComponent<ForbiddanceController>().life == 0)
            {
                Destroy(other.gameObject);
            } else {
                other.GetComponent<ForbiddanceController>().DecreaseLife();
            }
            Destroy(this.gameObject);
        }
        if (other.gameObject.tag == "RedGoal" && color == "blue") {
            bar.GetComponent<BarController>().changeChalk(-0.20f, "blue");
        }
        if (other.gameObject.tag == "BlueGoal" && color == "red") {
            bar.GetComponent<BarController>().changeChalk(-0.20f, "red");
        }
    }
}
