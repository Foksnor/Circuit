using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformToBillboard : MonoBehaviour
{
    [SerializeField] Transform[] Transform = null;

    void Start()
    {
        for (int i = 0; i < Transform.Length; i++)
        {
            Transform[i].eulerAngles = GlobalSettings.SpriteBillboardVector;
            Transform[i].position += GlobalSettings.SpriteOffsetVector;
        }
    }
}