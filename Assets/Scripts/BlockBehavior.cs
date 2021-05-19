using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class BlockBehavior : MonoBehaviour
{
    private System.Random randomizer = new System.Random();
    private SpawnTetromino spawner = new SpawnTetromino();
    private int isMoving = 0; // 0 - not moving, negative value - moving to left, positive - to right
    private float lastTimeFall;
    private float lastTimeMove;
    private float stepTime = 0.7f;
    private Vector3 rotationCenter;
    private static int xMax = 10, yMax = 20;
    private static Transform[,] currentGridCondition = new Transform[xMax, yMax];
    private static int score = 0;
    private static int lines = 0;
    private static int levelScore = 0;
    private static int speed = 1;
    private int stepCounter = 0;

    int Score
    {
        get
        {
            return Score;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        rotationCenter = GenerateRotationCenter();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || // check left/right arrow keys released
            Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) // check a/d  keys released
        {
            isMoving = 0; // stop moving
        }
        else if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && isMoving == 0)// check left arrow key pressed
        {
            isMoving = -1; // to left            
        }
        else if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && isMoving == 0) // check right arrow key pressed
        {
            isMoving = 1; // to right            
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            Rotate(90);
            if (!CanMove())
            {
                Rotate(-90);
            }
        }

        if (((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && Time.time - lastTimeFall > stepTime / (10 * speed)) || Time.time - lastTimeFall > stepTime / speed)
        {
            stepCounter++;
            transform.position += new Vector3(0, -1, 0);

            if (!CanMove())
            {
                transform.position -= new Vector3(0, -1, 0);
                GridConditionUpdate();
                LineEvent();
                if (score - levelScore > 200 && speed < 15) 
                {
                    levelScore = score;
                    speed++;
                }
                this.enabled = false;
                if (stepCounter > 2)
                {
                    FindObjectOfType<SpawnTetromino>().SpawnNew();
                }
            }
            lastTimeFall = Time.time;
        }

        if (isMoving != 0 && Time.time - lastTimeMove > stepTime / (5*Math.Pow(speed, 0.5)))
        {
            transform.position += new Vector3(isMoving, 0, 0);
            if (!CanMove())
            {
                transform.position -= new Vector3(isMoving, 0, 0);
            }
            lastTimeMove = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus)&&speed<15) 
        {
            speed++; 
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus) && speed > 1)
        {
            speed--;
        }

    }

    void Rotate(int deg)
    {
        transform.RotateAround(transform.TransformPoint(rotationCenter), new Vector3(0, 0, 1), deg);
        foreach (Transform child in transform)
        {
            child.rotation = Quaternion.Euler(new Vector3(0, 0, child.rotation.eulerAngles.z - deg));
        }
    }

    Vector3 GenerateRotationCenter()
    {
        int maxX = 0, maxY = 0;
        foreach (Transform child in transform)
        {
            int tempX = (int)Math.Round(child.transform.position.x - transform.transform.position.x);
            int tempY = (int)Math.Round(child.transform.position.y - transform.transform.position.y);


            if (maxX < tempX)
            {
                maxX = tempX;
            }
            if (maxY < tempY)
            {
                maxY = tempY;
            }
        }
        if (maxX % 2 == 1)
        {
            return new Vector3(maxX / 2.0f, 0.5f, 0);
        }
        return new Vector3(maxX / 2.0f, (maxY + 1) / 2.0f, 0);
    }

    void GridConditionUpdate()
    {
        foreach (Transform child in transform)
        {
            int roundedX = (int)Math.Round(child.transform.position.x);
            int roundedY = (int)Math.Round(child.transform.position.y);
            currentGridCondition[roundedX, roundedY] = child;

        }

    }

    bool CanMove()
    {
        foreach (Transform child in transform)
        {
            int roundedX = (int)Math.Round(child.transform.position.x);
            int roundedY = (int)Math.Round(child.transform.position.y);
            if (roundedX < 0 || roundedX >= xMax || roundedY < 0 || roundedY >= yMax)
            {
                return false;
            }
            if (currentGridCondition[roundedX, roundedY] != null)
            {
                return false;
            }
        }
        return true;
    }

    void LineEvent()
    {
        int tempScore = 0;
        int lineCounter = 0;
        for (int y = yMax-1; y>=0; y--)
        {
            if (LineCheck(y))
            {
                LineDestroy(y);
                tempScore += (int)Math.Round(10 * Math.Pow(1.5, lineCounter));
                lineCounter++;

                SegmentsLand(y);
            }
        }
        score += tempScore;
        Debug.Log(score);
    }

    bool LineCheck(int line)
    {
        bool isFull = true;
        for (int x = 0; x < xMax; x++)
        {
            Transform tempElem = currentGridCondition[x, line];
            if (tempElem == null)
            {
                isFull = false;
            }
        }
        return isFull;
    }

    void LineDestroy(int line)
    {
        for (int x = 0; x < xMax; x++)
        {
            Destroy(currentGridCondition[x, line].gameObject);
            currentGridCondition[x, line] = null;
        }
        lines++;
    }


    void SegmentsLand(int line)
    {
        for (int i = 1; i < yMax - line; i++) // for all lines up to the destoyed
        {
            for (int j = 0; j < xMax; j++)
            {
                if (currentGridCondition[j, i + line] != null)
                {
                    int currentLine = i + line;
                    Transform child = currentGridCondition[j, currentLine];
                    if (currentGridCondition[j, currentLine - 1] == null)
                    {
                        currentGridCondition[j, currentLine - 1] = child;
                        currentGridCondition[j, currentLine] = null;
                        child.transform.position += new Vector3(0, -1, 0);

                    }
                }
            }
        }
    }

    
}



