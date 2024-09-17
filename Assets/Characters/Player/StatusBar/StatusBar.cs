using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    [SerializeField] private GameObject Fire, Shock;

    private void Awake()
    {
        // Disable status icons by default
        ToggleStatusIcon(_StatusType.Fire, false);
        ToggleStatusIcon(_StatusType.Shocked, false);
    }

    public void ToggleStatusIcon(_StatusType status, bool isActive)
    {
        switch (status)
        {
            default:
            case _StatusType.None:
                break;
            case _StatusType.Fire:
                Fire.SetActive(isActive);
                break;
            case _StatusType.Shocked:
                Shock.SetActive(isActive);
                break;
        }
    }
}
