// James Karlsson 13203260

using UnityEngine;

public class CameraMoveTrigger : MonoBehaviour
{
    private CameraController _camera;
    [SerializeField] private Transform _cameraTransformTarget;

    private Transform _previousTarget;
    [SerializeField] private bool _returnToPreviousTargetOnExit = true;
    [SerializeField] private float _speed = 4.0f;

    private void Start()
    {
        _camera = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerCharacter = other.gameObject.GetComponent<PlayerCharacter>();
        if (playerCharacter == null || !playerCharacter.hasAuthority)
            return;

        _previousTarget = _camera.Target;
        _camera.Target = _cameraTransformTarget;
        _camera.Speed = _speed;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_returnToPreviousTargetOnExit)
            return;

        var playerCharacter = other.gameObject.GetComponent<PlayerCharacter>();
        if (playerCharacter == null || !playerCharacter.hasAuthority)
            return;

        _camera.Target = _previousTarget;
        _camera.Speed = _speed;
    }
}