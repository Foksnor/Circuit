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
    private float curTime;
    private bool isMovingToEXPBar = false;
    private float timeBeforeMovingToXPBar = 1;

    [SerializeField]
    private GameObject particleWhenHittingXPBar;

    private void Awake()
    {
        float randomDist = Random.Range(spawnSpreadDistance.x, spawnSpreadDistance.y);
        Vector3 randomAngle = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        spawnSpreadTargetLocation = transform.position + (randomDist * randomAngle);
        startTime = Time.time;
        // Random offset to movespeed
        moveSpeed *= Random.Range(0.9f, 1.1f);
    }

    private void Update()
    {
        curTime = Time.time - startTime;

        // XP moves to XP bar
        if (timeBeforeMovingToXPBar <= 0)
        {
            if (isMovingToEXPBar)
                MoveToExperienceBar();
            else
            {
                // Reset startTime for lerp position calculation when moving to XP bar
                isMovingToEXPBar = true;
                startTime = Time.time;
            }
        }
        else
        {
            timeBeforeMovingToXPBar -= Time.deltaTime;
            // XP spread when spawned
            transform.position = Vector3.Lerp(transform.position, spawnSpreadTargetLocation, curTime * moveSpeed);
        }
    }

    private void MoveToExperienceBar()
    {
        // Get the screen position of the UI element
        Vector3 screenPosition = PlayerStats.ExperienceBar.GetProgressBarRectTransform().position;

        // Adjusts the screenPosition to the amount of current experience of the bar
        float curExpFill = PlayerStats.ExperienceBar.curExpFill;
        screenPosition.x *= curExpFill * 2; // the 'times 2' is because the pivot of the rect transform is half the screen size

        // Calculate the Z depth based on the world object's position relative to the camera
        float worldObjectZDepth = Mathf.Abs(Camera.main.transform.position.z - spawnSpreadTargetLocation.z);

        // Convert the screen position to world position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, worldObjectZDepth));

        // Move the world object to the calculated world position
        transform.position = Vector3.Lerp(transform.position, worldPosition, curTime * moveSpeed);

        // Add XP to player once it reaches the XP bar
        float dist = Vector3.Distance(transform.position, worldPosition);
        if (dist <= 0.05f)
        {
            PlayerStats.ExperienceBar.AddExperiencePoints(1);
            Instantiate(particleWhenHittingXPBar, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
