using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public static PlayerManager Instance => instance;
    private Dictionary<string, PlayerScript> players = new Dictionary<string, PlayerScript>();
    public void Awake()
    {
        instance = this;
    }

    public void RegisterPlayer(PlayerScript playerScript)
    {
        string id = GetPlayerId();
        players[id] = playerScript;
    }

    public PlayerScript GetPlayer()
    {
        string id = GetPlayerId();
        return players.GetValueOrDefault(id);
    }

    private string GetPlayerId()
    {
        // This is temporary, only here in case we decide to add multiplayer at some point
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            return PlayerPrefs.GetString("PlayerID");
        }
        
        string id  = Guid.NewGuid().ToString();
        PlayerPrefs.SetString("PlayerID", id);
        return id;
    }
}
