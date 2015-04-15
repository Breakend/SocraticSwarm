using UnityEngine;
using System.Collections;

public class SearchedMessage : Message {
	
	public int tileId;
	
	public SearchedMessage (int tileId) : base(MessageType.SEARCHED) {
		this.tileId = tileId;
	}
	
}

