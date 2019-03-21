// James Karlsson 13203260

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Determines how exactly the camera will move to a certain point
    public enum CameraMode
    {
        Smooth,
        Instant,
        StaticLerp // CURRENTLY UNUSED
    }

    public Transform InitialTarget;
    public float DefaultSpeed = 4.0f;
    [HideInInspector] public float Speed = 4.0f;
    public CameraMode Mode = CameraMode.Smooth;
    public Tween.TweenType TweenType = Tween.TweenType.Linear;

    private float _lerpTime = 1.0f;

    private Tweener _tweener;

    public Transform Target
    {
        get { return _target; }
        set
        {
            if (Mode == CameraMode.StaticLerp && value != null) // Use linear tween with Tweener
                _tweener.AddTween(transform, transform.position, value.position, _lerpTime, TweenType);
            else if (Mode == CameraMode.Instant && value != null) // Instantly teleport camera
                transform.position = value.position;

            _target = value;
        }
    }

    private Transform _target;

    // Use this for initialization
    private void Start()
    {
        _tweener = Tweener.GetCurrent();
        Target = InitialTarget;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Mode != CameraMode.Smooth) return;

        // Smoothly move towards target position
        var distance = Vector3.Distance(transform.position, _target.position);
        var differenceVector = _target.position - transform.position;
        var moveVector = differenceVector.normalized;

        if (differenceVector.sqrMagnitude >= 0.05f)
            transform.position += distance * moveVector * Speed * Time.deltaTime;
        else Speed = DefaultSpeed;
        transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, Speed * Time.deltaTime);
    }
}