#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BulletHell.Data;
using System.Collections.Generic;

namespace BulletHell.Editor
{
    public class EnemyBalancerWindow : EditorWindow
    {
        private List<EnemyDataSO> enemyDataList = new List<EnemyDataSO>();
        private Vector2 scrollPosition;

        [MenuItem("BulletHell/Enemy Balancer Tool")]
        public static void ShowWindow()
        {
            EnemyBalancerWindow window = GetWindow<EnemyBalancerWindow>("Enemy Balancer");
            window.minSize = new Vector2(750, 300);
            window.LoadAllEnemyData();
        }

        private void OnEnable()
        {
            LoadAllEnemyData();
        }

        private void LoadAllEnemyData()
        {
            enemyDataList.Clear();
            string[] guids = AssetDatabase.FindAssets("t:EnemyDataSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyDataSO data = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (data != null)
                {
                    enemyDataList.Add(data);
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Enemy Balancer — Master Spreadsheet", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Edit all EnemyDataSO assets in one place. Changes are applied instantly.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Refresh Asset Data", GUILayout.Height(30)))
            {
                LoadAllEnemyData();
            }

            GUILayout.Space(10);

            if (enemyDataList.Count == 0)
            {
                EditorGUILayout.HelpBox("No EnemyDataSO assets found in this project.", MessageType.Warning);
                return;
            }

            // Table Header — matches EnemyDataSO fields exactly
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Asset", GUILayout.Width(130));
            EditorGUILayout.LabelField("Max HP", GUILayout.Width(70));
            EditorGUILayout.LabelField("Speed", GUILayout.Width(70));
            EditorGUILayout.LabelField("Fire Rate", GUILayout.Width(80));
            EditorGUILayout.LabelField("Score", GUILayout.Width(70));
            EditorGUILayout.LabelField("Hit Flash", GUILayout.Width(80));
            EditorGUILayout.LabelField("Shatter", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (EnemyDataSO data in enemyDataList)
            {
                if (data == null) continue;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // Asset name — click to ping in Project window
                if (GUILayout.Button(data.name, EditorStyles.linkLabel, GUILayout.Width(130)))
                {
                    Selection.activeObject = data;
                    EditorGUIUtility.PingObject(data);
                }

                EditorGUI.BeginChangeCheck();

                // Stats
                float newHealth = EditorGUILayout.FloatField(data.maxHealth, GUILayout.Width(70));
                float newSpeed = EditorGUILayout.FloatField(data.moveSpeed, GUILayout.Width(70));
                float newFireRate = EditorGUILayout.FloatField(data.fireRate, GUILayout.Width(80));
                int newScore = EditorGUILayout.IntField(data.scoreValue, GUILayout.Width(70));
                
                // Visuals
                Color newHitColor = EditorGUILayout.ColorField(data.hitFlashColor, GUILayout.Width(80));
                Color newShatterColor = EditorGUILayout.ColorField(data.shatterColor, GUILayout.Width(80));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(data, $"Edit {data.name}");
                    data.maxHealth = newHealth;
                    data.moveSpeed = newSpeed;
                    data.fireRate = newFireRate;
                    data.scoreValue = newScore;
                    data.hitFlashColor = newHitColor;
                    data.shatterColor = newShatterColor;
                    EditorUtility.SetDirty(data);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Save All Changes to Disk", GUILayout.Height(35)))
            {
                AssetDatabase.SaveAssets();
                Debug.Log("All enemy balance data saved successfully.");
            }
            GUILayout.Space(5);
        }
    }
}
#endif
