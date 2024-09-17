using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void SubtractHealth(int amount, Character instigator);
    public void SetStatus(_StatusType status);
}
