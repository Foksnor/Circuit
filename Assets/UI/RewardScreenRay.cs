using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class RewardScreenRay : MonoBehaviour
{
    private float rotateSpeed = 0;
    private const float alphaSpeed = 0.5f;
    private Image rayImage;
    int rngOffest = 0;

    private void Awake()
    {
        rayImage = GetComponent<Image>();
        SetDefaultRNGValues();
    }

    public void SetRotationSpeed(float speed)
    {
        rotateSpeed = speed;
    }


    private void SetDefaultRNGValues()
    {
        // Rotation
        rngOffest = Random.Range(0, 360);
        transform.eulerAngles = new Vector3(0, 0, rngOffest);
    }

    private void Update()
    {
        // Rotation
        transform.eulerAngles += new Vector3(0, 0, rotateSpeed * Time.deltaTime);

        // Alpha
        float time = Time.time + rngOffest;
        float rayImageAlpha = Mathf.PingPong(time * alphaSpeed, 1) / 5;
        rayImage.color = new Color(rayImage.color.r, rayImage.color.g, rayImage.color.b, rayImageAlpha);
    }
}
