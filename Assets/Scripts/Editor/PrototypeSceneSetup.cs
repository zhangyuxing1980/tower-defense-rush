// PROTOTYPE - NOT FOR PRODUCTION
// Scene Setup Tool - Automatically configures the prototype scene
// Date: 2026-04-07

using UnityEngine;
using UnityEditor;

namespace TowerDefenseRush.Editor
{
    public class PrototypeSceneSetup : EditorWindow
    {
        [MenuItem("Tower Defense Rush/Setup Prototype Scene")]
        public static void ShowWindow()
        {
            GetWindow<PrototypeSceneSetup>("Scene Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Prototype Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Setup Complete Scene", GUILayout.Height(40)))
            {
                SetupScene();
            }

            GUILayout.Space(10);
            GUILayout.Label("This will create:", EditorStyles.label);
            GUILayout.Label("- Player (Lord)", EditorStyles.label);
            GUILayout.Label("- 3 Jimmies (FlameFox, BoarKing, RockGolem)", EditorStyles.label);
            GUILayout.Label("- WaveManager", EditorStyles.label);
            GUILayout.Label("- TestRunner", EditorStyles.label);
            GUILayout.Label("- Town Center", EditorStyles.label);
            GUILayout.Label("- Enemy Spawn Points", EditorStyles.label);
        }

        static void SetupScene()
        {
            // Create Town Center
            var townCenter = CreateGameObject("TownCenter", new Vector3(0, -5, 0));
            var townSprite = townCenter.AddComponent<SpriteRenderer>();
            townSprite.color = Color.cyan;
            townSprite.drawMode = SpriteDrawMode.Sliced;
            townSprite.size = new Vector2(2, 2);

            // Create Player (Lord)
            var player = CreateGameObject("Player", new Vector3(0, 0, 0));
            player.tag = "Player";
            var playerRb = player.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0;
            playerRb.freezeRotation = true;
            var playerController = player.AddComponent<Prototype.PlayerController>();

            // Player visual
            var playerVisual = CreateGameObject("Visual", Vector3.zero, player.transform);
            var playerSr = playerVisual.AddComponent<SpriteRenderer>();
            playerSr.color = Color.blue;
            playerSr.drawMode = SpriteDrawMode.Sliced;
            playerSr.size = new Vector2(0.8f, 0.8f);
            playerController.visualTransform = playerVisual.transform;

            // Create 3 Jimmies
            var jimmy1 = CreateJimmy("Jimmy_FlameFox", new Vector3(-2, 1, 0), Prototype.JimmyType.FlameFox, player.transform);
            var jimmy2 = CreateJimmy("Jimmy_BoarKing", new Vector3(2, 1, 0), Prototype.JimmyType.BoarKing, player.transform);
            var jimmy3 = CreateJimmy("Jimmy_RockGolem", new Vector3(0, 2, 0), Prototype.JimmyType.RockGolem, player.transform);

            // Create Enemy Prefab
            var enemyPrefab = CreateEnemyPrefab();

            // Create WaveManager
            var waveManager = CreateGameObject("WaveManager", Vector3.zero);
            var wm = waveManager.AddComponent<Prototype.WaveManager>();
            wm.townCenter = townCenter.transform;

            // Create spawn points
            var spawn1 = CreateGameObject("SpawnPoint_1", new Vector3(-8, 5, 0));
            var spawn2 = CreateGameObject("SpawnPoint_2", new Vector3(8, 5, 0));
            var spawn3 = CreateGameObject("SpawnPoint_3", new Vector3(-8, -3, 0));
            var spawn4 = CreateGameObject("SpawnPoint_4", new Vector3(8, -3, 0));
            wm.spawnPoints = new Transform[] { spawn1.transform, spawn2.transform, spawn3.transform, spawn4.transform };

            // Setup wave configs
            wm.waves = new Prototype.WaveConfig[3];
            for (int i = 0; i < 3; i++)
            {
                wm.waves[i] = new Prototype.WaveConfig
                {
                    enemyCount = 5 + i * 2,
                    spawnInterval = 1.5f,
                    timeBetweenWaves = 5f,
                    enemyPrefab = enemyPrefab
                };
            }

            // Create TestRunner
            var testRunner = CreateGameObject("TestRunner", Vector3.zero);
            testRunner.AddComponent<Testing.TestRunner>();

            // Add test components
            testRunner.AddComponent<Testing.CombatSystemTest>();
            testRunner.AddComponent<Testing.JimmyAITest>();
            testRunner.AddComponent<Testing.WaveSystemTest>();
            testRunner.AddComponent<Testing.SynergySystemTest>();
            testRunner.AddComponent<Testing.PerformanceTest>();

            // Setup Camera
            var camera = GameObject.Find("Main Camera");
            if (camera != null)
            {
                camera.transform.position = new Vector3(0, 0, -10);
                var cam = camera.GetComponent<Camera>();
                cam.orthographicSize = 10;
            }

            // Save prefab
            string prefabPath = "Assets/Prefabs/Enemy.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(enemyPrefab, prefabPath);
            DestroyImmediate(enemyPrefab);

            // Update wave manager with prefab
            for (int i = 0; i < wm.waves.Length; i++)
            {
                wm.waves[i].enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            Debug.Log("Prototype scene setup complete!");
            EditorUtility.DisplayDialog("Success", "Prototype scene has been set up!\n\nObjects created:\n- Player (Lord)\n- 3 Jimmies\n- WaveManager\n- TestRunner\n- Enemy Prefab", "OK");
        }

        static GameObject CreateGameObject(string name, Vector3 position, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            if (parent != null) go.transform.SetParent(parent);
            return go;
        }

        static GameObject CreateJimmy(string name, Vector3 position, Prototype.JimmyType type, Transform lord)
        {
            var jimmy = CreateGameObject(name, position);
            jimmy.tag = "Jimmy";
            var rb = jimmy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;

            var jimmyComp = jimmy.AddComponent<Prototype.Jimmy>();
            jimmyComp.jimmyType = type;
            jimmyComp.lord = lord;

            // Visual
            var visual = CreateGameObject("Visual", Vector3.zero, jimmy.transform);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(0.6f, 0.6f);
            jimmyComp.visualTransform = visual.transform;

            // Set color based on type
            switch (type)
            {
                case Prototype.JimmyType.FlameFox:
                    sr.color = new Color(1f, 0.5f, 0f);
                    break;
                case Prototype.JimmyType.BoarKing:
                    sr.color = new Color(0.6f, 0.4f, 0.2f);
                    break;
                case Prototype.JimmyType.RockGolem:
                    sr.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
            }

            return jimmy;
        }

        static GameObject CreateEnemyPrefab()
        {
            var enemy = CreateGameObject("EnemyPrefab", Vector3.zero);
            enemy.tag = "Enemy";
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;

            var enemyComp = enemy.AddComponent<Prototype.Enemy>();

            // Visual
            var visual = CreateGameObject("Visual", Vector3.zero, enemy.transform);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.color = Color.red;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(0.5f, 0.5f);
            enemyComp.visualTransform = visual.transform;

            // Health bar
            var healthBar = CreateGameObject("HealthBar", new Vector3(0, 0.4f, 0), enemy.transform);
            var healthSr = healthBar.AddComponent<SpriteRenderer>();
            healthSr.color = Color.green;
            healthSr.drawMode = SpriteDrawMode.Sliced;
            healthSr.size = new Vector2(0.5f, 0.1f);
            enemyComp.healthBar = healthSr;

            return enemy;
        }
    }
}
