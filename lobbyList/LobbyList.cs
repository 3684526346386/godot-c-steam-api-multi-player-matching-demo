using Godot;
using Steamworks;
using System;

public partial class LobbyList : Node
{
    protected Callback<LobbyMatchList_t> lobbylistCallback;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;

    Node entryParent;
    public override void _EnterTree()
    {
        entryParent = GetNode("Panel/VBoxContainer/ScrollContainer/VBoxContainer");
        var updateBut = GetNode<Button>("Panel/UpdateButton");
        updateBut.Pressed += UpdateLobbyList;

        lobbylistCallback = Callback<LobbyMatchList_t>.Create(OnRequestLobbyList);
        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);

        UpdateLobbyList();
    }
    public override void _ExitTree()
    {
        lobbylistCallback.Dispose();
        lobbyDataUpdateCallback.Dispose();
    }
    public void UpdateLobbyList()
    {
        SteamMatchmaking.RequestLobbyList();
        ClearAllEntry();
    }
    void ClearAllEntry()
    {
        var entryParent = GetNode("Panel/VBoxContainer/ScrollContainer/VBoxContainer");
        foreach (var entry in entryParent.GetChildren())
        {
            entry.QueueFree();
        }
    }
    void OnRequestLobbyList(LobbyMatchList_t list_T)
    {
        uint count = list_T.m_nLobbiesMatching;
        for (int i = 0; i < count; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            SteamMatchmaking.RequestLobbyData(lobbyID);

            var entry = GD.Load<PackedScene>("res://lobbyList/lobbyEntry/entry.tscn").
                Instantiate<LobbyEntry>();
            entryParent.AddChild(entry);
            entry.lobbyID = lobbyID;

            if(i == 0)
            {
                entry.GrabFocus();
            }
        }
    }
    void OnLobbyDataUpdate(LobbyDataUpdate_t update_T)
    {
        CSteamID lobbyID = (CSteamID)update_T.m_ulSteamIDLobby;

        foreach (var child in entryParent.GetChildren())
        {
            if (child is LobbyEntry entry && entry.lobbyID == lobbyID)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, "lobbyName");
                int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                int maxPlayerCount = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
                entry.Update(lobbyName, playerCount, maxPlayerCount);
            }
        }

    }
}
