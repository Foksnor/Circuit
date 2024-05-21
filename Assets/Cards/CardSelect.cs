using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardSelect
{
    private static CardSocket selectedSocket = null;

    public static CardSocket SelectedSocket
    {
        get => selectedSocket;
        set => selectedSocket = value;
    }
}
