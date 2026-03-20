using System.Collections.Generic;
using System.Linq;
using Template.Achievements;
using Template.Achievements.Conditions;
using Template.Bootstrap;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Template.Maze.Editor
{
    public static class MazeFeatureAssetGenerator
    {
        private const string MazeSceneName = "MazeScene";
        private const string MazeScenePath = "Assets/Scenes/MazeScene.unity";
        private const string FlatScenePath = "Assets/Scenes/FlatScene.unity";
        private const string BootstrapConfigPath = "Assets/Resources/Config/BootstrapConfig.asset";
        private const string MazeConfigPath = "Assets/Resources/Config/MazeConfig.asset";
        private const string MazeManagerPrefabPath = "Assets/Prefabs/MazeManager.prefab";
        private const string MazeSaveServicePrefabPath = "Assets/Prefabs/MazeSaveService.prefab";
        private const string DoorwayPrefabPath = "Assets/Prefabs/Architecture/Doorway.prefab";
        private const string CollectablePrefabPath = "Assets/Prefabs/Collectable.prefab";
        private const string MusicConfigPrefabPath = "Assets/Prefabs/MusicConfig.prefab";
        private const string PauseScreenPrefabPath = "Assets/Prefabs/UI/PauseScreen.prefab";
        private const string GoldHudPrefabPath = "Assets/Prefabs/UI/GoldDisplayHud.prefab";
        private const string PlayerManagerPrefabPath = "Assets/Prefabs/PlayerManager.prefab";
        private const string SmallSceneMusicPath = "Assets/Audio/Free_Sketchbook_Tallbeard.ogg";
        private const string FloorMaterialPath = "Assets/StarterAssets/Environment/Art/Materials/GridBlue_01_Mat.mat";
        private const string WallMaterialPath = "Assets/StarterAssets/Environment/Art/Materials/GridWhite_01_Mat.mat";
        private const string MedalSpritePath = "Assets/Graphics/UI Sprites/medal 1 256 px.png";
        private const string SolveMazeConditionPath = "Assets/Resources/Achievements/Conditions/1MazeSolved.asset";
        private const string SolveMazeAchievementPath = "Assets/Resources/Achievements/Solve 1 Maze.asset";
        private const string SolveMazeAchievementId = "0401";

        private static readonly Vector3 FlatSceneDoorwayPosition = new Vector3(9.8f, 2.02f, -10.6f);
        private static readonly Quaternion FlatSceneDoorwayRotation = Quaternion.identity;
        private static readonly Vector3 FlatSceneReturnPosition = new Vector3(9.8f, 0f, -7.6f);

        [MenuItem("Tools/Maze/Generate Feature Assets")]
        public static void GenerateOrUpdate()
        {
            var originalScenePath = SceneManager.GetActiveScene().path;

            EnsureFolders();

            var mazeConfig = EnsureMazeConfig();
            var mazeManagerPrefab = EnsureMazeManagerPrefab(mazeConfig);
            var mazeSaveServicePrefab = EnsureMazeSaveServicePrefab();

            EnsureMazeAchievementAssets();
            EnsureMazeScene();
            UpdateFlatSceneDoorway(mazeConfig);
            UpdateBootstrapConfig(mazeManagerPrefab, mazeSaveServicePrefab);
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!string.IsNullOrWhiteSpace(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            Debug.Log("Maze feature assets generated.");
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets/Resources/Config");
            CreateFolderIfMissing("Assets/Resources/Achievements");
            CreateFolderIfMissing("Assets/Resources/Achievements/Conditions");
            CreateFolderIfMissing("Assets/Prefabs");
            CreateFolderIfMissing("Assets/Scenes");
        }

        private static MazeConfig EnsureMazeConfig()
        {
            var mazeConfig = AssetDatabase.LoadAssetAtPath<MazeConfig>(MazeConfigPath);
            if (mazeConfig == null)
            {
                mazeConfig = ScriptableObject.CreateInstance<MazeConfig>();
                AssetDatabase.CreateAsset(mazeConfig, MazeConfigPath);
            }

            var serializedObject = new SerializedObject(mazeConfig);
            serializedObject.FindProperty("algorithmKind").enumValueIndex = (int)MazeAlgorithmKind.RecursiveBacktracker;
            serializedObject.FindProperty("width").intValue = 10;
            serializedObject.FindProperty("height").intValue = 10;
            serializedObject.FindProperty("cellSize").floatValue = 6f;
            serializedObject.FindProperty("floorThickness").floatValue = 0.25f;
            serializedObject.FindProperty("wallThickness").floatValue = 0.45f;
            serializedObject.FindProperty("wallHeight").floatValue = 4f;
            serializedObject.FindProperty("mazeOrigin").vector3Value = Vector3.zero;
            serializedObject.FindProperty("useSeedOverride").boolValue = false;
            serializedObject.FindProperty("seedOverride").intValue = 12345;
            serializedObject.FindProperty("minimumCoinCount").intValue = 4;
            serializedObject.FindProperty("maximumCoinCount").intValue = 9;
            serializedObject.FindProperty("coinHeight").floatValue = 1f;
            serializedObject.FindProperty("collectablePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(CollectablePrefabPath);
            serializedObject.FindProperty("doorwayPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(DoorwayPrefabPath);
            serializedObject.FindProperty("playerSpawnOffset").vector3Value = new Vector3(0f, 1f, 0f);
            serializedObject.FindProperty("flatSceneReturnPosition").vector3Value = FlatSceneReturnPosition;
            serializedObject.FindProperty("doorwayCenterHeight").floatValue = 2f;
            serializedObject.FindProperty("exitDoorwayPositionOffset").vector3Value = Vector3.zero;
            serializedObject.FindProperty("exitDoorwayScale").vector3Value = Vector3.one;
            serializedObject.FindProperty("floorMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>(FloorMaterialPath);
            serializedObject.FindProperty("wallMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>(WallMaterialPath);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(mazeConfig);
            return mazeConfig;
        }

        private static GameObject EnsureMazeManagerPrefab(MazeConfig mazeConfig)
        {
            var root = new GameObject("MazeManager");
            var manager = root.AddComponent<MazeManager>();

            var serializedObject = new SerializedObject(manager);
            serializedObject.FindProperty("config").objectReferenceValue = mazeConfig;
            serializedObject.FindProperty("returnSceneName").stringValue = "FlatScene";
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, MazeManagerPrefabPath);
            Object.DestroyImmediate(root);

            return prefab;
        }

        private static GameObject EnsureMazeSaveServicePrefab()
        {
            var root = new GameObject("MazeSaveService");
            var saveService = root.AddComponent<MazeSaveService>();

            var serializedObject = new SerializedObject(saveService);
            serializedObject.FindProperty("mazeFileName").stringValue = "maze_save.json";
            serializedObject.FindProperty("mazeSchemaVersion").intValue = 1;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, MazeSaveServicePrefabPath);
            Object.DestroyImmediate(root);

            return prefab;
        }

        private static void EnsureMazeAchievementAssets()
        {
            var solveCondition = AssetDatabase.LoadAssetAtPath<TotalMazesSolvedAtLeastCondition>(SolveMazeConditionPath);
            if (solveCondition == null)
            {
                solveCondition = ScriptableObject.CreateInstance<TotalMazesSolvedAtLeastCondition>();
                AssetDatabase.CreateAsset(solveCondition, SolveMazeConditionPath);
            }

            var conditionObject = new SerializedObject(solveCondition);
            conditionObject.FindProperty("requiredSolvedMazeCount").intValue = 1;
            conditionObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(solveCondition);

            var achievement = AssetDatabase.LoadAssetAtPath<AchievementDefinition>(SolveMazeAchievementPath);
            if (achievement == null)
            {
                achievement = ScriptableObject.CreateInstance<AchievementDefinition>();
                AssetDatabase.CreateAsset(achievement, SolveMazeAchievementPath);
            }

            var achievementObject = new SerializedObject(achievement);
            achievementObject.FindProperty("id").stringValue = SolveMazeAchievementId;
            achievementObject.FindProperty("displayName").stringValue = "Maze Runner";
            achievementObject.FindProperty("description").stringValue = "Solve your first maze.";
            achievementObject.FindProperty("flavorText").stringValue = "Every wall is just a delayed answer.";
            achievementObject.FindProperty("icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(MedalSpritePath);
            achievementObject.FindProperty("hideUntilUnlocked").boolValue = true;
            achievementObject.FindProperty("displayOrder").intValue = 4;
            achievementObject.FindProperty("unlockCondition").objectReferenceValue = solveCondition;
            achievementObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(achievement);
        }

        private static void EnsureMazeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var musicPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MusicConfigPrefabPath);
            var musicObject = PrefabUtility.InstantiatePrefab(musicPrefab) as GameObject;
            if (musicObject != null)
            {
                musicObject.name = "MusicConfig";
                var musicConfig = musicObject.GetComponent<MusicConfig>();
                var musicSerializedObject = new SerializedObject(musicConfig);
                musicSerializedObject.FindProperty("mode").enumValueIndex = (int)MusicConfig.MusicMode.Override;
                musicSerializedObject.FindProperty("musicClip").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(SmallSceneMusicPath);
                musicSerializedObject.FindProperty("loop").boolValue = true;
                musicSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.SaveScene(scene, MazeScenePath);
        }

        private static void UpdateFlatSceneDoorway(MazeConfig mazeConfig)
        {
            var scene = EditorSceneManager.OpenScene(FlatScenePath, OpenSceneMode.Single);
            var doorway = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Maze Scene Doorway");
            if (doorway == null)
            {
                var doorwayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DoorwayPrefabPath);
                doorway = PrefabUtility.InstantiatePrefab(doorwayPrefab) as GameObject;
                doorway.name = "Maze Scene Doorway";
            }

            doorway.transform.SetPositionAndRotation(FlatSceneDoorwayPosition, FlatSceneDoorwayRotation);

            var doorwayText = doorway.GetComponentInChildren<TMP_Text>(true);
            if (doorwayText != null)
            {
                doorwayText.text = "Maze Scene";
                doorwayText.fontSize = 8.85f;
            }

            var transition = doorway.GetComponentInChildren<LevelTransition>(true);
            if (transition != null)
            {
                transition.ConfigureDestination(MazeSceneName, mazeConfig.GetMazeEntrySpawnPosition());
            }

            EditorSceneManager.SaveScene(scene);
        }

        private static void UpdateBootstrapConfig(GameObject mazeManagerPrefab, GameObject mazeSaveServicePrefab)
        {
            var bootstrapConfig = AssetDatabase.LoadAssetAtPath<BootstrapConfig>(BootstrapConfigPath);
            var persistentServices = bootstrapConfig.persistentServices != null
                ? bootstrapConfig.persistentServices.ToList()
                : new List<ServicePrefabEntry>();

            if (!persistentServices.Any(entry => entry != null && entry.prefab == mazeSaveServicePrefab))
            {
                persistentServices.Add(new ServicePrefabEntry
                {
                    prefab = mazeSaveServicePrefab
                });
            }

            bootstrapConfig.persistentServices = persistentServices.ToArray();

            var sceneProfiles = bootstrapConfig.sceneProfiles != null
                ? bootstrapConfig.sceneProfiles.Where(profile => profile != null).ToList()
                : new List<SceneProfile>();
            var mazeProfile = sceneProfiles.FirstOrDefault(profile => profile.sceneName == MazeSceneName);
            if (mazeProfile == null)
            {
                mazeProfile = new SceneProfile
                {
                    sceneName = MazeSceneName
                };
                sceneProfiles.Add(mazeProfile);
            }

            mazeProfile.perSceneManagers = new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>(PauseScreenPrefabPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(GoldHudPrefabPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(PlayerManagerPrefabPath),
                mazeManagerPrefab
            };

            bootstrapConfig.sceneProfiles = sceneProfiles.ToArray();
            EditorUtility.SetDirty(bootstrapConfig);
        }

        private static void UpdateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(scene => scene.path == MazeScenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(MazeScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void CreateFolderIfMissing(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var segments = folderPath.Split('/');
            var currentPath = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var nextPath = $"{currentPath}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[index]);
                }

                currentPath = nextPath;
            }
        }
    }
}
