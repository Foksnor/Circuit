using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.Reactor;
using UnityEngine;
using UnityEngine.UI;

public class DelayProgressbar : MonoBehaviour
{
    [SerializeField]
    private Progressor progressor;
    private float progressorValueLastFrame;
    [SerializeField]
    private Image progressbarImage;
    [SerializeField]
    private float delayTime, speed;
    private float timer;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            if (progressbarImage.fillAmount < progressor.currentValue)
                progressbarImage.fillAmount += Time.deltaTime * speed;
        }

        // If progressor value changed since last frame
        if (progressorValueLastFrame != progressor.currentValue)
        {
            timer = delayTime;
            progressorValueLastFrame = progressor.currentValue;
        }
    }
}
