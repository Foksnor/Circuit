using UnityEngine;

public class RewardScreenRayVisuals : MonoBehaviour
{
    [SerializeField] private RewardScreenRay targetRayVisual;
    const int rayCount = 50;
    const int rayRotateSpeed = 10;
    const float rayRotateDeviation = 0.33f;
    const float raySizeDeviation = 0.5f;

    private void Awake()
    {
        for (int i = 0; i < rayCount; i++)
        {
            RewardScreenRay ray = Instantiate(targetRayVisual, transform.position, transform.rotation, transform);

            // Apply speed with deviation
            float speedFactor = rayRotateSpeed * rayRotateDeviation;
            float rngSpeedDeviation = Random.Range(-speedFactor, speedFactor);
            ray.SetRotationSpeed(rayRotateSpeed + rngSpeedDeviation);

            // Apply size with deviation
            float sizeFactor = ray.transform.localScale.y * raySizeDeviation;
            float rngSizeDeviation = Random.Range(-sizeFactor, sizeFactor);
            ray.transform.localScale += new Vector3(0, rngSizeDeviation, 0);

            int rngAngle = Random.Range(0, 360);
            ray.SetDefaultRotation(rngAngle);
        }
    }
}
