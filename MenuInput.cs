using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script attaches to the camera as a component and then reads data from the inputControl script
//Menu objects(like buttons, selectors, etc) can then read from this object to determine the input state of each player.
//The character setup screen uses this to allow players to control their own customization menu.

public struct menuInputState {
    public float LX;
    public float LY;
    public int A;
    public int B;
    public int X;
    public int START;

}

public class MenuInput : MonoBehaviour
{
    public int player;

    public int PlayerController = 0;
    private float LX;
    private float LY;
    private int AButton = 1;
    private int BButton = 2;
    private int XButton = 0;
    private int STARTButton = 9;
    const float deadzone = 0.25f;

    private int A;
    private int B;
    private int X;
    private int START;


    public bool left;
    public bool right;
    public bool up;
    public bool down;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        getInputs();
    }

    //Menu objects call this method to read the input state from this player
    public menuInputState getState() {
        menuInputState x = new menuInputState();
        x.A = A;
        x.B = B;
        x.X = X;
        x.START = START;
        x.LX = LX;
        x.LY = LY;

        return x;
    }

    public void getInputs()
    {
        //Gets gets controller data depending on which player this object belongs to
        switch (this.player) {
            case 1:
                PlayerController = InputControl.P1JOY;
                LX = InputControl.P1LX;
                LY = InputControl.P1LY;
                break;
            case 2:
                PlayerController = InputControl.P2JOY;
                LX = InputControl.P2LX;
                LY = InputControl.P2LY;
                break;
            case 3:
                PlayerController = InputControl.P3JOY;
                LX = InputControl.P3LX;
                LY = InputControl.P3LY;
                break;
            case 4:
                PlayerController = InputControl.P4JOY;
                LX = InputControl.P4LX;
                LY = InputControl.P4LY;
                break;
        }




        //Controller 999 is used as noinput for trainingmode
        if (PlayerController != 0 && PlayerController != 999)
        {
            AButton = InputControl.mapButtons("A", player);
            BButton = InputControl.mapButtons("B", player);
            XButton = InputControl.mapButtons("X", player);
            STARTButton = InputControl.mapButtons("STR", player);

            //Chekcs to see if the button is pressed by the joystick this player owns
            if (Input.GetKeyDown("joystick " + (PlayerController) + " button " + InputControl.mapButtons("A", player)))
            {
                A = 1;
            }
            if (Input.GetKeyDown("joystick " + (PlayerController) + " button " + InputControl.mapButtons("B", player)))
            {
                B = 1;
            }
            if (Input.GetKeyDown("joystick " + (PlayerController) + " button " + InputControl.mapButtons("X", player)))
            {
                X = 1;
            }
            if (Input.GetKeyDown("joystick " + (PlayerController) + " button " + InputControl.mapButtons("STR", player)))
            {
                START = 1;
            }

            //Controls the stick values read by menu objects
            if (LX < deadzone * -1) {
                right = false;
                left = true;
            }
            else if (LX > deadzone)
            {
                left = false;
                right = true;
            }
            if (LY < deadzone * -1)
            {
                down = false;
                up = true;

            }
            else if (LY > deadzone)
            {
                up = false;
                down = true;
            }
            if (Mathf.Abs(LX) < deadzone) {
                right = false;
                left = false;
            }
            if (Mathf.Abs(LY) < deadzone)
            {
                up = false;
                down = false;
            }


        }

    }



    //Clear all inputs from the last frame
    public void clearInputs()
    {
        A = 0;
        B = 0;
        X = 0;
        START = 0;
    }

}
