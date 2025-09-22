using NUnit;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool gameEnded = false; // has the game ended?
    public float timeToWin; // time a player needs to hold the hat to win
    public float invincibleDuration; // how long after a player gets the hat, are they invincible
    private float hatPickupTime; // the time the hat was picked up by the current holder
    [Header("Players")]
    public string playerPrefabLocation; // path in Resources folder to the Player prefab
    public string[] powerupsPrefabLocation;
    public Transform[] spawnPoints; // array of all available spawn points
    public Transform[] powerSpawnPoints;
    public PlayerController[] players; // array of all the players
    public int playerWithHat; // id of the player with the hat
    private int playersInGame; // number of players in the game
                               // instance
    public static GameManager instance;
    void Awake()
    {
        // instance
        instance = this;
        StartCoroutine(SpawnPowerup());
    }
    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.All);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
    }

    // spawns a player and initializes it
    void SpawnPlayer()
    {
        // instantiate the player across the network
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity); //Made Changes
        // get the player script
        PlayerController playerScript = playerObj.GetComponent<PlayerController>();
        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer); //Made Changes
    }

    private IEnumerator SpawnPowerup()
    {
        while (true)
        {
            if (Random.Range(1, 2) == 1)
            {
                GameObject spawnObj = PhotonNetwork.Instantiate(powerupsPrefabLocation[Random.Range(0, powerupsPrefabLocation.Length)],
                    powerSpawnPoints[Random.Range(0, powerSpawnPoints.Length)].position, Quaternion.identity);
            }
            yield return new WaitForSeconds(5f);
        }
    }
    public PlayerController GetPlayer(int playerId)
    {
        return players.First(x => x.id == playerId);
    }
    public PlayerController GetPlayer(GameObject playerObject)
    {
        return players.First(x => x.gameObject == playerObject);
    }

    // called when a player hits the hatted player - giving them the hat
    [PunRPC]
    public void GiveHat(int playerId, bool initialGive)
    {
        // remove the hat from the currently hatted player
        if (!initialGive)
            GetPlayer(playerWithHat).SetHat(false);
        // give the hat to the new player
        playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        hatPickupTime = Time.time;
    }

    [PunRPC]
    public void SlowDown(int playerId)
    {
        Debug.Log($"SlowDown START for player {playerId} at t={Time.time}");

        StartCoroutine(SpeedCountdown(playerId));
    }

    private IEnumerator SpeedCountdown(int playerId)
    {
        Debug.Log($"Applied slow: -> {players[playerId - 1].moveSpeed} at t={Time.time}");

        players[playerId - 1].moveSpeed = players[playerId - 1].moveSpeed * (float)0.5;
        yield return new WaitForSeconds(5f);
        players[playerId - 1].moveSpeed = players[playerId - 1].moveSpeed / (float)0.5;
        Debug.Log($"SlowDown END for player {playerId}; restored to {players[playerId - 1].moveSpeed} at t={Time.time}");

    }

    // is the player able to take the hat at this current time?
    public bool CanGetHat()
    {
        if (Time.time > hatPickupTime + invincibleDuration)
            return true;
        else
            return false;
    }

    [PunRPC]
    void WinGame(int playerId)
    {
        gameEnded = true;
        PlayerController player = GetPlayer(playerId);
        // set the UI to show who's won
        Invoke("GoBackToMenu", 3.0f);
        GameUI.instance.SetWinText(player.photonPlayer.NickName);
    }
    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}
