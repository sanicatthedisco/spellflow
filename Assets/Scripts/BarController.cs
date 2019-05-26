using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    public bool gameEnded;
    public Text gameOverText;
    public Button restartButton;

    public static List<GameObject> toDestroy;

    float chalk;
    // Start is called before the first frame update
    void Start()
    {
        toDestroy = new List<GameObject>();
        restart();
    }

    // Update is called once per frame

    public void changeChalk(float amt, string color) {
        if (color == "red") {
            chalk -= amt;
        } else {
            chalk += amt;
        }

        //Debug.Log(chalk);

        if (chalk < -0.5f) {
            chalk = -0.5f;
            gameEnded = true;
        }
        else if (chalk > 0.5f) {
            chalk = 0.5f;
            gameEnded = true;
        }
        //Debug.Log(transform.position.x);
        transform.position = new Vector3(-3.9f * chalk, transform.position.y, transform.position.z);
        //Debug.Log(transform.position.x);
        if (gameEnded == true) {
            gameOverText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }
    }

    public void restart() {
        gameEnded = false;
        gameOverText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        chalk = 0.0f;
        foreach (GameObject obj in toDestroy) {
            Destroy(obj);
        }

        toDestroy.Clear();
    }
}
