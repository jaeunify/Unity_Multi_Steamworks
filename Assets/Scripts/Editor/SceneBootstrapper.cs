using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneBootstrapper
{
    const string KeyPrevScene = "SceneBootstrapper_PreviousScene";
    const string KeyIsBooting = "SceneBootstrapper_IsBooting";

    static string BootstrapScene => EditorBuildSettings.scenes.Length > 0
        ? EditorBuildSettings.scenes[0].path
        : string.Empty;

    static SceneBootstrapper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 플레이 "진입" 시: BootScene을 잠시 띄워 DontDestroyObject들 생성
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (string.IsNullOrEmpty(BootstrapScene)) return;
            if (EditorPrefs.GetBool(KeyIsBooting, false)) return;

            var active = EditorSceneManager.GetActiveScene();
            var currentPath = active.path;
            if (string.IsNullOrEmpty(currentPath)) return;
            if (BootstrapScene == currentPath) return;

            // 작업중이던 씬 저장
            EditorPrefs.SetString(KeyPrevScene, currentPath);
            EditorPrefs.SetBool(KeyIsBooting, true);

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.isPlaying = false;
                return;
            }

            EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene(BootstrapScene);
            EditorApplication.isPlaying = true;
            return;
        }

        // 플레이 "종료" 후: 원래 씬으로 복귀 (BootScene으로 안 돌아가게)
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (!EditorPrefs.GetBool(KeyIsBooting, false)) return;

            var prev = EditorPrefs.GetString(KeyPrevScene, string.Empty);
            if (!string.IsNullOrEmpty(prev) && prev != BootstrapScene)
            {
                EditorSceneManager.OpenScene(prev);
            }

            EditorPrefs.DeleteKey(KeyPrevScene);
            EditorPrefs.DeleteKey(KeyIsBooting);
        }
    }
}
