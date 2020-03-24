using System;
using UnityEngine;

// Simply show the cursor in the Editor but hide it in the Player.
public class CursorController : MonoBehaviour
{
    void OnEnable()
    {
#if UNITY_EDITOR
        Cursor.visible = true;
#else
        Cursor.visible = false;
#endif
    }
}
