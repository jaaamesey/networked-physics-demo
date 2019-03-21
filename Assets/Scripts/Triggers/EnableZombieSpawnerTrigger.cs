using UnityEngine;

public class EnableZombieSpawnerTrigger : MonoBehaviour
{
    public bool DisableInsteadOfEnable;

    public ZombieSpawner[] SpawnersToEnable;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        foreach (var enemyComp in SpawnersToEnable)
            enemyComp.enabled = !DisableInsteadOfEnable;
    }
}