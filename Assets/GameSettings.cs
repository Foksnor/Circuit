using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private Vector3 globalSpriteTransformOrientation = new (0, -55, 90);
    [SerializeField] private Vector3 globalSpriteTransformOffset = new (0.4f, 0, -0.4f);
    [SerializeField] private GameObject globalFireEffectObject = null, GlobalBurningEffectObject = null;
    [SerializeField] private GameObject globalElectricEffectObject = null;

    private void Awake()
    {
        GlobalSettings.SpriteBillboardVector = globalSpriteTransformOrientation;
        GlobalSettings.SpriteOffsetVector = globalSpriteTransformOffset;
        GlobalSettings.FireEffectObject = globalFireEffectObject;
        GlobalSettings.BurningEffectObject = GlobalBurningEffectObject;
        GlobalSettings.ElectricEffectObject = globalElectricEffectObject;
    }
}

public static class GlobalSettings
{
    public static Vector3 SpriteBillboardVector;
    public static Vector3 SpriteOffsetVector;
    public static GameObject FireEffectObject;
    public static GameObject BurningEffectObject;
    public static GameObject ElectricEffectObject;
}
