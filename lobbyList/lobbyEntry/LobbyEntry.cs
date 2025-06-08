using Godot;
using Steamworks;
using System;

public partial class LobbyEntry : Button
{
    public CSteamID lobbyID;
    
    public void Update(string lobbyName,int playerCount,int maxPlayerCount)
    {
        var lobbyNameLabel = GetNode<Label>("HBoxContainer/lobbyName");
        lobbyNameLabel.Text = lobbyName;
        var playerCountLabel = GetNode<Label>("HBoxContainer/playerCount");
        playerCountLabel.Text = $"{playerCount}/{maxPlayerCount}";

    }
    public override void _Pressed()
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }
}
