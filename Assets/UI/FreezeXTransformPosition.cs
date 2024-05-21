using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeXTransformPosition : MonoBehaviour
{
    private float FrozenPositionX;

    private void Start()
    {
        FrozenPositionX = transform.position.x;
    }

    void Update()
    {
        transform.position = new Vector3(FrozenPositionX, transform.position.y, transform.position.z);
    }
}
