using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObjectLifetime : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 1f;

    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
            Destroy(gameObject);
    }
}
