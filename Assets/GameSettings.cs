using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField] public Vector3 SpriteTransformOrientation = -Vector3.right;
    [SerializeField] public Vector3 SpriteTransformOffset = Vector3.zero;

    private void Awake()
    {
        GlobalSettings.SpriteBillboardVector = SpriteTransformOrientation;
        GlobalSettings.SpriteOffsetVector = SpriteTransformOffset;
    }
}

public static class GlobalSettings
{
    public static Vector3 SpriteBillboardVector;
    public static Vector3 SpriteOffsetVector;
}
