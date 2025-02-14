﻿/*=============================================================================
 * Game:        Metallicide
 * Version:     Beta
 * 
 * Class:       PlayerManager.cs
 * Purpose:     Manages the loading of players into the game scene.
 * 
 * Author:      Lachlan Wernert
 * Team:        Skylighter
 * 
 * Deficiences:
 * 
 *===========================================================================*/
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerManager : MonoBehaviour
{
    // Singleton instance:
    public static PlayerManager instance;

    [Header("References")]
    public GameObject playerPrefab;
    public List<GamemodeHandler> splitScreenModeArray = new List<GamemodeHandler>();

    [Header("Debug Options")]
    public bool forceDebugMode = false;
    [Range(1, 4)]
    public int debugPlayerCount = 1;

    // Private variables:
    private PlayerData.SplitScreenMode splitScreenMode;
    private GamemodeHandler currentGameMode;
    private List<PlayerHandler> activePlayers = new List<PlayerHandler>();
    private List<PlayerContainer> playerContainers;
    private int playerCount = 0;
    private bool containersValid = false;

    private void Awake()
    {
        instance = this;

        if(PlayerData.instance != null && PlayerData.instance.StartInDebugMode)
        {
            forceDebugMode = true;
            debugPlayerCount = PlayerData.instance.DebugPlayerCount;
        }

        if (PlayerData.instance != null && !forceDebugMode)
        {
            splitScreenMode = PlayerData.instance.CurrentSplitScreenMode;
            playerContainers = PlayerData.instance.GetTransferedPlayerContainers();
            containersValid = true;
        }

        if(forceDebugMode)
        {
            PlayerData.SplitScreenMode splitDebugMode = ((PlayerData.SplitScreenMode)(debugPlayerCount - 1));
            splitScreenMode = splitDebugMode;
        }
    }

    private void Start()
    {
        // Set up correct screen view:
        ActivateCorrectScreenView();

        // Position all players:
        ActivateAndPositionAllPlayers();

        // Resets the occupancy of the mech station:
        RespawnArray.instance.ResetOccupiedMechStations();
    }
    
    private void ActivateCorrectScreenView()
    {
        currentGameMode = splitScreenModeArray[(int)splitScreenMode];
        activePlayers = currentGameMode.GetPlayerList();

        // Checking if debugMode:
        if (forceDebugMode)
            playerCount = debugPlayerCount;
        else
        {
            playerCount = activePlayers.Count;
        }

        Debug.Log("Setting gamestate up for " + playerCount + " players!");
        currentGameMode.gameObject.SetActive(true);
    }

    private void ActivateAndPositionAllPlayers()
    {
        for(int i = 0; i < playerCount; ++i)
        {
            // Getting the playerHandler for ease of use:
            PlayerHandler playerHandler = activePlayers[i];

            // Activating the player:
            playerHandler.gameObject.SetActive(true);

            //Assigning the correct controller:
            if (!forceDebugMode)
            {
                // Checking if the players have been loaded correctly:
                if (containersValid)
                {
                    // Assigning the players controller to the controller saved in the playerContainer: 
                    playerHandler.AssignedController = playerContainers[i].Controller;
                }
                else
                {
                    // Logging an error to the user to let them know the player hasn't loaded properly:
                    Debug.LogError("Player container " + "'" + i + "'" + " is null! " +
                        "Make sure you're loading the player from the Player-Manager and one doesn't already exist in the scene.");
                }
            }
            else
            {
                // Converting index 'i' into an xbox controller, then setting it to the indexed player handler:
                playerHandler.AssignedController = ((XboxController)i);

                // If not player0
                if (i > 0)
                {
                    playerHandler.IsTestDummy = true;
                    playerHandler.IsControllable = false;
                }
            }

            // Spawning the player in.
            playerHandler.RandomSpawn_Unactive();
        }

        // Resetting the occupied flag in the respawn stations to prevent overflows:
        RespawnArray.instance.ResetOccupiedMechStations();
    }

    public List<PlayerHandler> ActivePlayers
    {
        get { return activePlayers; }
    }
}
