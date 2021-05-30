using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    private static Color transparent = new Color(1f, 1f, 1f, 0f);
    private static Color colored = new Color(1f, 1f, 1f, 1f);
    // Start is called before the first frame update
    void Start()
    {
        transform.gameObject.GetComponent<SpriteRenderer>().color = transparent;
    }

    public void EndGame()
    {
        transform.gameObject.GetComponent<SpriteRenderer>().color = colored;
    }

    public void StartGame()
    {
        transform.gameObject.GetComponent<SpriteRenderer>().color = transparent;
    }
}
