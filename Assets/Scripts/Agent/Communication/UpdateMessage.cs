using UnityEngine;
using System.Collections;

public class UpdateMessage : Message {

	public Vector3 position;
	public Vector3 velocity;
	public Vector3 accleration;
	public Vector3 goalTilePosition;
	public Vector3 desiredPosition;

	public UpdateMessage (Vector3 position, Vector3 velocity, Vector3 accleration, Vector3 desiredPosition, Vector3 goalTilePosition) : base(MessageType.UPDATE) {
		this.position = position;
		this.velocity = velocity;
		this.accleration = accleration;
		this.desiredPosition = desiredPosition;
		this.goalTilePosition = goalTilePosition;
	}

}



