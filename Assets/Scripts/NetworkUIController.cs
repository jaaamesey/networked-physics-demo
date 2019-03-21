// James Karlsson 13203260

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class NetworkUIController : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private InputField _roomCodeField;
    [SerializeField] private Button _joinGameButton;
    [SerializeField] private Button _hostGameButton;
    [SerializeField] private Text _messageText;
    [SerializeField] private Text _pingText;
    [SerializeField] private Text _playerText;

    private NetworkManagerHUD _oldHud; // Default terrible Unity Network Manager HUD

    private const float SpamPreventionTime = 3.0f;

    private float _spamPreventionTimer = 0.0f;

    // Use this for initialization
    private void Start()
    {
        _oldHud = _networkManager.GetComponent<NetworkManagerHUD>();

        _joinGameButton.onClick.AddListener(OnJoinPressed);
        _hostGameButton.onClick.AddListener(OnHostPressed);

        _networkManager.StartMatchMaker();
    }

    // Update is called once per frame
    private void Update()
    {
        // Show ping if connected
        if (UnityEngine.Networking.NetworkManager.singleton.IsClientConnected())
            _pingText.text = "Ping: " + UnityEngine.Networking.NetworkManager.singleton.client.GetRTT();
        else
            _pingText.text = "Disconnected";

        // Prevent clients from spamming "Join Game" or "Host Game" and inadvertently 
        // DDoSing Unity's matchmaking servers :( 

        _spamPreventionTimer -= 1 * Time.deltaTime;

        if (_spamPreventionTimer > 0)
        {
            _joinGameButton.interactable = false;
            _hostGameButton.interactable = false;
        }
        else
        {
            _joinGameButton.interactable = true;
            _hostGameButton.interactable = true;
            // Prevent value from overflowing downwards if left idle for 1.077418*10^31 years
            _spamPreventionTimer = 0;
        }

        // Toggle old hud on n keypress
        if (Input.GetKeyDown(KeyCode.N))
            _oldHud.showGUI = !_oldHud.showGUI;
    }

    private void OnJoinPressed()
    {
        _spamPreventionTimer = SpamPreventionTime;
        ResetNetworking();

        var matchName = _roomCodeField.text.Trim();

        // Join IP address directly without matchmaker service if specified
        if (matchName.Contains(".") || matchName == "localhost")
        {
            //_networkManager.client.Configure(new ConnectionConfig(), 1);
            var ipSplit = matchName.Split(':');
            _networkManager.networkAddress = ipSplit[0];
            _networkManager.networkPort = ipSplit.Length == 2 ? int.Parse(ipSplit[1]) : 7777;

            _networkManager.StartClient();
            _networkManager.client.Connect(_networkManager.networkAddress, _networkManager.networkPort);
            return;
        }

        _networkManager.matchMaker.ListMatches(0, 20, matchName, false, 0, 0, OnMatchListFound);
    }

    private void OnHostPressed()
    {
        _spamPreventionTimer = SpamPreventionTime;

        ResetNetworking();
        var matchName = _roomCodeField.text.Trim();

        // Host locally without matchmaker service if specified
        if (matchName.Contains(".") || matchName.Contains("localhost"))
        {
            //_networkManager.client.Configure(new ConnectionConfig(), 1);
            _networkManager.serverBindToIP = true;
            var ipSplit = matchName.Split(':');
            _networkManager.serverBindAddress = ipSplit[0];
            _networkManager.networkPort = ipSplit.Length == 2 ? int.Parse(ipSplit[1]) : 7777;
            //NetworkServer.Listen(_networkManager.networkPort);
            _networkManager.StartHost();
            return;
        }

        _networkManager.matchMaker.CreateMatch(matchName, 10, true, "", "", "", 0, 0, OnMatchCreated);
    }

    private void OnMatchCreated(bool success, string info, MatchInfo matchInfoData)
    {
        print(matchInfoData);
        _messageText.text = matchInfoData.ToString();

        if (!success)
        {
            _messageText.text = info;
            return;
        }

        NetworkServer.Listen(matchInfoData, 443);
        _networkManager.StartHost(matchInfoData);
    }

    private void OnMatchListFound(bool success, string info, List<MatchInfoSnapshot> matchInfoSnapshots)
    {
        _messageText.text = matchInfoSnapshots.ToString();

        if (!success)
        {
            _messageText.text = info;
            return;
        }

        var match = matchInfoSnapshots[0];
        _networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, OnMatchJoined);
    }

    private void OnMatchJoined(bool success, string info, MatchInfo matchInfoData)
    {
        _messageText.text = matchInfoData.ToString();

        if (!success)
        {
            _messageText.text = info;
            return;
        }

        _networkManager.StartClient(matchInfoData);
    }

    private void ResetNetworking()
    {
        _networkManager.StopMatchMaker();
        _networkManager.StopHost();
        _networkManager.StopClient();
        _networkManager.StartMatchMaker();
        NetworkServer.Reset();
    }
}