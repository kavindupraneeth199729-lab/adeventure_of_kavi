using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    private static EnemySpawner _instance;
    public static EnemySpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("EnemySpawner");
                _instance = go.AddComponent<EnemySpawner>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnAdditionalEnemies()
    {
        Debug.Log("EnemySpawner: [NUCLEAR SPAWN] Starting on 'platform_0 (2)'...");

        GameObject groundObj = GameObject.Find("platform_0 (2)");
        if (groundObj == null)
        {
            Debug.LogError("EnemySpawner: platform_0 (2) not found!");
            return;
        }

        Collider2D groundCol = groundObj.GetComponent<Collider2D>();
        if (groundCol == null) return;

        float minX = groundCol.bounds.min.x + 5f;
        float maxX = groundCol.bounds.max.x - 5f;
        float groundY = groundCol.bounds.max.y;

        // 1. Find EXACT Templates
        MushroomEnemy mTemplate = null;
        SlimeEnemy sTemplate = null;

        MushroomEnemy[] allM = GameObject.FindObjectsByType<MushroomEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var m in allM) if (m.name == "Mushroom" || m.name == "MushroomEnemy") mTemplate = m;

        SlimeEnemy[] allS = GameObject.FindObjectsByType<SlimeEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var s in allS) if (s.name == "Slime" || s.name == "SlimeEnemy") sTemplate = s;

        // 2. Perform Identical Spawning
        if (mTemplate != null)
        {
            for (int i = 0; i < 4; i++) SpawnGeneric(mTemplate.gameObject, "Mushroom", i + 1, minX, maxX, groundY);
        }
        else Debug.LogWarning("EnemySpawner: Mushroom template missing!");

        if (sTemplate != null)
        {
            for (int i = 0; i < 3; i++) SpawnGeneric(sTemplate.gameObject, "Slime", i + 1, minX, maxX, groundY);
        }
        else Debug.LogWarning("EnemySpawner: Slime template missing!");

        // --- NEW: Test Slime next to player ---
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && sTemplate != null)
        {
            Vector3 pPos = playerObj.transform.position;
            Debug.Log($"EnemySpawner: Template Slime Active={sTemplate.gameObject.activeSelf}, ScriptEnabled={sTemplate.enabled}");
            // Spawn EXACTLY at player's Z depth to test visibility issues
            SpawnGeneric(sTemplate.gameObject, "Test_Slime", 0, pPos.x + 2f, pPos.x + 2f, groundY);
            // Override Z for test to be sure
            GameObject testObj = GameObject.Find("Test_Slime_0");
            if (testObj != null) 
            {
                Vector3 tPos = testObj.transform.position;
                tPos.z = 0;
                testObj.transform.position = tPos;
            }
        }
    }

    private void SpawnGeneric(GameObject template, string typeName, int index, float minX, float maxX, float groundY)
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(randomX, groundY + 0.5f, -1.0f);
        
        // IDENTICAL Logic for all extras
        GameObject clone = Instantiate(template, spawnPos, Quaternion.identity, template.transform.parent);
        clone.name = $"{typeName}_Extra_{index}";
        clone.transform.localScale = template.transform.localScale;
        
        // --- PREVENT ACCIDENTAL KILLER SCRIPTS ---
        // If HealthUI is accidentally on the template, it will kill the clone. Remove it!
        HealthUI sui = clone.GetComponent<HealthUI>();
        if (sui != null) Destroy(sui); 

        clone.SetActive(true);

        // Notify scripts
        clone.SendMessage("ResetEnemy", SendMessageOptions.DontRequireReceiver);
        clone.SendMessage("SetPatrolCenter", randomX, SendMessageOptions.DontRequireReceiver);

        Debug.Log($"EnemySpawner: [SPAWN] {clone.name} at {spawnPos}. Hierarchy: {(clone.transform.parent != null ? clone.transform.parent.name : "ROOT")}, Scale: {clone.transform.localScale}");
    }
}
