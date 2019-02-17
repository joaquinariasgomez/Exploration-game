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
    private Texture2D drawTexture;

    private Vector2 mousePosition;
    private Vector2 movementVector;

    private float secondsCounter = 0;
    private float minimumTimeInState = 0.3f;

    private float variationBorderline = 3f;

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

    public void SetTexture(string type)
    {
        if(type == "point")
        {
            drawTexture = Point_hand;
        }
        Cursor.SetCursor(drawTexture, Vector2.zero, CursorMode.Auto);
    }

    public void UnsetTexture()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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

        if (largerVariation > variationBorderline)
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
            if (largerVariation < -variationBorderline)
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

        secondsCounter += Time.deltaTime;
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
        TextureUpdate();
    }

    private void OnDrawGizmos()
    {
    }
}
