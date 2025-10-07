using UnityEngine;
using Steamworks;

public class SteamBooter : MonoBehaviour
{
    [Tooltip("테스트용 Spacewar AppID(480). 실제 출시 때는 우리 게임의 AppID로 변경해야 합니다.")]
    public uint appId = 480;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        try
        {
            SteamClient.Init(appId, true);
            Debug.Log($"[Steam] Init OK - {SteamClient.Name} ({SteamClient.SteamId})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Steam] Init failed: {e.Message}");
        }
    }

    private void Update()
    {
        // Facepunch.Steamworks는 매 프레임 콜백 펌프가 필요
        // 이 함수가 있어야 Steam SDK 내부 큐에 쌓인 콜백들이 실행됩니다! (초대, 방생성 등등 ...)
        if (SteamClient.IsValid)
            SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        if (SteamClient.IsValid)
            SteamClient.Shutdown();
    }
}
