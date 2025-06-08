using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

public partial class LobbyChatRoom : Control
{
    public static LobbyChatRoom instance;
    public LobbyChatRoom()
    {
        instance = this;
    }

    Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;
    Callback<LobbyChatMsg_t> lobbyChatMsgCallback;

    Node playerItemCont;
    TextEdit chatEdit;
    Control msgCont;
    VScrollBar chatScrollBar;
    public override void _EnterTree()
    {
        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
        
        playerItemCont = GetNode("Panel/VBoxContainer/ScrollContainer/ItemCont");
        chatEdit = GetNode<TextEdit>("Panel/VBoxContainer/Bottom/VBoxContainer/TextEdit");
        msgCont = GetNode<Control>("Panel/VBoxContainer/Bottom/VBoxContainer/Panel/Chat/VBoxContainer");
        chatScrollBar = GetNode<ScrollContainer>("Panel/VBoxContainer/Bottom/VBoxContainer/Panel/Chat").GetVScrollBar();
        chatScrollBar.FocusMode = FocusModeEnum.Click;
       
        GetNode<Button>("quit").Pressed += () =>
        {
            SteamMatchmaking.LeaveLobby(lobbyID);
            QueueFree();
            instance = null;
        };
    }
    public override void _Ready()
    {
        chatEdit.GrabFocus();
        // change it
    }
    public override void _ExitTree()
    {
        lobbyDataUpdateCallback.Dispose();
    }

    public override void _Process(double delta)
    {
        if (!chatScrollBar.HasFocus()) SetBar();
    }
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey key)
        {
            if (chatEdit.HasFocus() && key.Keycode == Key.Enter && key.Pressed)
            {
                AcceptEvent();
                if(chatEdit.Text != null && chatEdit.Text != "")
                {
                    if (chatEdit.Text.Length > 256) return;
                    byte[] buf = Encoding.UTF8.GetBytes(chatEdit.Text);
                    SteamMatchmaking.SendLobbyChatMsg(lobbyID, buf, buf.Length + 1);
                    chatEdit.Text = "";
                }
            }
        }
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
    public void SetBar()
    {
        chatScrollBar.Value = chatScrollBar.MaxValue;
    }
    void OnLobbyChatMsg(LobbyChatMsg_t msg_T)
    {
        byte[] buf = new byte[1024];
        CSteamID userID = new CSteamID();
        EChatEntryType eChatEntryType = new EChatEntryType();
        SteamMatchmaking.GetLobbyChatEntry(lobbyID, (int)msg_T.m_iChatID, out userID, buf, 1024, out eChatEntryType);

        Label label = new Label();
        label.Text = SteamFriends.GetFriendPersonaName(userID) +": "+ Encoding.UTF8.GetString(buf);
        msgCont.AddChild(label);

        if(msgCont.GetChildCount() > 20)
        {
            msgCont.GetChild(0).QueueFree();
        }
    }
}
