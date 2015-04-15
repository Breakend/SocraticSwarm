using UnityEngine;
using System.Collections;

public class AgentUpdate {

	public UpdateMessage lastUpdate;
	public float distance;

	public void CleanUp(){
		this.lastUpdate = null;
	}

	public AgentUpdate (UpdateMessage lastUpdate) {
		this.lastUpdate = new UpdateMessage(lastUpdate.position, lastUpdate.velocity, lastUpdate.accleration, lastUpdate.desiredPosition, lastUpdate.goalTilePosition);

	}

	public void UpdatePredictedCurrentState (float time) {
		//This is just ripped from the flight controller to align the two
		// TODO: could be refactored so that there is a function to generate this that both can use
		float distanceToGoal = -1f;
		if (lastUpdate.desiredPosition == null) return;
		if (lastUpdate.desiredPosition == Vector3.zero) {
			this.lastUpdate.position += time * lastUpdate.velocity;		
		}

		if(lastUpdate.goalTilePosition != Vector3.zero) distanceToGoal = Vector3.Distance(lastUpdate.goalTilePosition, lastUpdate.position);

		Vector3 accleration = this.lastUpdate.accleration;
		Vector3 velocity = this.lastUpdate.velocity;

		if (distanceToGoal >= 0 && distanceToGoal < 100) {
			float factor = distanceToGoal / 100;
			factor = Mathf.Max(factor, 0.6f);
			accleration *=  factor;

			velocity += accleration*time*50;
			if (velocity.magnitude > (factor * NewFlightController.MaxVelocity) ) {
				velocity = factor * NewFlightController.MaxVelocity * velocity.normalized;
			}
		} else {
			velocity += accleration*time*50;
			if (velocity.magnitude > NewFlightController.MaxVelocity) {
				velocity = NewFlightController.MaxVelocity * velocity.normalized;
			}
		}
		this.lastUpdate.accleration = accleration;
		this.lastUpdate.velocity = velocity;
		this.lastUpdate.position += time * velocity;
	}

}
