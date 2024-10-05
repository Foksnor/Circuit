using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyAfterSceneLoad : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // This method will be called after the scene is fully loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnNewScene();
    }

    public virtual void OnNewScene()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
