using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperiencePoint : MonoBehaviour
{
    [SerializeField]
    private Vector2 spawnSpreadDistance = new Vector2();
    private Vector3 spawnSpreadTargetLocation = new Vector3();
    [SerializeField]
    private float moveSpeed = 1.0f;
    private float startTime;
    private bool isMovingToEXPBar = false;

    private void Awake()
    {
        Invoke(nameof(MoveToExperienceBar), 1);
        float randomDist = Random.Range(spawnSpreadDistance.x, spawnSpreadDistance.y);
        Vector3 randomAngle = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        spawnSpreadTargetLocation = transform.position + (randomDist * randomAngle);
        startTime = Time.time;
    }

    private void Update()
    {
        float curTime = Time.time - startTime;
        if (!isMovingToEXPBar)
            transform.position = Vector3.Lerp(transform.position, spawnSpreadTargetLocation, curTime * moveSpeed);
        else
        {
            float curExpFillWidth = Screen.width / PlayerStats.ExperienceBar.curExpFill;
            // QQQ TODO: Currently x and y are swapped because the camera is at an 90 degree angle for PC build
            Vector3 xpBarPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.height, curExpFillWidth, 0));
            transform.position = Vector3.Lerp(transform.position, xpBarPos, curTime * moveSpeed);
        }
    }

    private void MoveToExperienceBar()
    {
        PlayerStats.ExperienceBar.AddExperiencePoints(1);
        isMovingToEXPBar = true;
        startTime = Time.time;
    }
}
