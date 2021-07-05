using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Photon.Pun;

public class BlockBehavior : MonoBehaviourPunCallbacks
{
    private System.Random randomizer = new System.Random(); // randomizer variable
    private int isMoving = 0; // variable, contains current X-movement condition: 0 - not moving, negative value - moving to left, positive - to right
    private float lastTimeFall; // used to calculate time between fall steps
    private float lastTimeMove; // used to calculate time between move steps
    private float stepTime = 0.7f;  // time what mean how many sec script will wait before next move
    private Vector3 rotationCenter; // center of the rotation
    private static int xMax = 10, yMax = 20;    // max X and max Y coordinates - borders of the game field
    private static Transform[,] currentGridCondition = new Transform[xMax, yMax];   // array of cubes/nulls which contains the condition of the every grid: if it's not empty, it contains link to the object in that position
    private static int score = 0;   // score value
    private static int lines = 0;   // destroyed lines count
    private static int levelScore = 0;  // score at the last level change (used in the level change)
    private static int speed = 1;   // current game speed
    private int stepCounter = 0;    // how many steps to the bottom did the current object before stop moving (used to determine the finish of the game)
    private int rotationCounter = 0;

    private new PhotonView photonView;
    int Score
    {
        get
        {
            return score;
        }
    }

    int Length
    {
        get
        {
            int maxX = 0;
            foreach (Transform child in transform)
            {
                int tempX = (int)Math.Round(child.transform.position.x - transform.transform.position.x);
                if (maxX < tempX)
                {
                    maxX = tempX;
                }
                
            }
            return maxX;
        }
    }


