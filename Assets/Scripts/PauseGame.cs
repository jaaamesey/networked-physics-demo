using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour
{   //vito Colavito 12022036
    public Transform canvas;





    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }
    public void Pause()
    {
        if (canvas.gameObject.activeInHierarchy == false)
        {
            canvas.gameObject.SetActive(true);
            //Time.timeScale = 0;
            //Player.GetComponent<FirstPersonController>().enabled = false;
        }
        else
        {
            canvas.gameObject.SetActive(false);
            //Time.timeScale = 1;
            //Player.GetComponent<FirstPersonController>().enabled = true;
        }


    }
    public void CloseGame()
    {
        Application.Quit();
    }
}