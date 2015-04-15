using UnityEngine;
using System.Collections;

public class Message {
	
	public enum MessageType { UPDATE, BID, CLAIM, SEARCHED };
	public MessageType type;
	
	public Message (MessageType type) {
		this.type = type;
	}
	
}

