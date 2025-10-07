using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Threading.Tasks;

public class BootSceneLoader : MonoBehaviour
{
#if UNITY_EDITOR
    const string KeyPrevScene = "SceneBootstrapper_PreviousScene";
    const string KeyIsBooting = "SceneBootstrapper_IsBooting";
    [SerializeField] string FirstScene = "LobbyScene";
#endif

    async void Start()
    {
#if UNITY_EDITOR
        var activeScene = SceneManager.GetActiveScene();
        string prevScene = EditorPrefs.GetString(KeyPrevScene, string.Empty);

        // prevScene이 유효하고, 현재(=BootScene)와 다를 때만 전환
        if (!string.IsNullOrEmpty(prevScene) && prevScene != activeScene.path)
        {
            var loadOp = SceneManager.LoadSceneAsync(prevScene, LoadSceneMode.Additive);
            while (!loadOp.isDone) await Task.Yield();

            var unloadOp = SceneManager.UnloadSceneAsync(activeScene);
            while (!unloadOp.isDone) await Task.Yield();
        }
#endif
        await SceneManager.LoadSceneAsync(FirstScene);
    }
}
