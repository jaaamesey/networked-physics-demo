// Vito Colavito 12022036

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerPoints : MonoBehaviour
{
    private GameObject playerUI;
    private GameObject triggeringObj;
    //public float pointsXP; //experience
    public Text pointsText; // experience text
    public Text moneyText;
    public Text lvlText;

    public int money;
    //var expTexture : Texture;

    //public float xpRequired;

    //current level
    public int currentLevel = 1;

    //current exp amount
    public int playerCurrentExp = 0;

    //level 1 EXP
    public int expBase = 10;

    //exp amount left to next levelup
    public int expLeft = 10;

    //modifier that increases needed exp each level
    public float expMod = 1.15f;


    // Use this for initialization
    void Start()
    {
        //pointsXP = 0;
        money = 0;
        playerUI = GameObject.FindWithTag("PlayerUI");
        pointsText = playerUI.transform.Find("PointsText").GetComponent<Text>();
        moneyText = playerUI.transform.Find("MoneyText").GetComponent<Text>();
        lvlText = playerUI.transform.Find("LevelText").GetComponent<Text>();
        pointsText.text = playerCurrentExp.ToString();
        moneyText.text = money.ToString();
        lvlText.text = currentLevel.ToString();
        //xpRequired = 100;
    }

    // Update is called once per frame
    void Update()
    {
    }

    /* public void addPoints(float pointsToAdd)
    {

    } old addPoints method*/

    /* public void subtractPoints(float pointsToSubtract)
    {
        if (pointsXP - pointsToSubtract <= 0)
        {
            pointsXP = 0;
        }
        else
        {
            pointsXP -= pointsToSubtract;
            pointsText.text = pointsXP.ToString();
        }
    }*/

    private void OnCollisionEnter(Collision other)
    {
        //Contact Add's Points - Vito
        if (other.gameObject.CompareTag("Enemy"))
        {
            //addPoints(10f); Old method to add points
            GainExp(5);
            pointsText.text = playerCurrentExp.ToString();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Pickup Points
        if (other.CompareTag("Money Pickup"))
        {
            money += 50;
            triggeringObj = other.gameObject;
            Destroy(triggeringObj); // Triggers Destroy
            moneyText.text = money.ToString();
        }
    }

    public void GainExp(int e)
    {
        playerCurrentExp += e;
        if (playerCurrentExp >= expLeft)
        {
            LvlUp();
        }
    }


    void LvlUp()
    {
        playerCurrentExp -= expLeft;
        currentLevel++;
        float t = Mathf.Pow(expMod, currentLevel);
        expLeft = (int) Mathf.Floor(expBase * t); //rounds down
        lvlText.text = currentLevel.ToString();
    }

    // Keep public to be accessed by player character - Jamie
    public void Reset()
    {

        currentLevel = 1;
        currentLevel = 1;
        //current exp amount
        playerCurrentExp = 0;
        //level 1 EXP
        expBase = 10;
        //exp amount left to next levelup
        expLeft = 10;
        //modifier that increases needed exp each level
        expMod = 1.15f;
        pointsText.text = playerCurrentExp.ToString();
        lvlText.text = currentLevel.ToString();
    }

    /* Old level up function
     * void LevelUp()
    {
        level += 1;
        pointsXP = 0;

        switch (level)
        {
            case 2:
                Debug.Log("Congratulations! You hit level 2");
                xpRequired = 200;
                break;

            case 3:
                Debug.Log("Congratulations! You hit level 3");
                xpRequired = 300;
                break;
        }
    } */

    /* void Exp()
    {
        if (pointsXP >= xpRequired)
        {
            LevelUp();
        }
    }*/
    /* void OnGUI()
    {
        GUI.DrawTexture(Rect(300, 500, 800 * (playerCurrentExp / expLeft), 30), expTexture);
    }*/
}