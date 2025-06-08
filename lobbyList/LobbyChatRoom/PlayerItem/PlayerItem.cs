using Godot;
using Steamworks;
using System;

public partial class PlayerItem : Node
{
    public static CSteamID lobbyID;
    public CSteamID steamID;
    public void Update(CSteamID steamID)
    {
        this.steamID = steamID;
        var nameLabel = GetNode<Label>("name");
        nameLabel.Text = SteamFriends.GetFriendPersonaName(steamID);
        if(SteamMatchmaking.GetLobbyOwner(lobbyID) == steamID)
        {
            nameLabel.Text = nameLabel.Text + "(host)";
        }
    }
}
