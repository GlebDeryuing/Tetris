using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class BlockBehavior : MonoBehaviour
{
    private int isMoving = 0; // 0 - not moving, negative value - moving to left, positive - to right
    private float lastTimeFall;
    private float lastTimeMove;
    private float stepTime = 0.8f;
    private Vector3 rotationCenter;
    private int xMax = 10, yMax = 20;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {    if (Input.GetKeyUp(KeyCode.LeftArrow)||Input.GetKeyUp(KeyCode.RightArrow) || // check left/right arrow keys released
            (Input.GetKeyDown(KeyCode.RightArrow)&&Input.GetKeyDown(KeyCode.LeftArrow))) // or both buttons are pressed
        {            
            isMoving = 0; // stop moving
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && isMoving == 0) // check left arrow key pressed
        {
            isMoving = -1; // to left            
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && isMoving == 0) // check right arrow key pressed
        {
            isMoving = 1; // to right            
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationCenter), new Vector3(0, 0, 1), 90);
            if (!CanMove())
            {
                transform.RotateAround(transform.TransformPoint(rotationCenter), new Vector3(0, 0, 1), -90);
            }
        }
        if ((Input.GetKey(KeyCode.DownArrow) && Time.time - lastTimeFall>stepTime/10) || Time.time - lastTimeFall > stepTime)
        {
            transform.position += new Vector3(0, -1, 0);
            if (!CanMove())
            {
                transform.position -= new Vector3(0, -1, 0);
            }
            lastTimeFall = Time.time;
        }
        if (isMoving != 0 && Time.time - lastTimeMove > stepTime / 5)
        {
            transform.position += new Vector3(isMoving, 0, 0);
            if (!CanMove())
            {
                transform.position -= new Vector3(isMoving, 0, 0);
            }
            lastTimeMove = Time.time;
        }

    }

    bool CanMove()
    {
        foreach (Transform child in transform)
        {
            int roundedX = (int)Math.Round(child.transform.position.x);
            int roundedY = (int)Math.Round(child.transform.position.y);

            if (roundedX<0||roundedX>=xMax||roundedY<0||roundedY>=yMax)
            {
                return false;
            }
        }
        return true;
    }

}
