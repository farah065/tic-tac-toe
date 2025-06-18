using Mirror;
using UnityEngine;

public class NetworkPlayerBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int PlayerId;
}
