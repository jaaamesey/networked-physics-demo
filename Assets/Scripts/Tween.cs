using UnityEngine;

public class Tween
{
    public enum TweenType
    {
        Linear,
        Quadratic,
        Cubic
    }
    
    public Transform Target { get; private set; }
    public Vector3 StartPos { get; private set; }
    public Vector3 EndPos { get; private set; }
    public float StartTime { get; private set; }
    public float Duration { get; private set; }
    public TweenType Type { get; private set; }

    public Tween(Transform target, Vector3 startPos, Vector3 endPos, float startTime, float duration, TweenType type)
    {
        Target = target;
        StartPos = startPos;
        EndPos = endPos;
        StartTime = startTime;
        Duration = duration;
        Type = type;
    }
}