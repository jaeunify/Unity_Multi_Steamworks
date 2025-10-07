using UnityEngine;

public class LobbySceneUI : MonoBehaviour
{
    public async void CreateRoomAsync()
    {
        await SteamLobbyController.SingletonInstance.CreateLobbyAsync();
    }
}