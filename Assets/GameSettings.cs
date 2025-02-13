using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public enum _SurfaceType { None, Water, Oil, Burning, Electrified };
public enum _StatusType { None, Fire, Shocked };

public class GameSettings : MonoBehaviour 
{
    [SerializeField] private Vector3 globalSpriteTransformOrientation = new (0, -55, 90);
    [SerializeField] private Vector3 globalSpriteTransformOffset = new (0.4f, 0, -0.4f);
    [SerializeField] private GameObject fireHit = null;
    [SerializeField] private GameObject fireChain = null;
    [SerializeField] private GameObject burningSurface = null;
    [SerializeField] private GameObject dousedSurface = null;
    [SerializeField] private GameObject shockHit = null;
    [SerializeField] private GameObject shockChain = null;
    [SerializeField] private StatusEffect_ScriptableObject fireStatus = null;
    [SerializeField] private StatusEffect_ScriptableObject shockStatus = null;
    [SerializeField] private Color c_RarityRare = Color.blue;
    [SerializeField] private Color c_RarityEpic = Color.yellow;

    private void Awake()
    {
        GlobalSettings.SpriteBillboardVector = globalSpriteTransformOrientation;
        GlobalSettings.SpriteOffsetVector = globalSpriteTransformOffset;
        GlobalSettings.FireHit = fireHit;
        GlobalSettings.FireChain = fireChain;
        GlobalSettings.BurningSurface = burningSurface;
        GlobalSettings.DousedSurface = dousedSurface;
        GlobalSettings.ShockHit = shockHit;
        GlobalSettings.ShockChain = shockChain;
        GlobalSettings.FireStatus = fireStatus;
        GlobalSettings.ShockStatus = shockStatus;
        GlobalSettings.C_RarityRare = c_RarityRare;
        GlobalSettings.C_RarityEpic = c_RarityEpic;
    }
}

public static class GlobalSettings
{
    public static Vector3 SpriteBillboardVector;
    public static Vector3 SpriteOffsetVector;
    public static GameObject FireHit;
    public static GameObject FireChain;
    public static GameObject BurningSurface;
    public static GameObject DousedSurface;
    public static GameObject ShockHit;
    public static GameObject ShockChain;
    public static StatusEffect_ScriptableObject FireStatus;
    public static StatusEffect_ScriptableObject ShockStatus;
    public static Color C_RarityRare;
    public static Color C_RarityEpic;
}
