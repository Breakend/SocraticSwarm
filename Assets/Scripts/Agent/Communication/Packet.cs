using UnityEngine;
using System.Collections;

public class Packet{
	public Agent src;
	public Agent dst; 
	public Message message;
	
	public float sendTime;
	
	public Packet (Agent src, Agent dst, Message message) {
		this.src = src;
		this.dst = dst; 
		this.message = message;
	}

	public void CleanUp(){
		src = null;
		dst = null;
		message = null;
	}
}

