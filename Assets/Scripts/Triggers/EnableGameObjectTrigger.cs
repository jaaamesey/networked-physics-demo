// James Karlsson 13203260

using UnityEngine;

public class EnableGameObjectTrigger : MonoBehaviour
{
    public bool DisableInsteadOfEnable;
    public GameObject[] ObjectsToEnable;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        foreach (var go in ObjectsToEnable)
            go.SetActive(!DisableInsteadOfEnable);
    }
}