using UnityEngine;

public class RotationTween
{
	// @TODO: Actually implement this maybe?
	public Quaternion StartRot { get; private set; }
	public Quaternion EndRot { get; private set; }
	public float StartTime { get; private set; }
	public float Duration { get; private set; }

	public RotationTween(Quaternion startPos, Quaternion endPos, float startTime, float duration)
	{
		StartRot = startPos;
		EndRot = endPos;
		StartTime = startTime;
		Duration = duration;
	}
}