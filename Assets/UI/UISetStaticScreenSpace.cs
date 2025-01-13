using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISetStaticScreenSpace : MonoBehaviour
{
    private void Awake()
    {
        PlayerUI.CanvasScreenSpace = GetComponent<Canvas>();
    }
}
