using System;
using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    // @TODO: Allow for selection of easing in, out, or both
    
    private readonly List<Tween> _activeTweens = new List<Tween>();
    private readonly List<Tween> _activeTweensToAdd = new List<Tween>();
    private readonly List<Tween> _activeTweensToRemove = new List<Tween>();

    // Update is called once per frame
    private void Update()
    {
        foreach (var tween in _activeTweens)
        {
            // Get distance between EndPos and Target.position
            var dist = Vector3.Distance(tween.EndPos, tween.Target.position);
            if (Mathf.Abs(dist) >= 0.001f)
                UpdateTween(tween);
            else
            {
                tween.Target.position = tween.EndPos;
                _activeTweensToRemove.Add(tween); // Queue removal from list once completed
            }
        }

        MergeTweenLists();
    }

    public bool AddTween(Transform targetObject, Vector3 startPos, Vector3 endPos, float duration,
        Tween.TweenType type = Tween.TweenType.Linear, bool removeExisting = true)
    {
        var existingTween = GetActiveTween(targetObject);
        if (existingTween != null) // Tween already exists
        {
            if (!removeExisting)
                return false; // Return false and do nothing else if exists and told to not remove

            _activeTweensToRemove.Add(existingTween); // Queue removal of existing tween before adding new one
        }

        _activeTweensToAdd.Add(new Tween
            (targetObject, startPos, endPos, Time.time, duration, type));
        return true;
    }

    private bool TweenExists(Transform target)
    {
        foreach (var tween in _activeTweens)
            if (tween.Target.Equals(target))
                return true;
        return false;
    }

    // Returns the active tween for that object (returns null if doesn't exist)
    private Tween GetActiveTween(Transform target)
    {
        foreach (var tween in _activeTweens)
            if (tween.Target.Equals(target))
                return tween;
        return null;
    }


    private void MergeTweenLists()
    {
        foreach (var tween in _activeTweensToAdd)
            _activeTweens.Add(tween);
        foreach (var tween in _activeTweensToRemove)
            _activeTweens.Remove(tween);

        _activeTweensToAdd.Clear();
        _activeTweensToRemove.Clear();
    }

    private static void UpdateTween(Tween tween)
    {
        var timeFraction = (Time.time - tween.StartTime) / tween.Duration;
        switch (tween.Type)
        {
            case Tween.TweenType.Linear:
                tween.Target.position = Vector3.Lerp(tween.StartPos, tween.EndPos, timeFraction);
                break;
            case Tween.TweenType.Quadratic:
                tween.Target.position = QuadEase(tween.StartPos, tween.EndPos, timeFraction);
                break;
            case Tween.TweenType.Cubic:
                tween.Target.position = CubicEase(tween.StartPos, tween.EndPos, timeFraction);
                break;
            default:
                tween.Target.position = Vector3.Lerp(tween.StartPos, tween.EndPos, timeFraction);
                break;
        }
    }

    public static Tweener GetCurrent()
    {
        // Swap this out depending on where the tweener component is actually held
        return GameObject.FindWithTag("Managers").GetComponent<Tweener>();
    }


    private static Vector3 QuadEase(Vector3 a, Vector3 b, float t)
    {
        return Vector3.Lerp(a, b, Mathf.Pow(t, 2));
    }

    private static float QuadEase(float a, float b, float t)
    {
        return Mathf.Lerp(a, b, Mathf.Pow(t, 2));
    }

    private static Vector3 CubicEase(Vector3 a, Vector3 b, float t)
    {
        t = (--t) * t * t + 1; // Note: Eases out
        return Vector3.Lerp(a, b, t);
    }

    private static float CubicEase(float a, float b, float t)
    {
        t = (--t) * t * t + 1; // Note: Eases out
        return Mathf.Lerp(a, b, t);
    }
}