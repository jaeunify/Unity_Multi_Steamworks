using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BootSceneLoader : MonoBehaviour
{
#if UNITY_EDITOR
    const string KeyPrevScene = "SceneBootstrapper_PreviousScene";
    const string KeyIsBooting = "SceneBootstrapper_IsBooting";
#endif

    async void Start()
    {
#if UNITY_EDITOR
        // 현재 실행 중인 씬 (BootScene)
        var activeScene = SceneManager.GetActiveScene();

        // 에디터Prefs에서 이전 씬 경로 읽기
        string prevScene = EditorPrefs.GetString(KeyPrevScene, string.Empty);

        // BootScene이 아닐 때는 아무 것도 안 함 (루프 방지)
        if (string.IsNullOrEmpty(prevScene) || prevScene == activeScene.path)
            return;

        // Additive로 원래 씬 로드
        var loadOp = SceneManager.LoadSceneAsync(prevScene, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            await System.Threading.Tasks.Task.Yield();

        // BootScene 언로드 (현재 씬만 제거)
        var unloadOp = SceneManager.UnloadSceneAsync(activeScene);
        while (!unloadOp.isDone)
            await System.Threading.Tasks.Task.Yield();

        // 부팅 상태 플래그 해제 (다음 실행 때 재작동 가능)
        EditorPrefs.SetBool(KeyIsBooting, false);
#endif
    }
}
