using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum _SurfaceType { None, Water, Oil, Burning, Electrified };
public enum _StatusType { None, Fire, Shocked };

public class GameSettings : MonoBehaviour 
{
    [SerializeField] private Vector3 globalSpriteTransformOrientation = new (0, -55, 90);
    [SerializeField] private Vector3 globalSpriteTransformOffset = new (0.4f, 0, -0.4f);
    [SerializeField] private GameObject globalFireEffectObject = null;
    [SerializeField] private GameObject globalBurningEffectObject = null;
    [SerializeField] private GameObject globalShockEffectObject = null;
    [SerializeField] private GameObject globalElectrifiedEffectObject = null;
    [SerializeField] private StatusEffect_ScriptableObject globalFireStatus = null;
    [SerializeField] private StatusEffect_ScriptableObject globalShockStatus = null;

    private void Awake()
    {
        GlobalSettings.SpriteBillboardVector = globalSpriteTransformOrientation;
        GlobalSettings.SpriteOffsetVector = globalSpriteTransformOffset;
        GlobalSettings.FireEffectObject = globalFireEffectObject;
        GlobalSettings.BurningEffectObject = globalBurningEffectObject;
        GlobalSettings.ShockEffectObject = globalShockEffectObject;
        GlobalSettings.ElectrifiedEffectObject = globalElectrifiedEffectObject;
        GlobalSettings.FireStatus = globalFireStatus;
        GlobalSettings.ShockStatus = globalShockStatus;
    }
}

public static class GlobalSettings
{
    public static Vector3 SpriteBillboardVector;
    public static Vector3 SpriteOffsetVector;
    public static GameObject FireEffectObject;
    public static GameObject BurningEffectObject;
    public static GameObject ShockEffectObject;
    public static GameObject ElectrifiedEffectObject;
    public static StatusEffect_ScriptableObject FireStatus;
    public static StatusEffect_ScriptableObject ShockStatus;
}
