using System.Collections.Generic;
using System.Linq;
using Template.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Template.BetterInputHandling.Editor
{
    /// <summary>
    /// Creates the template-specific BetterInputHandling sample assets and scene wiring.
    /// </summary>
    public static class BetterInputHandlingSetup
    {
        private const string InputActionsPath = "Assets/StarterAssets/InputSystem/StarterAssets.inputactions";
        private const string SettingsPath = "Assets/Scripts/BetterInputHandling/BetterInputSettings.asset";
        private const string KeyboardGlyphSetPath = "Assets/Scripts/BetterInputHandling/KeyboardMouseGlyphSet.asset";
        private const string XboxGlyphSetPath = "Assets/Scripts/BetterInputHandling/XboxGlyphSet.asset";
        private const string PlayStationGlyphSetPath = "Assets/Scripts/BetterInputHandling/PlayStationGlyphSet.asset";
        private const string GenericGlyphSetPath = "Assets/Scripts/BetterInputHandling/GenericGamepadGlyphSet.asset";
        private const string ServicePrefabFolder = "Assets/Prefabs/BetterInputHandling";
        private const string ServicePrefabPath = ServicePrefabFolder + "/BetterInputService.prefab";
        private const string BootstrapConfigPath = "Assets/Resources/Config/BootstrapConfig.asset";
        private const string PlayerPrefabPath = "Assets/Prefabs/PlayerContainer.prefab";
        private const string UIServicePrefabPath = "Assets/Prefabs/UI/UIService.prefab";
        private const string FlatScenePath = "Assets/Scenes/FlatScene.unity";
        private const string KeyboardGlyphSheetPath = "Assets/Graphics/UI Sprites/Keyboard Mouse Glyphs/keyboard buttons.png";
        private const string ControllerGlyphFolder = "Assets/Graphics/UI Sprites/Controller Glyphs";

        [MenuItem("Tools/Better Input Handling/Apply Template Setup")]
        public static void ApplyTemplateSetup()
        {
            EnsureFolder("Assets/Prefabs", "BetterInputHandling");
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            var keyboardGlyphSet = CreateKeyboardGlyphSet();
            var xboxGlyphSet = CreateXboxGlyphSet();
            var playStationGlyphSet = CreatePlayStationGlyphSet();
            var genericGlyphSet = CreateGenericGlyphSet(xboxGlyphSet);
            var settings = CreateSettings(inputActions, keyboardGlyphSet, xboxGlyphSet, playStationGlyphSet, genericGlyphSet);
            var servicePrefab = CreateServicePrefab(settings);

            AddServiceToBootstrap(servicePrefab);
            AddPromptDetectorToPlayerPrefab();
            RepairUIInputModule(inputActions);

            CreateFlatScenePromptTestObjects();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static BetterInputGlyphSet CreateKeyboardGlyphSet()
        {
            var entries = new List<BetterInputGlyphEntry>
            {
                new BetterInputGlyphEntry("escape", LoadKeyboardSprite("esc"), "Esc"),
                new BetterInputGlyphEntry("e", LoadKeyboardSprite("e"), "E"),
                new BetterInputGlyphEntry("q", LoadKeyboardSprite("q"), "Q"),
                new BetterInputGlyphEntry("spacebar", LoadKeyboardSprite("spacebar"), "Space"),
                new BetterInputGlyphEntry("leftShift", LoadKeyboardSprite("shift"), "Shift"),
                new BetterInputGlyphEntry("enter", LoadKeyboardSprite("enter"), "Enter"),
                new BetterInputGlyphEntry("leftButton", null, "LMB"),
            };

            return CreateGlyphSet(KeyboardGlyphSetPath, BetterInputDeviceKind.KeyboardMouse, "Keyboard & Mouse", entries);
        }

        private static BetterInputGlyphSet CreateXboxGlyphSet()
        {
            var entries = new List<BetterInputGlyphEntry>
            {
                new BetterInputGlyphEntry("start", LoadControllerSprite("xbox-start-16.png"), "Start"),
                new BetterInputGlyphEntry("select", LoadControllerSprite("xbox-select-16.png"), "Select"),
                new BetterInputGlyphEntry("buttonSouth", LoadControllerSprite("xbox-A-16.png"), "A"),
                new BetterInputGlyphEntry("buttonEast", LoadControllerSprite("xbox-B-16.png"), "B"),
                new BetterInputGlyphEntry("buttonWest", LoadControllerSprite("xbox-X-16.png"), "X"),
                new BetterInputGlyphEntry("buttonNorth", LoadControllerSprite("xbox-Y-16.png"), "Y"),
                new BetterInputGlyphEntry("leftShoulder", LoadControllerSprite("xbox-LB-16.png"), "LB"),
                new BetterInputGlyphEntry("rightShoulder", LoadControllerSprite("xbox-RB-16.png"), "RB"),
                new BetterInputGlyphEntry("leftTrigger", LoadControllerSprite("xbox-LT-16.png"), "LT"),
                new BetterInputGlyphEntry("rightTrigger", LoadControllerSprite("xbox-RT-16.png"), "RT"),
            };

            return CreateGlyphSet(XboxGlyphSetPath, BetterInputDeviceKind.XboxGamepad, "Xbox Controller", entries);
        }

        private static BetterInputGlyphSet CreatePlayStationGlyphSet()
        {
            var entries = new List<BetterInputGlyphEntry>
            {
                new BetterInputGlyphEntry("start", LoadControllerSprite("ps-start-16.png"), "Options"),
                new BetterInputGlyphEntry("select", LoadControllerSprite("ps-select-16.png"), "Share"),
                new BetterInputGlyphEntry("buttonSouth", LoadControllerSprite("ps-cross-16.png"), "Cross"),
                new BetterInputGlyphEntry("buttonEast", LoadControllerSprite("ps-circle-16.png"), "Circle"),
                new BetterInputGlyphEntry("buttonWest", LoadControllerSprite("ps-square-16.png"), "Square"),
                new BetterInputGlyphEntry("buttonNorth", LoadControllerSprite("ps-triangle-16.png"), "Triangle"),
                new BetterInputGlyphEntry("leftShoulder", LoadControllerSprite("ps-L1-16.png"), "L1"),
                new BetterInputGlyphEntry("rightShoulder", LoadControllerSprite("ps-R1-16.png"), "R1"),
                new BetterInputGlyphEntry("leftTrigger", LoadControllerSprite("ps-L2-16.png"), "L2"),
                new BetterInputGlyphEntry("rightTrigger", LoadControllerSprite("ps-R2-16.png"), "R2"),
            };

            return CreateGlyphSet(PlayStationGlyphSetPath, BetterInputDeviceKind.PlayStationGamepad, "PlayStation Controller", entries);
        }

        private static BetterInputGlyphSet CreateGenericGlyphSet(BetterInputGlyphSet xboxGlyphSet)
        {
            var entries = new List<BetterInputGlyphEntry>
            {
                new BetterInputGlyphEntry("start", LoadControllerSprite("xbox-start-16.png"), "Start"),
                new BetterInputGlyphEntry("buttonSouth", LoadControllerSprite("xbox-A-16.png"), "South"),
                new BetterInputGlyphEntry("buttonEast", LoadControllerSprite("xbox-B-16.png"), "East"),
                new BetterInputGlyphEntry("buttonWest", LoadControllerSprite("xbox-X-16.png"), "West"),
                new BetterInputGlyphEntry("buttonNorth", LoadControllerSprite("xbox-Y-16.png"), "North"),
                new BetterInputGlyphEntry("leftShoulder", LoadControllerSprite("xbox-LB-16.png"), "L1"),
                new BetterInputGlyphEntry("rightShoulder", LoadControllerSprite("xbox-RB-16.png"), "R1"),
                new BetterInputGlyphEntry("leftTrigger", LoadControllerSprite("xbox-LT-16.png"), "L2"),
                new BetterInputGlyphEntry("rightTrigger", LoadControllerSprite("xbox-RT-16.png"), "R2"),
            };

            return CreateGlyphSet(GenericGlyphSetPath, BetterInputDeviceKind.GenericGamepad, "Generic Gamepad", entries);
        }

        private static BetterInputSettings CreateSettings(
            InputActionAsset inputActions,
            BetterInputGlyphSet keyboardGlyphSet,
            BetterInputGlyphSet xboxGlyphSet,
            BetterInputGlyphSet playStationGlyphSet,
            BetterInputGlyphSet genericGlyphSet)
        {
            var settings = AssetDatabase.LoadAssetAtPath<BetterInputSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<BetterInputSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            settings.Configure(
                inputActions,
                keyboardGlyphSet,
                xboxGlyphSet,
                playStationGlyphSet,
                genericGlyphSet,
                new[]
                {
                    new BetterInputRemappableAction("Pause", BetterInputActionReference.Pause),
                    new BetterInputRemappableAction("Interact", BetterInputActionReference.Interact),
                    new BetterInputRemappableAction("Jump", new BetterInputActionReference("Player", "Jump")),
                    new BetterInputRemappableAction("Sprint", new BetterInputActionReference("Player", "Sprint")),
                    new BetterInputRemappableAction("Emote Up", new BetterInputActionReference("Player", "EmoteUp")),
                    new BetterInputRemappableAction("Emote Down", new BetterInputActionReference("Player", "EmoteDown")),
                    new BetterInputRemappableAction("Emote Left", new BetterInputActionReference("Player", "EmoteLeft")),
                    new BetterInputRemappableAction("Emote Right", new BetterInputActionReference("Player", "EmoteRight")),
                });

            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static GameObject CreateServicePrefab(BetterInputSettings settings)
        {
            var root = new GameObject("BetterInputService");
            var service = root.AddComponent<BetterInputService>();
            root.AddComponent<BetterInputHudInstaller>();

            var serviceObject = new SerializedObject(service);
            serviceObject.FindProperty("settings").objectReferenceValue = settings;
            serviceObject.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ServicePrefabPath);
            Object.DestroyImmediate(root);
            return prefab != null ? prefab : AssetDatabase.LoadAssetAtPath<GameObject>(ServicePrefabPath);
        }

        private static void AddServiceToBootstrap(GameObject servicePrefab)
        {
            var config = AssetDatabase.LoadAssetAtPath<BootstrapConfig>(BootstrapConfigPath);
            if (config == null || servicePrefab == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(config);
            var services = serializedObject.FindProperty("persistentServices");

            for (var index = 0; index < services.arraySize; index++)
            {
                var existingPrefab = services.GetArrayElementAtIndex(index).FindPropertyRelative("prefab").objectReferenceValue;
                if (existingPrefab == servicePrefab)
                {
                    return;
                }
            }

            services.arraySize++;
            services.GetArrayElementAtIndex(services.arraySize - 1).FindPropertyRelative("prefab").objectReferenceValue = servicePrefab;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void AddPromptDetectorToPlayerPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (root == null)
            {
                return;
            }

            if (root.GetComponentInChildren<BetterInputPromptDetector>(true) == null)
            {
                root.AddComponent<BetterInputPromptDetector>();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void RepairUIInputModule(InputActionAsset inputActions)
        {
            if (inputActions == null)
            {
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(UIServicePrefabPath);
            if (root == null)
            {
                return;
            }

            var uiModule = root.GetComponentInChildren<InputSystemUIInputModule>(true);
            if (uiModule != null)
            {
                uiModule.actionsAsset = inputActions;
                uiModule.point = FindActionReference(inputActions, "UI", "Point");
                uiModule.move = FindActionReference(inputActions, "UI", "Navigate");
                uiModule.submit = FindActionReference(inputActions, "UI", "Submit");
                uiModule.cancel = FindActionReference(inputActions, "UI", "Cancel");
                uiModule.leftClick = FindActionReference(inputActions, "UI", "Click");
                uiModule.middleClick = FindActionReference(inputActions, "UI", "MiddleClick");
                uiModule.rightClick = FindActionReference(inputActions, "UI", "RightClick");
                uiModule.scrollWheel = FindActionReference(inputActions, "UI", "ScrollWheel");
                uiModule.trackedDevicePosition = FindActionReference(inputActions, "UI", "TrackedDevicePosition");
                uiModule.trackedDeviceOrientation = FindActionReference(inputActions, "UI", "TrackedDeviceOrientation");
                EditorUtility.SetDirty(uiModule);
                PrefabUtility.SaveAsPrefabAsset(root, UIServicePrefabPath);
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void CreateFlatScenePromptTestObjects()
        {
            var scene = EditorSceneManager.OpenScene(FlatScenePath, OpenSceneMode.Single);
            CreatePromptTestObject("BetterInput_TakePromptTest", PrimitiveType.Cube, new Vector3(2f, 1f, 2f), "Take", true);
            CreatePromptTestObject("BetterInput_TalkPromptTest", PrimitiveType.Capsule, new Vector3(4f, 1f, 2f), "Talk", false);
            CreatePromptTestObject("BetterInput_OpenPromptTest", PrimitiveType.Cube, new Vector3(6f, 1f, 2f), "Open", false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreatePromptTestObject(string objectName, PrimitiveType primitiveType, Vector3 position, string promptText, bool hideOnExecute)
        {
            var existing = GameObject.Find(objectName);
            var target = existing != null ? existing : GameObject.CreatePrimitive(primitiveType);
            target.name = objectName;
            target.transform.position = position;
            target.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            var source = target.GetComponent<BetterInputPromptSource>();
            if (source == null)
            {
                source = target.AddComponent<BetterInputPromptSource>();
            }

            var serializedObject = new SerializedObject(source);
            serializedObject.FindProperty("actionReference").FindPropertyRelative("actionMapName").stringValue = "Player";
            serializedObject.FindProperty("actionReference").FindPropertyRelative("actionName").stringValue = "Interact";
            serializedObject.FindProperty("promptText").stringValue = promptText;
            serializedObject.FindProperty("priority").intValue = 0;
            serializedObject.FindProperty("hideObjectWhenExecuted").boolValue = hideOnExecute;
            serializedObject.FindProperty("logExecution").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static BetterInputGlyphSet CreateGlyphSet(string path, BetterInputDeviceKind kind, string displayName, IEnumerable<BetterInputGlyphEntry> entries)
        {
            var glyphSet = AssetDatabase.LoadAssetAtPath<BetterInputGlyphSet>(path);
            if (glyphSet == null)
            {
                glyphSet = ScriptableObject.CreateInstance<BetterInputGlyphSet>();
                AssetDatabase.CreateAsset(glyphSet, path);
            }

            glyphSet.Configure(kind, displayName, entries);
            EditorUtility.SetDirty(glyphSet);
            return glyphSet;
        }

        private static Sprite LoadKeyboardSprite(string spriteName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(KeyboardGlyphSheetPath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == spriteName);
        }

        private static Sprite LoadControllerSprite(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{ControllerGlyphFolder}/{fileName}");
        }

        private static InputActionReference FindActionReference(InputActionAsset inputActions, string mapName, string actionName)
        {
            var action = inputActions.FindAction($"{mapName}/{actionName}", false);
            if (action == null)
            {
                return null;
            }

            var assetPath = AssetDatabase.GetAssetPath(inputActions);
            var importedReference = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<InputActionReference>()
                .FirstOrDefault(reference => reference.action != null && reference.action.id == action.id);

            return importedReference != null ? importedReference : InputActionReference.Create(action);
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
