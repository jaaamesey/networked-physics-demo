// James Karlsson 13203260
 
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GoalTrigger : NetworkBehaviour
{
    [SerializeField] private TextMeshPro _textMesh;
    [SyncVar] public int SvrScore;

    private void Update()
    {
        // Might have performance implications but was the fastest and safest way to sync properly
        _textMesh.text = SvrScore.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.gameObject.name != "basketball")
            return;

        CmdIncrementScore();
    }

    [Command]
    private void CmdIncrementScore()
    {
        SvrScore += 1;
    }
}