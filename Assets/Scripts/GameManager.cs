// James Karlsson 13203260

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class GameManager : NetworkBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _enemyPrefab;
    public PlayerCharacter CurrentPlayer = null;
    public NetworkUIController NetworkUIController;

    private List<Transform> _enemies = new List<Transform>();

    // @TODO: Maybe add a dictionary of starting positions for all physics objects so that they are able to be reset properly each time a game is hosted
    // (or just reload the scene without somehow messing up networking functionality)

    // Update is called once per frame
    private void Update()
    {
        // DEBUG: Spawn an enemy when K is pressed.
        if (Input.GetKeyDown(KeyCode.K))
            CmdSpawnEnemy(GetPlayerCharacter(0).RespawnLocation);

        // DEBUG: Soft reset the game with Ctrl F5 (BUGGY)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F5))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private List<PlayerController> GetPlayerControllers()
    {
        return _networkManager.client.connection.playerControllers;
    }

    public PlayerConnection GetPlayerConnection(int index)
    {
        var controllers = GetPlayerControllers();
        if (controllers == null || controllers.Count <= 0)
            return null;
        return GetPlayerControllers()[index].gameObject.GetComponent<PlayerConnection>();
    }

    public PlayerCharacter GetPlayerCharacter(int index)
    {
        // @TODO: Store a reference to already obtained player characters for performance reasons
        var connection = GetPlayerConnection(index);
        // Scary ternary operator just returns player or null if connection doesn't exist
        return connection == null ? null : GetPlayerConnection(index).PlayerCharacter;
    }

    public static GameManager GetCurrent()
    {
        return GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    // COMMANDS (**SERVER ONLY** FUNCTIONS)
    // Spawn an enemy at a specified position in the scene
    [Command]
    public void CmdSpawnEnemy(Vector3 pos)
    {
        var enemy = Instantiate(_enemyPrefab, pos, Quaternion.identity);
        NetworkServer.Spawn(enemy.gameObject);
        _enemies.Add(enemy.transform);
    }

/*    [Command] // Not yet implemented
    public void CmdResetPhysicsObjects()
    {
        
    }*/

}