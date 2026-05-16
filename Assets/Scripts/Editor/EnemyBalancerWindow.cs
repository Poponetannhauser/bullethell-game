#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BulletHell.Data;
using System.Collections.Generic;

namespace BulletHell.Editor
{
    public class GameBalancerWindow : EditorWindow
    {
        private List<EnemyDataSO> enemyDataList = new List<EnemyDataSO>();
        private List<WeaponDataSO> weaponDataList = new List<WeaponDataSO>();
        private Vector2 scrollPosition;

        [MenuItem("BulletHell/Game Balancer Tool")]
        public static void ShowWindow()
        {
            GameBalancerWindow window = GetWindow<GameBalancerWindow>("Game Balancer");
            window.minSize = new Vector2(800, 400);
            window.LoadAllData();
        }

        private void OnEnable()
        {
            LoadAllData();
        }

        private void LoadAllData()
        {
            // Load Enemy Data
            enemyDataList.Clear();
            string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO");
            foreach (string guid in enemyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyDataSO data = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (data != null) enemyDataList.Add(data);
            }

            // Load Weapon Data
            weaponDataList.Clear();
            string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponDataSO");
            foreach (string guid in weaponGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                WeaponDataSO data = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(path);
                if (data != null) weaponDataList.Add(data);
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Game Balancer — Master Spreadsheet", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Edit all game statistics in one place. Changes are applied instantly.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Refresh All Data", GUILayout.Height(30)))
            {
                LoadAllData();
            }

            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Weapon / Player Stats", EditorStyles.whiteLargeLabel);
            
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Asset", GUILayout.Width(130));
            EditorGUILayout.LabelField("Damage", GUILayout.Width(60));
            EditorGUILayout.LabelField("Fire Rate", GUILayout.Width(70));
            EditorGUILayout.LabelField("Heat/Shot", GUILayout.Width(70));
            EditorGUILayout.LabelField("Cool Rate", GUILayout.Width(70));
            EditorGUILayout.LabelField("OH Cooldown", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            foreach (WeaponDataSO data in weaponDataList)
            {
                if (data == null) continue;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                if (GUILayout.Button(data.name, EditorStyles.linkLabel, GUILayout.Width(130)))
                {
                    Selection.activeObject = data;
                    EditorGUIUtility.PingObject(data);
                }

                EditorGUI.BeginChangeCheck();
                float newDamage = EditorGUILayout.FloatField(data.damage, GUILayout.Width(60));
                float newFireRate = EditorGUILayout.FloatField(data.fireRate, GUILayout.Width(70));
                float newHeat = EditorGUILayout.FloatField(data.heatPerShot, GUILayout.Width(70));
                float newCool = EditorGUILayout.FloatField(data.coolDownRate, GUILayout.Width(70));
                float newOHDur = EditorGUILayout.FloatField(data.overheatCooldownDuration, GUILayout.Width(80));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(data, $"Edit {data.name}");
                    data.damage = newDamage;
                    data.fireRate = newFireRate;
                    data.heatPerShot = newHeat;
                    data.coolDownRate = newCool;
                    data.overheatCooldownDuration = newOHDur;
                    EditorUtility.SetDirty(data);
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Enemy Stats", EditorStyles.whiteLargeLabel);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Asset", GUILayout.Width(130));
            EditorGUILayout.LabelField("Max HP", GUILayout.Width(60));
            EditorGUILayout.LabelField("Speed", GUILayout.Width(60));
            EditorGUILayout.LabelField("Fire Rate", GUILayout.Width(70));
            EditorGUILayout.LabelField("Score", GUILayout.Width(60));
            EditorGUILayout.LabelField("Hit Flash", GUILayout.Width(70));
            EditorGUILayout.LabelField("Shatter", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            foreach (EnemyDataSO data in enemyDataList)
            {
                if (data == null) continue;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                if (GUILayout.Button(data.name, EditorStyles.linkLabel, GUILayout.Width(130)))
                {
                    Selection.activeObject = data;
                    EditorGUIUtility.PingObject(data);
                }

                EditorGUI.BeginChangeCheck();
                float newHealth = EditorGUILayout.FloatField(data.maxHealth, GUILayout.Width(60));
                float newSpeed = EditorGUILayout.FloatField(data.moveSpeed, GUILayout.Width(60));
                float newFireRate = EditorGUILayout.FloatField(data.fireRate, GUILayout.Width(70));
                int newScore = EditorGUILayout.IntField(data.scoreValue, GUILayout.Width(60));
                Color newHitColor = EditorGUILayout.ColorField(data.hitFlashColor, GUILayout.Width(70));
                Color newShatterColor = EditorGUILayout.ColorField(data.shatterColor, GUILayout.Width(70));

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
                Debug.Log("All game balance data saved successfully.");
            }
            GUILayout.Space(5);
        }
    }
}
#endif
