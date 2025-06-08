using Godot;
using System;
using Steamworks;
public partial class SteamManager : Node
{
    public SteamManager()
    {
        Instance = this;
    }
    public static SteamManager Instance;
    public bool initialised = false;

    protected Callback<LobbyEnter_t> lobbyEnterCallback;
    public override void _EnterTree()
    {
        if (!SteamAPI.Init())
        {
            GD.PrintErr("steamwork init error");
            return;
        }
        initialised = true;
        GD.Print("current user name:" + SteamFriends.GetPersonaName());

        lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }
    public override void _ExitTree()
    {
        lobbyEnterCallback.Dispose();
        SteamAPI.Shutdown();
    }
    public CSteamID currentLobbyID;
    void OnLobbyEnter(LobbyEnter_t enter_T)
    {
        if (enter_T.m_EChatRoomEnterResponse != 1) return;
        currentLobbyID = new CSteamID(enter_T.m_ulSteamIDLobby);

        if (LobbyChatRoom.instance != null)
        {
            LobbyChatRoom.instance.QueueFree();
            LobbyChatRoom.instance = null;
        }
        var room = GD.Load<PackedScene>("res://lobbyList/LobbyChatRoom/chatroom.tscn").
                Instantiate<LobbyChatRoom>();
        AddChild(room);
        room.Init(new CSteamID(enter_T.m_ulSteamIDLobby)); 
    }

    public override void _Process(double delta)
    {
        if (!initialised) return;
        SteamAPI.RunCallbacks(); 
    }
}
