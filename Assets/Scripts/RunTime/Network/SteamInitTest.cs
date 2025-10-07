using UnityEngine;
using Steamworks;

public class SteamInitTest : MonoBehaviour
{
    private void Awake()
    {
        try
        {
            // Spacewar AppID (테스트용)
            SteamClient.Init(480, true);
            Debug.Log($"[Steam] Initialized. Username: {SteamClient.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Steam] Init failed: {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        SteamClient.Shutdown();
    }
}
