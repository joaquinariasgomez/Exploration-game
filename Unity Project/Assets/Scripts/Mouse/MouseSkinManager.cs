using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSkinManager : MonoBehaviour {

    public Texture2D Center_hand;
    public Texture2D Left_hand;
    public Texture2D Right_hand;
    public Texture2D Up_hand;
    public Texture2D Down_hand;
    public Texture2D Point_hand;
    public Texture2D Grab_hand;
    public Texture2D Defend_texture;
    public Texture2D Attack_texture;
    private Texture2D drawTexture;

    private Vector2 mousePosition;
    private Vector2 movementVector;

    private float timeToCompleteFramePaused = 0.02f;

    private float secondsCounter = 0;
    private float minimumTimeInState = 0.15f;   //.3f;

    private float variationThreshold = 1.5f;   //3f;

    [HideInInspector]
    public bool isPointingAstronaut = false;
    [HideInInspector]
    public bool isPointingButton = false;
    [HideInInspector]
    public bool isPointingMenu = false;
    [HideInInspector]
    public bool isPointingAlienToAttack = false;
    [HideInInspector]
    public bool isPointingAstronautToDefend = false;

    private void Start()
    {
        drawTexture = Center_hand;

        Cursor.SetCursor(drawTexture, Vector2.zero, CursorMode.Auto);
        mousePosition = Input.mousePosition;
        movementVector = Vector2.zero;
    }

    private void UpdateMousePosition()
    {
        Vector2 newMousePosition = Input.mousePosition;
        movementVector = newMousePosition - mousePosition;
        mousePosition = newMousePosition;
    }

    public void Point(string cause = "astronaut")
    {
        switch(cause)
        {
            case "astronaut": isPointingAstronaut = true; break;
            case "button": isPointingButton = true; break;
            case "defend": isPointingAstronautToDefend = true; break;
            case "attack": isPointingAlienToAttack = true; break;
        }
    }

    public void PointMenu()
    {
        isPointingMenu = true;
    }

    public void Unpoint(string cause = "astronaut")
    {
        switch (cause)
        {
            case "astronaut": isPointingAstronaut = false; break;
            case "button": isPointingButton = false; break;
            case "defend": isPointingAstronautToDefend = false; break;
            case "attack": isPointingAlienToAttack = false; break;
        }
    }

    public void UnpointMenu()
    {
        isPointingMenu = false;
    }

    private void TextureUpdate()
    {
        float xVariation = movementVector.x;
        float yVariation = movementVector.y;
        float largerVariation = xVariation;
        bool xAxis = true;
        if(Mathf.Abs(yVariation) > Mathf.Abs(xVariation))
        {
            largerVariation = yVariation;
            xAxis = false;
        }

        if (largerVariation > variationThreshold)
        {
            if(xAxis)
            {
                drawTexture = Right_hand;
            }
            else
            {
                drawTexture = Up_hand;
            }
        }
        else
        {
            if (largerVariation < -variationThreshold)
            {
                if (xAxis)
                {
                    drawTexture = Left_hand;
                }
                else
                {
                    drawTexture = Down_hand;
                }
            }
            else
            {
                drawTexture = Center_hand;
            }
        }

        if(PauseMenu.GamePaused)
        {
            secondsCounter += timeToCompleteFramePaused;
        }
        else
        {
            secondsCounter += Time.deltaTime;
        }
        if (secondsCounter > minimumTimeInState)
        {
            secondsCounter = 0;
            //DO THINGS EVERY minimumTimeInState SECONDS
            Cursor.SetCursor(drawTexture, Vector2.zero, CursorMode.Auto);
        }
    }

    private void Update()
    {
        UpdateMousePosition();
        if(PauseMenu.GamePaused)    //PAUSE MENU
        {
            if (isPointingMenu)
            {
                Cursor.SetCursor(Point_hand, Vector2.zero, CursorMode.Auto);
                secondsCounter = minimumTimeInState;    //Reset counter for TextureUpdate()
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    Cursor.SetCursor(Grab_hand, Vector2.zero, CursorMode.Auto);
                    secondsCounter = minimumTimeInState;    //Reset counter for TextureUpdate()
                }
                else
                {
                    TextureUpdate();
                }
            }
        }
        else
        {
            if (isPointingAstronaut || isPointingButton)
            {
                Cursor.SetCursor(Point_hand, Vector2.zero, CursorMode.Auto);
                secondsCounter = minimumTimeInState;    //Reset counter for TextureUpdate()
            }
            else
            {
                if(isPointingAstronautToDefend)
                {
                    Cursor.SetCursor(Defend_texture, Vector2.zero, CursorMode.Auto);
                    secondsCounter = minimumTimeInState;    //Reset counter for TextureUpdate()
                }
                else
                {
                    if(isPointingAlienToAttack)
                    {
                        Cursor.SetCursor(Attack_texture, Vector2.zero, CursorMode.Auto);
                        secondsCounter = minimumTimeInState;    //Reset counter for TextureUpdate()
                    }
                    else
                    {
                        if (Input.GetMouseButton(0))
                        {
                            Cursor.SetCursor(Grab_hand, Vector2.zero, CursorMode.Auto);
                            secondsCounter = minimumTimeInState;    //Reset counter for TextureUpdate()
                        }
                        else
                        {
                            TextureUpdate();
                        }
                    }
                }
            }
        }
    }
}
