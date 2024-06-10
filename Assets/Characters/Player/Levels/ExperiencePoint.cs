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
        float curExpFill = PlayerStats.ExperienceBar.curExpFill;
        if (curExpFill >= 0)
            curExpFill *= Screen.width;

        Vector3 xpBarPos = Camera.main.ScreenToWorldPoint(new Vector3(curExpFill, Screen.height, 0));
        transform.position = Vector3.Lerp(transform.position, xpBarPos, curTime * moveSpeed);

        // Add XP to player once it reaches the XP bar
        float dist = Vector3.Distance(transform.position, xpBarPos);
        if (dist <= 0.5f)
        {
            PlayerStats.ExperienceBar.AddExperiencePoints(1);
            Instantiate(particleWhenHittingXPBar, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
