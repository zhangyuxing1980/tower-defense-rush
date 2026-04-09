// PROTOTYPE - NOT FOR PRODUCTION
// 2D Scene Setup Tool - Configures the scene for 2D gameplay
// Date: 2026-04-08

using UnityEngine;
using UnityEditor;

namespace TowerDefenseRush.Editor
{
    public class Setup2DScene : EditorWindow
    {
        [MenuItem("Tower Defense Rush/Setup 2D Scene")]
        public static void ShowWindow()
        {
            GetWindow<Setup2DScene>("2D Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("2D Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Convert to 2D", GUILayout.Height(40)))
            {
                ConvertTo2D();
            }

            GUILayout.Space(10);
            GUILayout.Label("This will:", EditorStyles.label);
            GUILayout.Label("- Set Main Camera to Orthographic", EditorStyles.label);
            GUILayout.Label("- Create 2D sprites for all game objects", EditorStyles.label);
            GUILayout.Label("- Position everything on XY plane", EditorStyles.label);
        }

        static void ConvertTo2D()
        {
            // Setup Main Camera for 2D
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 10;
                camera.transform.position = new Vector3(0, 0, -10);
                camera.transform.rotation = Quaternion.identity;
                Debug.Log("✓ Camera set to Orthographic");
            }

            // Create 2D sprites for existing objects
            CreateSpriteForObject("Player", Color.blue, new Vector3(0.8f, 0.8f, 1));
            CreateSpriteForObject("Jimmy_FlameFox", new Color(1f, 0.5f, 0f), new Vector3(0.6f, 0.6f, 1));
            CreateSpriteForObject("Jimmy_BoarKing", new Color(0.6f, 0.4f, 0.2f), new Vector3(0.7f, 0.7f, 1));
            CreateSpriteForObject("Jimmy_RockGolem", new Color(0.5f, 0.5f, 0.5f), new Vector3(0.9f, 0.9f, 1));
            CreateSpriteForObject("TownCenter", Color.cyan, new Vector3(2, 2, 1));

            // Create enemy prefab (red square)
            var enemyPrefab = CreateEnemyPrefab2D();

            // Setup WaveManager with enemy prefab
            var waveManager = GameObject.Find("WaveManager");
            if (waveManager != null)
            {
                var wm = waveManager.GetComponent<TowerDefenseRush.Prototype.WaveManager>();
                if (wm != null)
                {
                    // Setup wave configs
                    wm.waves = new TowerDefenseRush.Prototype.WaveConfig[3];
                    for (int i = 0; i < 3; i++)
                    {
                        wm.waves[i] = new TowerDefenseRush.Prototype.WaveConfig
                        {
                            enemyCount = 5 + i * 2,
                            spawnInterval = 1.5f,
                            timeBetweenWaves = 5f,
                            enemyPrefab = enemyPrefab
                        };
                    }
                }
            }

            // Add VisualTestRunner
            var testRunner = GameObject.Find("TestRunner");
            if (testRunner != null)
            {
                if (testRunner.GetComponent<TowerDefenseRush.Testing.VisualTestRunner>() == null)
                {
                    testRunner.AddComponent<TowerDefenseRush.Testing.VisualTestRunner>();
                }
            }

            Debug.Log("✓ 2D Scene Setup Complete!");
            EditorUtility.DisplayDialog("Success", "Scene converted to 2D!\n\nNow you can:\n- Click Play to run tests\n- See visual feedback in Game view", "OK");
        }

        static void CreateSpriteForObject(string objectName, Color color, Vector3 scale)
        {
            var obj = GameObject.Find(objectName);
            if (obj == null) return;

            // Check if already has visual
            var visual = obj.transform.Find("Visual");
            if (visual != null) return;

            // Create visual child
            var visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(obj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = scale;

            // Add SpriteRenderer
            var sr = visualObj.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (sr.sprite == null)
            {
                // Try to create a simple sprite
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = new Vector2(1, 1);
            }

            // Set sorting layer
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            Debug.Log($"✓ Added 2D visual to {objectName}");
        }

        static GameObject CreateEnemyPrefab2D()
        {
            var enemy = new GameObject("EnemyPrefab");
            enemy.tag = "Enemy";

            // Add Rigidbody2D
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;

            // Add Enemy script
            var enemyComp = enemy.AddComponent<TowerDefenseRush.Prototype.Enemy>();

            // Add visual
            var visual = new GameObject("Visual");
            visual.transform.SetParent(enemy.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.5f, 0.5f, 1);

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.color = Color.red;
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (sr.sprite == null)
            {
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = new Vector2(1, 1);
            }

            enemyComp.visualTransform = visual.transform;

            // Add health bar
            var healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(enemy.transform);
            healthBar.transform.localPosition = new Vector3(0, 0.4f, 0);
            healthBar.transform.localScale = new Vector3(0.5f, 0.1f, 1);

            var healthSr = healthBar.AddComponent<SpriteRenderer>();
            healthSr.color = Color.green;
            healthSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (healthSr.sprite == null)
            {
                healthSr.drawMode = SpriteDrawMode.Sliced;
                healthSr.size = new Vector2(1, 1);
            }
            enemyComp.healthBar = healthSr;

            // Save as prefab
            string prefabPath = "Assets/Prefabs/Enemy2D.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
            DestroyImmediate(enemy);

            Debug.Log("✓ Created Enemy2D prefab");
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
    }
}
