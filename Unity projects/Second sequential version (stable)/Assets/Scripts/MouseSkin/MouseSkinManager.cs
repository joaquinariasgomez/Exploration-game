using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSkinManager : MonoBehaviour {

    public Texture2D defaultTexture;

    private void Start()
    {
        Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);
    }
}
