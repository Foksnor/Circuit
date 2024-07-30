using Doozy.Runtime.Reactor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Progressor progressor;
    [SerializeField] private CanvasGroup canvasGroup;
    private float curHealthFill = 1f;

    private void Start()
    {
        progressor.SetValueAt(curHealthFill);
    }

    public void UpdateHealthBar(int maxHealth, int curHealth, int damageReceived)
    {
        // Make healthbar visible as soon as the character gets damaged
        canvasGroup.alpha = 1;

        // Update healthbar to display health values (from 0 to 1)
        curHealthFill = (float)curHealth / (float)maxHealth;
        progressor.SetValueAt(curHealthFill);
    }
}
