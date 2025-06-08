using Godot;
using Steamworks;
using System;

public partial class LobbyCreate : Node
{
    int maxplayercount = 4;
    ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic;
    Callback<LobbyCreated_t> lobbyCreateCallback;
    public override void _EnterTree()
    {
        var edit = GetNode<TextEdit>("Panel/VBoxContainer/TextEdit");
        edit.Text = SteamFriends.GetPersonaName()+"'s lobby";

        var pcountBut = GetNode<MenuButton>("Panel/VBoxContainer/HBoxContainer/pcount/MenuButton");
        pcountBut.Text = maxplayercount.ToString();
        var countpop = pcountBut.GetPopup();
        countpop.IndexPressed += (long index) =>
        {
            var text = countpop.GetItemText((int)index);
            pcountBut.Text = text;
            maxplayercount = int.Parse(text);
        };

        var visiBut = GetNode<MenuButton>("Panel/VBoxContainer/HBoxContainer/visi/MenuButton");
        visiBut.Text = "public";
        var visipop = visiBut.GetPopup();
        visipop.IndexPressed += (long index) =>
        {
            var text = visipop.GetItemText((int)index);
            visiBut.Text = text;
            switch (index)
            {
                case 0:
                    lobbyType = ELobbyType.k_ELobbyTypePublic;
                    break;
                case 1:
                    lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
                    break;
                case 2:
                    lobbyType = ELobbyType.k_ELobbyTypePrivate;
                    break;
            }
        };

        var create = GetNode<Button>("Panel/create");
        create.Pressed += () =>
        {
            SteamMatchmaking.CreateLobby(lobbyType,maxplayercount);
        };
        var quit = GetNode<Button>("Panel/quit");
        quit.Pressed += () =>
        {
            QueueFree();
        };


        lobbyCreateCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreate);
    }
    public override void _Ready()
    {
        var create = GetNode<Button>("Panel/create");
        create.GrabFocus();
    }
    public override void _ExitTree()
    {
        lobbyCreateCallback.Dispose();
    }
    void OnLobbyCreate(LobbyCreated_t created_T)
    {
        SteamMatchmaking.SetLobbyData(new CSteamID(created_T.m_ulSteamIDLobby), "lobbyName",
            GetNode<TextEdit>("Panel/VBoxContainer/TextEdit").Text);
        QueueFree();
    }
}
