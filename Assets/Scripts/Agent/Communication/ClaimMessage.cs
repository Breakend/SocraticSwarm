using UnityEngine;
using System.Collections;

public class ClaimMessage : Message {
	
	public int tileId;
	public int agentId;
	public float bestBidAmount;
	
	public ClaimMessage (int tileId, int agentId, float bestBidAmount) : base(MessageType.CLAIM) {
		this.tileId = tileId;
		this.agentId = agentId;
		this.bestBidAmount = bestBidAmount;
	}
	
}

