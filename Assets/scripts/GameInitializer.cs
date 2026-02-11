using UnityEngine;

[DefaultExecutionOrder(-100)] // Execute before other scripts
public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("GameInitializer: Starting initialization");
        
        // Create PauseMenuManager if it doesn't exist
        if (PauseMenuManager.Instance == null)
        {
            GameObject pauseMenuObj = new GameObject("PauseMenuManager");
            pauseMenuObj.AddComponent<PauseMenuManager>();
            Debug.Log("GameInitializer: Created PauseMenuManager");
        }
        
        // Create ControlEditor if it doesn't exist
        if (ControlEditor.Instance == null)
        {
            GameObject controlEditorObj = new GameObject("ControlEditor");
            controlEditorObj.AddComponent<ControlEditor>();
            Debug.Log("GameInitializer: Created ControlEditor");
        }
        
        // Create GameOverManager if it doesn't exist
        if (GameOverManager.Instance == null)
        {
            GameObject gameOverObj = new GameObject("GameOverManager");
            gameOverObj.AddComponent<GameOverManager>();
            Debug.Log("GameInitializer: Created GameOverManager");
        }
        
        Debug.Log("GameInitializer: Initialization complete");
    }

    void Start()
    {
        // Give the original enemies a moment to initialize their positions
        Invoke("DelayedEnemySpawning", 0.5f);
    }

    void DelayedEnemySpawning()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SpawnAdditionalEnemies();
        }
    }
}
