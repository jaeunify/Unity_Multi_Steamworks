using System.Threading.Tasks;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class SteamLobbyController : MonoBehaviour
{
    public static SteamLobbyController SingletonInstance;

    [Header("Lobby Settings")]
    [Range(2, 16)] public int maxMembers = 8;
    public string lobbyName = "WobbleWitches Test Lobby";
    public bool isPublic = true;

    private Lobby? _currentLobby;

    void Awake()
    {
        if (SingletonInstance != null && SingletonInstance != this)
            throw new System.Exception("SteamLobbyController is Singleton. But there is anonther");

        SingletonInstance = this;
    }

    private void Start()
    {
        // 초대 수락 시 자동 입장
        SteamFriends.OnGameLobbyJoinRequested += async (lobby, friend) =>
        {
            Log($"Join requested from {friend.AccountId}, lobby: {lobby.Id}");
            await JoinLobbyAsync(lobby);
        };
    }

    private void OnDestroy()
    {
        if (_currentLobby.HasValue)
        {
            _currentLobby.Value.Leave();
            _currentLobby = null;
        }

        SteamMatchmaking.OnLobbyMemberJoined -= HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= HandleMemberLeft;
        SteamMatchmaking.OnLobbyMemberDataChanged -= HandleMemberDataChanged;
    }

    public async Task CreateLobbyAsync()
    {
        if (!SteamClient.IsValid)
        {
            LogError("SteamClient 미초기화. Steam을 켜고 SteamBootstrap이 있는지 확인하세요.");
            return;
        }

        Log("Creating lobby...");
        var result = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
        if (!result.HasValue)
        {
            LogError("CreateLobbyAsync 실패");
            return;
        }

        var lobby = result.Value;

        if (isPublic) lobby.SetPublic();
        else lobby.SetPrivate();

        lobby.SetJoinable(true);
        lobby.SetData("name", lobbyName);
        lobby.SetData("ver", Application.version);
        lobby.SetData("region", Application.cloudProjectId ?? "local");

        // 방장이 누군지 표현용
        SteamFriends.SetRichPresence("steam_display", $"Playing {Application.productName}");

        // 멤버 변화 이벤트 훅
        SteamMatchmaking.OnLobbyMemberJoined += HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += HandleMemberLeft;
        SteamMatchmaking.OnLobbyMemberDataChanged += HandleMemberDataChanged;
        _currentLobby = lobby;

        Log($"Lobby created: {lobby.Id}  |  Members: {lobby.MemberCount}/{lobby.MaxMembers}");
    }

    public async Task RefreshLobbyListAsync()
    {
        if (!SteamClient.IsValid)
        {
            LogError("SteamClient 미초기화");
            return;
        }

        Log("Requesting lobby list...");
        // 필터 예시: 이름/버전 맞는 방만
        var list = await SteamMatchmaking
            .LobbyList
            .WithKeyValue("ver", Application.version) // 동일 버전끼리만
            .WithSlotsAvailable(1) // 빈 자리 있는 방
            .RequestAsync();

        Log($"Found {list.Length} lobbies");
    }

    private async Task JoinLobbyAsync(Lobby lobby)
    {
        Log($"Joining lobby: {lobby.Id}");
        var result = await SteamMatchmaking.JoinLobbyAsync(lobby.Id);
        if (!result.HasValue)
        {
            LogError("JoinLobbyAsync 실패");
            return;
        }

        // 기존 로비 떠나기
        if (_currentLobby.HasValue)
            _currentLobby.Value.Leave();

        _currentLobby = result.Value;

        Log($"Joined lobby: {result.Value.Id} | Members: {result.Value.MemberCount}/{result.Value.MaxMembers}");
    }

    public async Task JoinLobbyAsync(string lobbyIdString)
    {
        if (string.IsNullOrWhiteSpace(lobbyIdString))
        {
            LogError("로비 ID가 비어 있습니다.");
            return;
        }

        if (!ulong.TryParse(lobbyIdString, out ulong lobbyIdValue))
        {
            LogError($"로비 ID가 올바르지 않습니다: {lobbyIdString}");
            return;
        }

        SteamId lobbySteamId = lobbyIdValue;

        var result = await SteamMatchmaking.JoinLobbyAsync(lobbySteamId);
        if (!result.HasValue)
        {
            LogError("JoinLobbyAsync 실패 (해당 ID의 로비가 없거나 접근 불가)");
            return;
        }

        if (_currentLobby.HasValue)
            _currentLobby.Value.Leave();

        _currentLobby = result.Value;
        Log($"Joined lobby: {(ulong)result.Value.Id} | Members: {result.Value.MemberCount}/{result.Value.MaxMembers}");
    }

    /// <summary>
    /// 초대 오버레이 띄우기
    /// </summary>
    public void OpenInviteOverlay()
    {
        if (!_currentLobby.HasValue)
        {
            LogError("현재 로비가 없습니다. 먼저 로비를 생성/입장하세요.");
            return;
        }

        SteamFriends.OpenGameInviteOverlay(_currentLobby.Value.Id);
        Log("Opened invite overlay.");
    }

    private void HandleMemberJoined(Lobby lobby, Friend friend)
    {
        if (_currentLobby.HasValue && lobby.Id == _currentLobby.Value.Id)
        {
            Log($"Member joined: {friend.Name}");
        }
    }

    private void HandleMemberLeft(Lobby lobby, Friend friend)
    {
        if (_currentLobby.HasValue && lobby.Id == _currentLobby.Value.Id)
        {
            Log($"Member left: {friend.Name}");
        }
    }

    private void HandleMemberDataChanged(Lobby lobby, Friend friend)
    {
        if (_currentLobby.HasValue && lobby.Id == _currentLobby.Value.Id)
        {
            Log($"Member data changed: {friend.Name}");
        }
    }

    private void Log(string msg)
    {
        Debug.Log($"[Lobby] {msg}");
    }

    private void LogError(string msg)
    {
        Debug.LogError($"[Lobby] {msg}");
    }
}
