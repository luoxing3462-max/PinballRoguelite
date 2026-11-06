using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PinballRoguelite.Editor
{
    [InitializeOnLoad]
    public static class PlayModeStartupScene
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";

        static PlayModeStartupScene()
        {
            // Configure immediately so a user can press the normal Play button as
            // soon as Unity finishes compiling this assembly. The delayed pass is
            // kept as a fallback for an asset database refresh during project import.
            ConfigureStartScene();
            EditorApplication.delayCall += ConfigureStartScene;
        }

        private static void ConfigureStartScene()
        {
            if (Application.isBatchMode) return;

            SceneAsset mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
            if (mainScene == null)
            {
                Debug.LogError($"PinballRoguelite could not configure the Play Mode start scene because {MainScenePath} is missing.");
                return;
            }

            EditorSceneManager.playModeStartScene = mainScene;
        }
    }
}
