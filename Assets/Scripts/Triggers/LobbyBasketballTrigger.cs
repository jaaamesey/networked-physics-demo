// James Karlsson 13203260

using UnityEngine;
using UnityEngine.Networking;

public class LobbyBasketballTrigger : NetworkBehaviour
{
    [SerializeField] private Transform _basketballGameTransform;
    private CameraController _camera;
    [SerializeField] private Transform _newCameraTarget;

    private Tweener _tweener;

    // Use this for initialization
    private void Start()
    {
        _tweener = Tweener.GetCurrent();
        _camera = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerCharacter>() != null)
            CmdStartBasketballGame();
    }

    [Command]
    private void CmdStartBasketballGame()
    {
        RpcStartBasketballGame();
    }

    [ClientRpc]
    private void RpcStartBasketballGame()
    {
        var endPos = new Vector3(_basketballGameTransform.position.x, 0f, _basketballGameTransform.position.z);
        _tweener.AddTween(_basketballGameTransform, _basketballGameTransform.position, endPos, 3.0f,
            Tween.TweenType.Cubic, false);
        _camera.Target = _newCameraTarget;
    }
}