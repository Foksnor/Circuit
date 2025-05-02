using UnityEngine;

public class RewardScreenRayPopulator : MonoBehaviour
{
    [SerializeField] private RewardScreenRay targetRayVisual;
    [SerializeField] private int rayCount = 50;
    [SerializeField] private int rayRotateSpeed = 5;
    [SerializeField] private float rayRotateDeviation = 0.33f;
    [SerializeField] private float raySizeDeviation = 0.5f;

    public void PopulateRays()
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
        }
    }

    public void RemoveRays()
    {
        foreach (Transform ray in transform)
        {
            Destroy(ray.gameObject);
        }
    }
}
