using UnityEngine;
using System.Collections;

public class BidMessage : Message {
	
	public int tileId;
	public float bidAmount;
	
	public BidMessage (int tileId, float bidAmount) : base(MessageType.BID) {
		this.tileId = tileId;
		this.bidAmount = bidAmount;
	}
	
}

