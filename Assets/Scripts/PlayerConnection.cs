// James Karlsson 13203260

using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour
{
    [SerializeField] private PlayerCharacter _playerCharacter;
    private CameraController _camera;

    private Vector3 _spawnPos = new Vector3(0, 2.6f, 0);


    // Use this for initialization
    private void Start()
    {
        _camera = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

        // Randomise randomise-er
        Random.InitState((int) (Time.deltaTime * 1000000 + Time.realtimeSinceStartup));
        _spawnPos = new Vector3(0, 2.6f, Random.Range(-6f, 6f));

        // Check if own local connection object
        if (!isLocalPlayer)
        {
            // Object belongs to another player - do not run code
            return;
        }


        // Command server to spawn _playerCharacter
        CmdSpawnPlayerCharacter();
    }

    // Update is called once per frame (ON LITERALLY EVERYONE'S COMPUTER - BE CAREFUL)
    private void Update()
    {
    }

    // Destructor
    private void OnDestroy()
    {
        if (!hasAuthority)
            return;

        // Reset camera position on disconnect
        _camera.Target = _camera.InitialTarget;
        _camera.Speed = 20f;
    }


    // COMMANDS (**SERVER ONLY** FUNCTIONS)
    [Command]
    private void CmdSpawnPlayerCharacter()
    {
        var count = NetworkServer.connections.Count;
        // Force (somewhat) truly random seed
        Random.InitState((int) (GetInstanceID() + Time.deltaTime * 1000000 + Time.realtimeSinceStartup));
        _spawnPos = new Vector3(0, 2.6f, Random.Range(-6f, 6f));

        // Make object exist on server and get reference to it
        _playerCharacter.SvrLightColor = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1);
        _playerCharacter.transform.position = _spawnPos;

        _playerCharacter.PlayerNetworkConnection = connectionToClient;

        _playerCharacter.Index = count - 1;

        _playerCharacter = Instantiate(_playerCharacter, _spawnPos, Quaternion.identity);

        // Spawn object on all clients
        NetworkServer.Spawn(_playerCharacter.gameObject);

        // Give spawning player connection authority over its own character
        _playerCharacter.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);

        Debug.Log("Player #" + count + " spawned at " + _spawnPos);
    }

    // Setters and getters

    public PlayerCharacter PlayerCharacter
    {
        // Read only
        get { return _playerCharacter; }
    }
}