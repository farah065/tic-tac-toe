using UnityEngine;
using Mirror;
using Mirror.Discovery;
using System.Net;
using System;

public struct DiscoveryRequest : NetworkMessage
{
    // Add public fields (not properties) for whatever information you want
    // sent by clients in their broadcast messages that servers will use.
}

public struct DiscoveryResponse : NetworkMessage
{
    // Add public fields (not properties) for whatever information you want the server
    // to return to clients for them to display or use for establishing a connection.
    public int CurrentPlayers;
    public string HostPlayerName;
    public Uri Uri;
    public long ServerId;
    public IPEndPoint EndPoint { get; set; }
}

public class NetworkDiscoveryTicTacToe : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        base.ProcessClientRequest(request, endpoint);
    }

    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        // TODO: Create your response and return it
        return new DiscoveryResponse
        {
            CurrentPlayers = NetworkServer.connections.Count,
            HostPlayerName = GetHostPlayerName(),
            Uri = transport.ServerUri(),
            ServerId = ServerId
        };
    }

    protected override DiscoveryRequest GetRequest()
    {
        return new DiscoveryRequest();
    }

    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
        response.EndPoint = endpoint;

        // although we got a supposedly valid url, we may not be able to resolve
        // the provided host
        // However we know the real ip address of the server because we just
        // received a packet from it,  so use that as host.
        UriBuilder realUri = new UriBuilder(response.Uri)
        {
            Host = response.EndPoint.Address.ToString()
        };
        response.Uri = realUri.Uri;

        OnServerFound.Invoke(response);
    }

    private string GetHostPlayerName()
    {
        // Assuming host is the first connection/player
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                var player = conn.identity.GetComponent<NetworkRoomPlayerTicTacToe>(); // your custom player class
                if (player != null)
                    return player.PlayerName; // adjust field name as needed
            }
        }

        return "Unknown";
    }

}
