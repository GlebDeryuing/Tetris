using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoShow : MonoBehaviour
{
    [SerializeField]
    Text textScrore; // text object showing game score
    [SerializeField]
    Text textLines; // text object showing count of destroyed lines
    [SerializeField]
    Text textSpeed; // text object showing current speed
    [SerializeField]
    Canvas endScreen; // end screen showed on the end of the game

    /// <summary>
    /// Update speed value on player's viewscreen
    /// </summary>
    /// <param name="speed">new speed value</param>
    public void UpdateSpeed(int speed)
    {
        // generating string in format of "XX/15"
        string sp = speed.ToString(); 
        if (speed < 10)
        {
            sp = "0" + sp;
        }
        textSpeed.text = sp + "/15";
    }

    /// <summary>
    /// Update score value on player's viewscreen
    /// </summary>
    /// <param name="score">new score value</param>
    public void UpdateScore(int score)
    {
        textScrore.text = score.ToString();
    }

    /// <summary>
    /// Update destroyed lines value on player's viewscreen
    /// </summary>
    /// <param name="lines">new destroyed lines value</param>
    public void UpdateLines(int lines)
    {
        textLines.text = lines.ToString();
    }

    /// <summary>
    /// Show end screen by moving it to the camera view
    /// </summary>
    public void EndGame()
    {
        endScreen.transform.position += new Vector3(30f, 0, 0);
    }

    /// <summary>
    /// Hide end screen by moving it out from the camera view
    /// </summary>
    public void StartGame()
    {
        endScreen.transform.position -= new Vector3(30f, 0, 0);
    }


}
