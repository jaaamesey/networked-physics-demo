// James Karlsson 13203260

using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public float Damage = 30f;
    public float EnemyDamage = 10f;
    public Transform[] IgnoredTransforms;
    public PlayerCharacter ParentCharacter;
    public float PhysicsBodyDamage = 100f;

    private void OnTriggerEnter(Collider other)
    {
        if (ParentCharacter != null && !ParentCharacter.hasAuthority)
            return; // Let owning player handle it

        // Return if set to ignore transform

        foreach (var ignoredTransform in IgnoredTransforms)
            if (other.transform == ignoredTransform)
                return;

        // If zombie
        var zombie = other.gameObject.GetComponent<EnemyController>();
        if (zombie != null)
        {
            // Get direction between player and zombie and apply force in said direction
            var direction = (zombie.transform.position - transform.position).normalized;
            if (ParentCharacter != null)
                ParentCharacter.CmdObtainAuthority(other.gameObject);
            if (!other.isTrigger)
            {
                zombie.ApplyForce(20 * direction);

                // Disable navigation mesh agent to allow for falling off
                zombie.DisableAgent();
                zombie.EnableAgentAfterSeconds(2.3f);
            }

            return;
        }

        // Player stuff is now handled by the player's client in PlayerCharacter.cs

        // Handle regular physics objects 
        var playerCharacter = other.gameObject.GetComponent<PlayerCharacter>();
        if (playerCharacter == null)
        {
            // Hit physics object
            var rb = other.attachedRigidbody;

            // Return if hitbox has no parent character or object is not a rigidbody
            if (ParentCharacter == null || rb == null)
                return;

            var direction = (rb.position - transform.position).normalized;
            ParentCharacter.CmdObtainAuthority(other.gameObject);
            rb.AddForce(direction * PhysicsBodyDamage, ForceMode.Impulse);
        }
    }
}