    // Start is called before the first frame update
    void Start()
    {   // generate new rotation center
        rotationCenter = GenerateRotationCenter();
        // update gui information
        FindObjectOfType<InfoShow>().UpdateSpeed(speed);
        FindObjectOfType<InfoShow>().UpdateScore(score);
        FindObjectOfType<InfoShow>().UpdateLines(lines);
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || // check left/right arrow keys released
            Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) // check a/d  keys released
        {
            isMoving = 0; // stop moving
        }
        else if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && isMoving == 0)// check left key pressed
        {
            isMoving = -1; // to left            
        }
        else if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && isMoving == 0) // check right key pressed
        {
            isMoving = 1; // to right            
        }
                
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) // check up key pressed
        {
            Rotate(90);
            // if the displacement has led to an invalid position of the tetro
            if (!CanMove())
            {
                if (transform.position.x < 1)
                {
                    transform.position += new Vector3(1, 0, 0);
                    if (!CanMove())
                    {
                        transform.position += new Vector3(1, 0, 0);
                        if (!CanMove())
                        {
                            transform.position -= new Vector3(2, 0, 0);
                            Rotate(-90);
                        }
                    }
                }
                else if (transform.position.x + Length > 8)
                {
                    transform.position -= new Vector3(1, 0, 0);
                    if (!CanMove())
                    {
                        transform.position -= new Vector3(1, 0, 0);
                        if (!CanMove())
                        {
                            transform.position += new Vector3(2, 0, 0);
                            Rotate(-90);
                        }
                    }
                }
                else
                {
                    Rotate(-90);
                }
            }
        }

        // fall dows if:
        if (((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && PhotonNetwork.Time - lastTimeFall > stepTime / (10 * speed)) ||
            PhotonNetwork.Time - lastTimeFall > stepTime / speed)
        // if the down / s key is pressed and the time elapsed since the last movement is more than the time of one step divided by 10 speeds 
        // or the time since the last step is more than the time of one step divided by 1 speed
        {
            stepCounter++;  // plus one step to the bottom
            transform.position += new Vector3(0, -1, 0);    // move the tetro to the bottom
            // if the displacement has led to an invalid position of the tetro
            if (!CanMove()) // the figure has finally landed
            {
                transform.position -= new Vector3(0, -1, 0);    // return older position
                GridConditionUpdate();  // update grid condition
                LineEvent();    // check, destroy  and move lines
                if (score - levelScore >= 2000 && speed < 15)    // if speed is not at max value and player riched +2000 score from the last speedup, do speedup
                {
                    levelScore = score; 
                    speed++;    
                    FindObjectOfType<InfoShow>().UpdateSpeed(speed);
                }
                this.enabled = false;   // disable control of this figure

                if (stepCounter > 2)    // if the figure rested after more then one step, spawn new one
                {
                    FindObjectOfType<SpawnTetromino>().SpawnNew();
                }
                else    // if the figure rested after one step, end the game
                {
                    FindObjectOfType<EndScreen>().EndGame();
                }
            }
            lastTimeFall = (float)PhotonNetwork.Time;   // update falltime
        }

        // move to left/right:
        if (isMoving != 0 && PhotonNetwork.Time - lastTimeMove > stepTime / (5*Math.Pow(speed, 0.5)))
        // if on of the buttons is pressed and times before steps is bigger that stepTime/5 (plus a slight acceleration from increasing the speed of the game) 
        {
            transform.position += new Vector3(isMoving, 0, 0);  // move it to left/right
            // if the displacement has led to an invalid position of the tetro
            if (!CanMove()) // hit the wall
            {
                transform.position -= new Vector3(isMoving, 0, 0); // return older position
            }
            lastTimeMove = (float)PhotonNetwork.Time; // update movetime
        }

        //speed up/down:
        if (Input.GetKeyDown(KeyCode.KeypadPlus)&&speed<15) // if + is pressed and speed is not at the max value 
        {
            speed++;
            FindObjectOfType<InfoShow>().UpdateSpeed(speed);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus) && speed > 1)    // if - is pressed and speed is not at the min value 
        {
            speed--;
            FindObjectOfType<InfoShow>().UpdateSpeed(speed);
        }

    }

    /// <summary>
    /// Rotation function: rotate the object to the variable value
    /// </summary>
    /// <param name="deg">degrees to rotate</param>
    void Rotate(int deg)
    {
        int temp = Math.Sign(deg);
        rotationCounter += temp;
        transform.RotateAround(transform.TransformPoint(rotationCenter), new Vector3(0, 0, 1), deg);    // rotate the object
        foreach (Transform child in transform)  // rotate all child object backward to save the pattern
        {
            child.Rotate(0, 0, -deg);
        }
        if (NeedToMoveAfterRotate())
        {
            if (rotationCounter % 4 == 2)
            {
                transform.position += new Vector3(0, -temp, 0);
                if (!CanMove())
                {
                    transform.position -= new Vector3(0, -temp, 0);
                }
            }

            else if (rotationCounter % 4 == 3)
            {
                transform.position += new Vector3(temp, temp, 0);
                if (!CanMove())
                {
                    transform.position -= new Vector3(temp, temp, 0);
                }
            }
            else if (rotationCounter % 4 == 0)
            {
                transform.position += new Vector3(-temp, 0, 0);
                if (!CanMove())
                {
                    transform.position -= new Vector3(-temp, 0, 0);
                }
            }
        }
    }

    bool NeedToMoveAfterRotate()
    {
        if (rotationCenter.x == 0.5f) return false;
        if (FindObjectOfType<SpawnTetromino>().Type.Substring(10, 1) == "T") return false;
        return true;
    }
    /// <summary>
    /// Generate rotation center using figure form
    /// </summary>
    /// <returns>Point of rotation</returns>
    Vector3 GenerateRotationCenter()
    {
        // check max X and Y values of child positions in the current figure
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


        if (maxX % 2 == 1) // if it is square or line
        {
            return new Vector3(maxX / 2.0f, 0.5f, 0);
        }
        return new Vector3(maxX / 2.0f, (maxY + 1) / 2.0f, 0); // else for other figures
    }

    /// <summary>
    /// Update current grid condition: save link to the all child objects to the currentGridCondition[,]
    /// </summary>
    void GridConditionUpdate()
    {
        foreach (Transform child in transform)
        {
            int roundedX = (int)Math.Round(child.transform.position.x);
            int roundedY = (int)Math.Round(child.transform.position.y);
            currentGridCondition[roundedX, roundedY] = child;
        }
    }

    /// <summary>
    /// Check if the position of object is avalible
    /// </summary>
    /// <returns>Can the object be there without any problems</returns>
    bool CanMove()
    {
        foreach (Transform child in transform) // for all child blocks
        {
            // check their positions
            int roundedX = (int)Math.Round(child.transform.position.x);
            int roundedY = (int)Math.Round(child.transform.position.y);
            // if it hit the borderline, return false
            if (roundedX < 0 || roundedX >= xMax || roundedY < 0 || roundedY >= yMax)
            {
                return false;
            }
            // if it hit the other block in the grid, return false
            if (currentGridCondition[roundedX, roundedY] != null)
            {
                return false;
            }
        }
        // if there were no incidents, then return true
        return true;
    }

    /// <summary>
    /// Complexly processes the behavior of lines: searches for filled lines, removes them, shifts the remaining ones down
    /// </summary>
    void LineEvent()
    {
        int tempScore = 0;  // temporary value of the score for the current line event
        int lineCounter = 0; // temporary value of the lines destroyed for the current line event
        for (int y = yMax-1; y>=0; y--) // checking from top to bottom
        {
            if (LineCheck(y)) // if the line is filled
            {
                LineDestroy(y); // destroy it
                tempScore += (int)Math.Round(100 * Math.Pow(2, lineCounter));   // add score (1 line - 100, 2 lines - 300, 3 lines - 800, 4 lines - 1500)
                lineCounter++;  // add lines counter
                SegmentsLand(y); // shifts flying ones down
            }
        }
        // update score
        score += tempScore;
        FindObjectOfType<InfoShow>().UpdateScore(score);
    }

    /// <summary>
    /// Check if i-line is full
    /// </summary>
    /// <param name="line">i value</param>
    /// <returns>true if line is full, else false</returns>
    bool LineCheck(int line)
    {
        // if the grid contains at least one null in the given line, then returns false
        for (int x = 0; x < xMax; x++)
        {
            Transform tempElem = currentGridCondition[x, line];
            if (tempElem == null)
            {
                return false;
            }
        }
        return true; // else returns true
    }

    /// <summary>
    /// Destroy i-line
    /// </summary>
    /// <param name="line">i value</param>
    void LineDestroy(int line)
    {
        for (int x = 0; x < xMax; x++)  // for all elements in the line
        {
            Transform temp = currentGridCondition[x, line];
            if (temp != null)
            {
                Transform par = temp.parent;
                temp.parent = null;
                PhotonNetwork.Destroy(temp.gameObject);  // destroy
                currentGridCondition[x, line] = null;   // clean up grid at the position
                if (par.childCount == 0)    // clean up parent object if it is empty
                {
                    PhotonNetwork.Destroy(par.gameObject);
                }
            }
        }
        // update line value
        lines++;
        FindObjectOfType<InfoShow>().UpdateLines(lines);
    }

    /// <summary>
    /// Land down all segments after deleting i-line
    /// </summary>
    /// <param name="line">i-value</param>
    void SegmentsLand(int line)
    {
        for (int i = 1; i < yMax - line; i++) // for all lines up to the destoyed
        {
            for (int j = 0; j < xMax; j++)  // for every element at the line
            {
                if (currentGridCondition[j, i + line] != null)  // if it is not empty
                {
                    //move it down
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

    public void Restart()
    {
        for (int i=0; i<yMax; i++)
        {
            LineDestroy(i);
        }
        score = 0;
        lines = 0;
        speed = 1;
        levelScore = 0;
        FindObjectOfType<SpawnTetromino>().Type = null;
        FindObjectOfType<SpawnTetromino>().SpawnNew();
}

    
}



