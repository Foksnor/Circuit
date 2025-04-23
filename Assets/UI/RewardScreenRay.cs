using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class RewardScreenRay : MonoBehaviour
{
    private float rotateSpeed = 0;
    private Image rayImage;

    private void Awake()
    {
        rayImage = GetComponent<Image>();
    }

    public void SetRotationSpeed(float speed)
    {
        rotateSpeed = speed;
    }

    public void SetDefaultRotation(int angle)
    {
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private void Update()
    {
        transform.eulerAngles += new Vector3(0, 0, rotateSpeed * Time.deltaTime);

        float rayImageAlpha = Mathf.PingPong(Time.time, 1) / 5;
        rayImage.color = new Color(rayImage.color.r, rayImage.color.g, rayImage.color.b, rayImageAlpha);
    }
}
