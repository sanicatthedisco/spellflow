using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using System;
using UnityEngine.UI;

public class Cucumber {
    public float angle, length;
    public Vector3 position;
    public bool vigor;

    public Cucumber(double theta, double len, double posX, double posZ)
    {
        angle = (float)theta;
        length = (float) len;
        position = new Vector3((float) posX, 0, (float) posZ);

        if (Math.Abs(angle) >= ((3 * Math.PI) / 4) || (Math.Abs(angle) <= (Math.PI / 4))) {
            vigor = true;
        } else {
            vigor = false;
        }
    }
}

public class fingersensor : MonoBehaviour
{
    Controller controller;
    public string color;

    public Transform bar;
    private float activationPitch = -0.75f;

    bool drawing = false;
    bool lastDrawingState = false;

    Color drawingColor, movingColor, red, blue;
    float darkenFactor = 1.5f;

    Material mat;

    List<double> pathX, pathZ;

    private double m; //Regression slope
    private double b; //Regression intercept
    private double theta; //Regression angle

    public Transform forbiddance, vigor;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        controller = new Leap.Controller();
        //Debug.Log(controller);
        mat = GetComponent<Renderer>().material;

        red = Color.red;
        blue = Color.blue;

        pathX = new List<double>();
        pathZ = new List<double>();

        if (color == "red") 
        {
            movingColor = red;
            drawingColor = new Color(red.r * 4f, red.g * 4f, red.b * 4f);
        }
        if (color == "blue")
        {
            movingColor = blue;
            drawingColor = new Color(blue.r * 4f, blue.g * 4f, blue.b * 4f);
        }
    }

    // Update is called once per frame
    void Update() {
        HandList hands = controller.Frame().Hands;


        for (int i = 0; i < hands.Count; i++)
        {
            Hand hand = controller.Frame().Hands[i];
            //Leap.Vector rawTp = controller.Frame().Hand(hand.Id).Fingers[1].TipPosition;
            Leap.Vector rawTp = controller.Frame().Hand(hand.Id).PalmPosition;
            Vector3 tp = TipPosToVector(rawTp);
            if ((tp.x < 0 && color == "blue") || (tp.x > 0 && color == "red")) {
                this.transform.position = tp;
                lastDrawingState = drawing;
                Debug.Log(hand.PalmNormal.Pitch);
                if (hand.PalmNormal.Pitch < activationPitch && !bar.GetComponent<BarController>().gameEnded)
                {
                    drawing = false;
                    mat.color = movingColor;

                    if (lastDrawingState == true)
                    {
                        Cucumber line = CreateShapeFromPoints(pathX, pathZ);
                        pathX.Clear();
                        pathZ.Clear();
                        if (line.length > 0.25f && line.length < 1.4f)
                        {
                            bar.GetComponent<BarController>().changeChalk(0.1f * line.length, color);
                            if (line.vigor)
                            {
                                Transform newLine = Instantiate(vigor, line.position, Quaternion.Euler(0, -(float)(line.angle * 180 / Math.PI), 90));
                                newLine.GetComponent<VigorController>().init(line.length, color, line.angle);
                                newLine.GetComponent<VigorController>().bar = bar;
                                BarController.toDestroy.Add(newLine.gameObject);
                            }
                            else
                            {
                                Transform newLine = Instantiate(forbiddance, line.position, Quaternion.Euler(0, -(float)(line.angle * 180 / Math.PI), 90));
                                newLine.GetComponent<ForbiddanceController>().init(line.length, color, line.angle);
                                BarController.toDestroy.Add(newLine.gameObject);
                            }
                        }
                    }
                }
                else
                {
                    drawing = true;
                    mat.color = drawingColor;
                    pathX.Add(tp.x);
                    pathZ.Add(tp.z);
                }

            }
            
        }
        //string hand = controller.Frame().Hand(0).ToString();
        //Debug.Log(hand);

    }

    Vector3 TipPosToVector (Leap.Vector tp) {
        return new Vector3(tp.x / 100, 0, -tp.z / 100);// - new Vector3(.3f, 0, .38f);
    }


    //Jeffery's Things
    double Hypot(double a, double b)
    {
        return Math.Sqrt((a * a) + (b * b));
    }

    double Mean(List<double> list)
    {
        double accumulate = 0.0;
        foreach (double num in list)
        {
            accumulate += num;
        }
        return (accumulate / list.Count);
    }

    Cucumber CreateShapeFromPoints(List<double> x, List<double> y)
    {
        double accumulate = 0.0;
        foreach (double num in x)
        {
            accumulate += Math.Pow(num - Mean(x), 2.0);
        }
        double variance = accumulate / x.Count;
        //Debug.Log("Variance: " + variance.ToString());

        bool swap = false;

        double lastX = x[x.Count - 1];
        double lastY = y[y.Count - 1];

        double posX = 0.0;
        double posY = 0.0;
        double length = 0.0;

        //findSlope
        if (variance < 0.007)
        {
            posX = (x[0] + lastX) / 2.0;
            posY = (y[0] + lastY)/2;

            length = Math.Sqrt(Math.Pow(lastX - x[0], 2.0) + Math.Pow(y[0] - lastY, 2.0));

            if (lastY - y[0] >= 0)
            {
                theta = Math.PI / 2;
            }
            else
            {
                theta = Math.PI / -2;
            }

        }
        else if (findSlope(x, y, lastX, lastY, ref swap))
        {
            //findIntercept
            findIntercept(x, y);

            if (swap)
            {
                List<double> bufferList = x;
                x = y;
                y = bufferList;
                b = -b / m;
                m = 1.0f / m;
            }
            double y1 = m * x[0] + b;
            double yf = m * lastX + b;
            //Midpoint
            posX = (x[0] + lastX) / 2.0;
            posY = (y1 + yf) / 2.0;

            //Length
            length = Math.Sqrt(Math.Pow(lastX - x[0], 2.0) + Math.Pow(yf - y1, 2.0));


        }

        return new Cucumber(theta, length, posX, posY);

    }

    bool findSlope(List<double> x, List<double> y, double lastX, double lastY, ref bool swap)
    {

        if (Math.Abs((lastY - y[0]) / (lastX - x[0])) > 1.5f)
        {
            //std::cout << "Swap" << std::endl;
            List<double> bufferList = x;
            x = y;
            y = bufferList;
            swap = true;
        }

        //slope
        double numerator = 0.0;
        double denominator = 0.0;
        double deltaX = 0.0;
        for (int i = 0; i < x.Count; i++)
        {
            deltaX = (x[i] - Mean(x));
            numerator += deltaX * (y[i] - Mean(y));
            denominator += deltaX * deltaX;
        }

        theta = Math.Atan2(numerator, denominator);

        m = numerator / denominator;

        return true;
    }

    void findIntercept(List<double> x, List<double> y)
    {
        //Intercept
        b = Mean(y) - m * Mean(x);
    }
}
