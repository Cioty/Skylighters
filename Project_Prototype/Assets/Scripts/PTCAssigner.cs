﻿using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;
using UnityEngine.SceneManagement;

public class PTCAssigner : MonoBehaviour
{
    public GameObject playerContainersGroup;
    public GameObject allControllersConnectedScreen;
    public static bool controllerFound = false;


    private int connectedControllers = 0;
    private int assignedPlayers = 0;
    private bool isAtConnectScreen = false;
    private bool hasSearchedForControllers = false;
    private List<XboxController> yetToBeConnectedList = new List<XboxController>();
    private List<PlayerContainer> playerContainers = new List<PlayerContainer>();
    private bool allControllersConnected = false;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        // Search for controllers:
        if (!hasSearchedForControllers)
        {
            hasSearchedForControllers = true;

            connectedControllers = XCI.GetNumPluggedCtrlrs();
            if (connectedControllers == 1)
            {
                Debug.Log("Only " + connectedControllers + " Xbox controller plugged in.");
                controllerFound = true;
            }
            else if (connectedControllers == 0)
                Debug.Log("No Xbox controllers plugged in!");
            else
            {
                Debug.Log(connectedControllers + " Xbox controllers plugged in.");
                controllerFound = true;
            }

            XCI.DEBUG_LogControllerNames();
        }
        
        // Adding the player containers to a list.
        for (int i = 0; i < playerContainersGroup.transform.childCount; ++i)
            playerContainers.Add(playerContainersGroup.transform.GetChild(i).transform.gameObject.GetComponent<PlayerContainer>());

        // Adding to the yet to bed added list
        for (int c = 1; c < connectedControllers + 1; ++c)
        {
            XboxController xboxController = ((XboxController)c);

            if (xboxController == XboxController.All)
                continue;

            yetToBeConnectedList.Add(xboxController);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isAtConnectScreen)
        {
            // check for input
            if (!allControllersConnected)
            {
                if(yetToBeConnectedList.Count > 0)
                {
                    for (int c = 0; c < yetToBeConnectedList.Count; ++c)
                    {
                        XboxController xboxController = yetToBeConnectedList[c];
                        if (xboxController == XboxController.All)
                            continue;

                        if (XCI.GetButtonUp(XboxButton.A, xboxController))
                        {
                            this.AddController(c, xboxController);
                        }
                    }
                }
                else if (assignedPlayers > 0)
                {
                    allControllersConnectedScreen.SetActive(true);
                    allControllersConnected = true;
                }
            }
            else
            {
                if (XCI.GetButtonUp(XboxButton.A, XboxController.All))
                {
                    PlayerData.instance.Save();
                    SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
                }

                if (XCI.GetButtonUp(XboxButton.B, XboxController.All))
                {
                    allControllersConnected = false;
                    allControllersConnectedScreen.SetActive(false);
                }
            }
        }
    }

    // Loops through the container list and returns the first container without a player.
    private PlayerContainer FindNextEmptyContainer()
    {
        for(int i = 0; i < playerContainers.Count; ++i)
        {
            // Checking if the container has a player, and skipping the return if it does:
            PlayerContainer container = playerContainers[i].GetComponent<PlayerContainer>();
            if (container.HasPlayer)
                continue;

            // Returning the empty container.
            return container;
        }

        // Returns null if no empty container is found.
        return null;
    }

    private void AddController(int id, XboxController controller)
    {
        // If there's a container empty, then add a player into it. (Not sure if we should do this, or just match the ID to the connected controller).
        GameObject containerGO = this.FindNextEmptyContainer().gameObject;
        if (containerGO)
        {
            PlayerContainer container = containerGO.GetComponent<PlayerContainer>();
            containerGO.GetComponent<Renderer>().material.color = Color.green;
            container.ID = id;
            container.Controller = controller;
            container.HasPlayer = true;
            yetToBeConnectedList.Remove(controller);
            ++assignedPlayers;
        }
        else
        {
            Debug.Log("Can't find an empty container.");
        }
    }

    public List<PlayerContainer> GetPlayerContainers()
    {
        return playerContainers;
    }

    public int AssignedPlayers
    {
        get { return assignedPlayers; }
    }

    public int ConnectedControllers
    {
        get { return connectedControllers; }
    }

    public bool IsAtConnectScreen
    {
        get { return isAtConnectScreen;  }
        set { isAtConnectScreen = value; }
    }
}