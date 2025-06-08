using Godot;
using Steamworks;
using System;
using System.Collections.Generic;

public partial class LobbyChatRoom : Node
{
    public static LobbyChatRoom instance;
    public LobbyChatRoom()
    {
        instance = this;
    }

    Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;

    Node playerItemCont;
    public override void _EnterTree()
    {
        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);

        playerItemCont = GetNode("Panel/VBoxContainer/ScrollContainer/ItemCont");
        GetNode<Button>("quit").Pressed += () =>
        {
            SteamMatchmaking.LeaveLobby(lobbyID);
            QueueFree();
            instance = null;
        };
    }
    public override void _ExitTree()
    {
        lobbyDataUpdateCallback.Dispose();
    }

    public CSteamID lobbyID;
    public void Init(CSteamID lobbyID)
    {
        this.lobbyID = lobbyID;
        PlayerItem.lobbyID = lobbyID;
        UpdateLobbyInfo();
        UpdatePlayerList();

    }
    public void UpdateLobbyInfo()
    {
        string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, "lobbyName");
        int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        int maxPlayerCount = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);

        GetNode<Label>("Panel/VBoxContainer/Bottom/Infos/lobbyname").Text
            = lobbyName;
    }
    public void UpdatePlayerList()
    {
        List<CSteamID> list = new List<CSteamID>();
        List<bool> found = new List<bool>();
        int pCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        for (int i = 0; i < pCount; i++)
        {
            list.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobbyID,i));
            found.Add(false);
        }
        foreach (var child in playerItemCont.GetChildren())
        {
            if(child is PlayerItem item)
            {
                for (int i = 0;i < list.Count; i++)
                {
                    if (list[i] == item.steamID)
                    {
                        item.Update(item.steamID);
                        found[i] = true;
                        break;
                    }
                    if(i ==list.Count - 1)
                    {
                        item.QueueFree();
                        continue;
                    }
                }
            }

        }
        for (int i = 0; i < list.Count; i++)
        {
            if (!found[i])
            {
                var item = GD.Load<PackedScene>("res://lobbyList/LobbyChatRoom/PlayerItem/playeritem.tscn").
               Instantiate<PlayerItem>();
                playerItemCont.AddChild(item);
                item.Update(list[i]);
            }
        }
    }
    void OnLobbyDataUpdate(LobbyDataUpdate_t update_T)
    {
        var id = new CSteamID(update_T.m_ulSteamIDLobby);
        PlayerItem.lobbyID = id;
        if (lobbyID != id)
        {
            this.lobbyID = id;
            UpdateLobbyInfo();
        }
        if(update_T.m_ulSteamIDLobby != update_T.m_ulSteamIDMember)
        {
            UpdatePlayerList();
        }
    }
}
