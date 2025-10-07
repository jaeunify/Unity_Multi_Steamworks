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
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        // 이미 부트 중이면 무시 (무한 루프 방지)
        if (EditorPrefs.GetBool(KeyIsBooting, false))
            return;

        var active = EditorSceneManager.GetActiveScene();
        var currentPath = active.path;
        if (string.IsNullOrEmpty(currentPath))
            return;

        // 현재 씬이 BootScene이면 스킵
        if (BootstrapScene == currentPath)
            return;

        // 현재 씬 경로 저장
        EditorPrefs.SetString(KeyPrevScene, currentPath);

        // 이번 실행이 "부트"임을 표시
        EditorPrefs.SetBool(KeyIsBooting, true);

        // BootScene으로 교체
        EditorApplication.isPlaying = false;
        EditorSceneManager.OpenScene(BootstrapScene);
        EditorApplication.isPlaying = true;
    }
}
