using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForbiddanceController : MonoBehaviour
{
    public float yScale;
    public string color;
    public float angle;
    public int life;

    Material mat;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale -= new Vector3(0.4f, 0, 0.4f);
        life = 1;
    }

    public void init(float yScale, string color, float angle) {
        float oldSize = this.GetComponent<Renderer>().bounds.size.y;
        Vector3 oldScale = this.transform.localScale;

        //Debug.Log(oldSize);
        oldScale.y = yScale * oldScale.y * 0.1f / oldSize;

        this.transform.localScale = oldScale;

        mat = GetComponent<Renderer>().material;
        if (color == "red")
        {
            mat.color = Color.red;
        }
        else
        {
            mat.color = Color.blue;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DecreaseLife() {
        life --;
        if (life == 0) {
            Material mat = this.GetComponent<Renderer>().material;
            mat.color = new Color(mat.color.r * 4f, mat.color.g * 4f, mat.color.b * 4f);
        }
    }
}
