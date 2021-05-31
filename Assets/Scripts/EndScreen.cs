using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    private static Color transparent = new Color(1f, 1f, 1f, 0f); // transparent
    private static Color colored = new Color(1f, 1f, 1f, 1f);   // white
    private static Color current = transparent; 
    private static bool needToRestart = false;
    // Start is called before the first frame update
    void Update()
    {
        transform.gameObject.GetComponent<SpriteRenderer>().color = current;    // on every frame - make objects color from current
        if (needToRestart&&Input.anyKeyDown)    // if need to restart and any key is pressed
        {
            StartGame(); // start game
        }
    }

    public void EndGame()
    {
        current = colored; // coloring endgame screen
        needToRestart = true;  
    }

    public void StartGame()
    {
        current = transparent;  // disable colored screen
        needToRestart = false;
        FindObjectOfType<BlockBehavior>().Restart();    //restart game
    }
}
