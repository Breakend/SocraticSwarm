using UnityEngine;
using System.Collections;
using NumUtils.NelderMeadSimplex;

public class Agent : MonoBehaviour
{
	public NewFlightController flightController;
	public KnowledgeBase knowledgeBase;
	private SensorModule sModule;
	public IDecisionManager decisionManager;


	public GameObject explosion; // drag your explosion prefab here

	// Keeping track of things
	//public Tile spawnLocation;
	private float currentAltitude = 0;
	public int ID;

	private bool hasCollided = false;
	private float headingUpdateTimer = 0;
	private const float HEADING_UPDATE_TIME = .05f;

	public SensorModule sensorModule{
		get{ return this.sModule;}
	}

	public void CleanUp(){
		if(flightController != null)
			flightController.CleanUp ();
		if(knowledgeBase != null)
			knowledgeBase.CleanUp();
		if(sModule != null)
			sModule.CleanUp ();
		if(decisionManager != null)
			decisionManager.CleanUp ();
		flightController = null;
		knowledgeBase = null;
		sModule = null;
		decisionManager = null;
	}

	void OnDestroy()
	{
		CleanUp ();
	}

	// Use this for initialization
	void Start () {
		WorldStateManager.currentAgents.Add(this);
//		Debug.Log ("ID = " + ID);
		knowledgeBase = new KnowledgeBase (this);
		this.sModule = new SensorModule (this);
		decisionManager = new SwarmOpsDecisionManager (knowledgeBase, sensorModule);
		flightController = new NewFlightController (this.gameObject, this.sensorModule);
	}

	// Explosion on Collision
	void OnCollisionEnter(){
		if(hasCollided) return;

		hasCollided = true;
//		this.knowledgeBase.incoming.Clear ();
//		this.knowledgeBase.updates.Clear ();
		// GameObject expl = Instantiate(explosion, this.sensorModule.gps.position, Quaternion.identity) as GameObject;
		WorldStateManager.currentAgents.Remove (this);
		Destroy(gameObject); // destroy the agent
		// Destroy(expl, 3); // delete the explosion after 3 seconds
		WorldStateManager.AlertCollision ();
	}

	// Should we update the current heading, this will eventually be
	// if we have an update from the motor controller in the C++ code
	// Right now, just return true
	private bool shouldUpdateHeading() {
		headingUpdateTimer -= Time.deltaTime;
		if (headingUpdateTimer < 0) {
			headingUpdateTimer = HEADING_UPDATE_TIME;
			return true;
		}
		return false;
	}

	/*
	 * 	Core agent update function.
	 */
	void Update () {
		if(this.sensorModule == null || this.knowledgeBase == null) return;
		float distToGoal = -1;

		if(WorldStateManager.CONTROL){
			// This is control logic for a simple test to compare algos against
			// here for now because logic was tightly coupled
			// TODO: extract this into separate agent
			this.sensorModule.Update ();
			if(knowledgeBase.GetCurrentGoal() == null){
				Tile t = WorldStateManager.GetControlTileForAgent(this.ID);
				if(t == null){
					// Debug.Log("Tile is null");
					//No goal so just chill out
					flightController.SetNextDestination(sensorModule.gps.position);
					this.flightController.UpdateMotion (1f);
					return;
				}
				knowledgeBase.SetGoalTile(new SearchTile(t.GetID(), t.GetCenter()));
			}
			this.flightController.UpdateMotion(distToGoal);
			flightController.SetNextDestination(knowledgeBase.GetCurrentGoal().center);
			Debug.DrawLine( this.sensorModule.gps.position, new Vector3(this.knowledgeBase.GetCurrentGoal().center.x, this.knowledgeBase.GetCurrentGoal().center.y-40f, this.knowledgeBase.GetCurrentGoal().center.z), Color.black);

			distToGoal = this.GetDistanceToGoal (this.sensorModule.gps.position, knowledgeBase.GetCurrentGoal ().center);
			if (distToGoal < 5.1f) {
				WorldStateManager.MarkTileAsSearched (knowledgeBase.GetCurrentGoal ());
				knowledgeBase.SetCurrentGoalAsSearched ();
			}
			return;
		}
		// Update the knowledge base first
		this.sensorModule.Update ();
		this.knowledgeBase.UpdateKnowledgeBase ();

		// Check if the agent is in range of the tile
		SearchTile goalTile = knowledgeBase.GetCurrentGoal ();
		if (goalTile != null) {
			distToGoal = this.GetDistanceToGoal (this.sensorModule.gps.position, goalTile.center);
			if (distToGoal < 5.1f) {
				WorldStateManager.MarkTileAsSearched (goalTile);
				knowledgeBase.SetCurrentGoalAsSearched ();
			}
		}

		//This should be used with horizons and stuff
		if (shouldUpdateHeading()) {
			this.knowledgeBase.ComputeNextHorizon (HEADING_UPDATE_TIME);
			Vector3 nextPoint = decisionManager.getNextDesiredPoint();
			// Debug.DrawLine( new Vector3(nextPoint.x, nextPoint.y - 50, nextPoint.z), new Vector3(nextPoint.x, nextPoint.y + 20, nextPoint.z));
			flightController.SetNextDestination(nextPoint);
		}

		// Update position
		this.flightController.UpdateMotion (distToGoal);
		// if(this.knowledgeBase.GetCurrentGoal() != null)
			// Debug.DrawLine( this.sensorModule.gps.position, new Vector3(this.knowledgeBase.GetCurrentGoal().center.x, this.knowledgeBase.GetCurrentGoal().center.y-40f, this.knowledgeBase.GetCurrentGoal().center.z), Color.black);
		this.knowledgeBase.SendAgentUpdate ();
		// if(this.ID == 0){
			foreach(AgentUpdate upd in this.knowledgeBase.updates){
				if(Vector3.Distance(this.sensorModule.gps.position, upd.lastUpdate.position) < 200)
					Debug.DrawLine(this.sensorModule.gps.position, upd.lastUpdate.position, Color.black);
			}
		// }
	}

	private float GetDistanceToGoal (Vector3 a, Vector3 b) {
		a.y = b.y; //Don't care about altitude so much. TODO maybe change this
		return Vector3.Distance (a, b);
	}

	private bool AgentInRange(Vector3 a, Vector3 b, float distance) {
		a.y = b.y; //Don't care about altitude so much. TODO maybe change this
		if (Vector3.Distance(a,b) < distance) {
			return true;
		}
		return false;
	}
}

