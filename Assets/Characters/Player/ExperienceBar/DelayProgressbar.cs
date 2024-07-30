using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.Reactor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DelayProgressbar : MonoBehaviour
{
    [SerializeField]
    private Progressor progressor;
    private float progressorValueLastFrame, progressbarImageValueBeforeLerp;
    [SerializeField]
    private Image progressbarImage;
    [SerializeField]
    private float delayTime, speed;
    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        // If progressor value changed since last frame
        if (progressorValueLastFrame != progressor.currentValue)
        {
            timer = -delayTime;
            progressbarImageValueBeforeLerp = progressbarImage.fillAmount;
            progressorValueLastFrame = progressor.currentValue;
        }

        if (timer >= 0 &&
            progressbarImage.fillAmount != progressor.currentValue)
            progressbarImage.fillAmount = Mathf.Lerp(progressbarImageValueBeforeLerp, progressor.currentValue, timer * speed);

        // Reset the delayed fill when the progressor value is either lower or has been reset
        if (progressor.currentValue == 0)
            progressbarImage.fillAmount = 0;
    }
}
