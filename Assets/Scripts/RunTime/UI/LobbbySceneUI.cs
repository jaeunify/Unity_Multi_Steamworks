using TMPro;
using UnityEngine;

public class LobbySceneUI : MonoBehaviour
{
    [SerializeField] TMP_InputField InputLobbyIdField;

    public async void CreateRoomAsync()
    {
        await SteamLobbyController.SingletonInstance.CreateLobbyAsync();
    }

    public async void JoinRoomAsync()
    {
        var lobbyId = InputLobbyIdField.text;
        await SteamLobbyController.SingletonInstance.JoinLobbyAsync(lobbyId);
    }
}