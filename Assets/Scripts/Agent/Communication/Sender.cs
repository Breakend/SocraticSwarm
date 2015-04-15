using UnityEngine;
using System.Collections;

public class Sender {

	private Agent agent;

	public void CleanUp(){
		agent = null;
	}

	public Sender (Agent agent) {
		this.agent = agent;
	}

	public void SendUpdate (Vector3 position, Vector3 velocity, Vector3 accleration, Vector3 desiredPosition, Vector3 goalTilePosition) {
		this.SendAllMessage (new UpdateMessage (position, velocity, accleration, desiredPosition, goalTilePosition));
	}

	public void SendBid (int tileId, float bidAmount) {
		this.SendAllMessage (new BidMessage (tileId, bidAmount));
	}

	public void SendClaimed (int tileId, int agentId, float bestBidAmount) {
		this.SendAllMessage (new ClaimMessage (tileId, agentId, bestBidAmount));
	}

	public void SendSearched (int tileId) {
		this.SendAllMessage (new SearchedMessage (tileId));
	}

	private void SendAllMessage (Message m) {
		Packet p = new Packet (this.agent, null, m);
		WorldStateManager.packetQueue.Enqueue (p);
	}

}
