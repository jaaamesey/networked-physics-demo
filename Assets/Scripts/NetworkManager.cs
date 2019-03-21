// James Karlsson 13203260

using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class NetworkManager : UnityEngine.Networking.NetworkManager
{
    public override void OnServerDisconnect(NetworkConnection connectionToClient)
    {
        ClearObjectAuthority(connectionToClient.clientOwnedObjects);

        // Do any other ordinary disconnection stuff
        base.OnServerDisconnect(connectionToClient);
    }

    // Sets ownership of all a client's owned objects to the host
    public void ClearObjectAuthority(HashSet<NetworkInstanceId> clientOwnedObjects)
    {
        if (clientOwnedObjects.Count <= 0)
            return;

        // Create a copy of the list to prevent any issues with deletion
        var ownedObjects = clientOwnedObjects.ToList();

        foreach (var ownedObject in ownedObjects)
        {
            var ownedGameObject = NetworkServer.FindLocalObject(ownedObject);

            // Kill any PlayerCharacter and PlayerConnection objects in the list
            if (ownedGameObject.GetComponent<PlayerCharacter>() != null ||
                ownedGameObject.GetComponent<PlayerConnection>() != null)
            {
                Destroy(ownedGameObject);
                continue; // Skip to next owned object
            }

            // Give authority back to host for non-player related objects
            var newConnectionToClient = GameManager.GetCurrent().GetPlayerConnection(0).connectionToClient;
            var objectNetworkIdentity = ownedGameObject.GetComponent<NetworkIdentity>();
            objectNetworkIdentity.RemoveClientAuthority(objectNetworkIdentity.clientAuthorityOwner);
            objectNetworkIdentity.AssignClientAuthority(newConnectionToClient);
            print("Authority assigned to " + newConnectionToClient + " for " + ownedGameObject);
        }
    }
}