using UnityEngine;
using System.Collections;

public class WorldStateManager : MonoBehaviour {

	public static RectZone zone;
	public static ArrayList currentAgents = new ArrayList();

	public static Queue packetQueue = new Queue ();  // Agents put packets in this queue
	private static ArrayList packets  = new ArrayList(); // Used by the WSM to keep packets until they are sent

	private ArrayList toRm = new ArrayList ();
	public GameObject agentPrefab;
	private static int AgentsToGenerate = 5;

	public static bool trialOngoing = false;
	public static bool taskCompleted = false;
	public static int numberOfCollisions = 0;
	public static int numberOfTilesSearched = 0;
	public static bool CONTROL = false;
	public static float AVE_COMM_DELAY = 0f;


	public static int GetAgentsToGenerate{
		get{ return AgentsToGenerate;}
	}

	//in seconds
	public static float timeElapsed = 0.0f;

	public static Vector3 baseStationPosition;

	void Start(){
		baseStationPosition = this.transform.position;
	}

	//TODO: need to refactor this to split off some logic into TrialRunner and some logic here
	// Need to decide which goes where

	public Vector2[] gridCorners;
	private float tileSize;

	public static System.Type[] GetListOfCostsToUse(){
		return TrialRunner.costsToUse;
	}

	public static int GetNumberOfTiles(){
		return zone.tiles.Length;
	}

	public static void AlertCollision(){
		if (trialOngoing) {
			numberOfCollisions += 1;
		}
	}

	public static Tile GetControlTileForAgent(int agentID){
		foreach (Tile tile in zone.tiles) {
			float maxspot = zone.minZ + (zone.maxZ - zone.minZ)/(AgentsToGenerate)*(agentID+1);
			float minspot = zone.minZ + (zone.maxZ - zone.minZ)/(AgentsToGenerate)*(agentID);
			if(tile.GetCenter().z < maxspot && tile.GetCenter().z > minspot && !tile.IsSearched()){
				return tile;
			}
		}
		return null;
	}

	public static void EndTrial(){
		trialOngoing = false;
		TrialRunner.GetSingletonInstance ().EndTrial ();

		Debug.Log ("Trial Ending with time: " + timeElapsed);
		Debug.Log ("Sustained " + numberOfCollisions + " collisions");

		// If any agents exist destroy them
		foreach(Agent a in currentAgents) {
			Destroy(a.gameObject);
		}

		if (zone != null) {
			zone.DestroyTiles();
			zone.CleanUp();
			zone = null;
		}

		currentAgents.Clear ();
		foreach (Packet p in packetQueue) {
			p.CleanUp();
		}
		packetQueue.Clear ();
		foreach (Packet p in packets) {
			p.CleanUp();
		}
		packets.Clear ();

		//Callback to trial runner
		TrialRunner.GetSingletonInstance().LaunchNextTrial ();
	}

	public void RunTrial(){
		trialOngoing = true;
		taskCompleted = false;

		/*
		 * Reset agents and other stuff
		 */

		//Reset collisions for this run
		numberOfCollisions = 0;
		timeElapsed = 0.0f;
		numberOfTilesSearched = 0;

		// If any agents exist destroy them
		foreach(Agent a in currentAgents) {
			Destroy(a.gameObject);
		}

		currentAgents.Clear ();
		foreach (Packet p in packetQueue) {
			p.CleanUp();
		}
		packetQueue.Clear ();
		foreach (Packet p in packets) {
			p.CleanUp();
		}
		packets.Clear ();


		if (zone != null) {
			zone.DestroyTiles();
			zone.CleanUp();
			zone = null;
		}

		GenerateGridFromCurrentCorners ();

//		Debug.Log ("Packetqueue Count: " +packetQueue.Count);
//		Debug.Log ("Packets Count: " + packets.Count);

		/*
		 *  Spawn agents and necesssary things
		 */
		// TODO: change starting positions such that they take off and maybe so they're actually on the base station
		 int j = 0;
		for (int i=0; i< AgentsToGenerate; i++) {
			Vector3 pos = this.transform.position;
			if(i % 2 == 0) j++;
			pos.y += (i % 2)*100 + 300;
			pos.x += j*50;
			pos.z += j*50;

			GameObject go = Instantiate(agentPrefab, pos, Quaternion.Euler(0, 180, 0)) as GameObject;
			go.GetComponent<Agent>().ID = i;

		}
	}

	public void GenerateGridFromCurrentCorners(){
		float minX, maxX, minZ, maxZ;

		minX = maxX = this.gridCorners[0].x;
		minZ = maxZ = this.gridCorners[0].y;

		for (int i=0; i<this.gridCorners.Length; i++) {
			if (this.gridCorners [i].x < minX)
				minX = this.gridCorners [i].x;
			if (this.gridCorners [i].x > maxX)
				maxX = this.gridCorners [i].x;
			if (this.gridCorners [i].y < minZ)
				minZ = this.gridCorners [i].y;
			if (this.gridCorners [i].y > maxZ)
				maxZ = this.gridCorners [i].y;
		}

		zone = new RectZone (minX, minZ, maxX, maxZ, this.tileSize);
	}

	public void SetGrid(Vector2[] newCorners, float tileSize){
		if (newCorners.Length != 4)
			throw new ImproperSearchBoundsException ("Must have 4 corners to the search area for now (also they must be a rectangle)");

		this.gridCorners = newCorners;
		this.tileSize = tileSize;

	}

	public static void MarkTileAsHasBid (SearchTile searchTile) {
		foreach (Tile tile in zone.tiles) {
			if (tile.GetID () == searchTile.id) {
				if (tile.IsSearched ()) {
					// Debug.Log("DEBUG | Trying to bid on an already searched tile");
					return;
				} else if(tile.IsClaimed ()) {
					// Debug.Log("DEBUG | Trying to bid on an already claimed tile");
				}
				tile.SetAsHasBid ();
			}
		}
	}

	public static void MarkTileAsOpen (SearchTile searchTile){
		foreach (Tile tile in zone.tiles) {
			if (tile.GetID () == searchTile.id) {
				tile.SetAsOpen();
			}
		}
	}

	public static void MarkTileAsClaimed (SearchTile searchTile) {
		foreach (Tile tile in zone.tiles) {
			if (tile.GetID () == searchTile.id) {
				if (tile.IsSearched ()) {
					// Debug.Log("DEBUG | TRYING TO CLAIM A SEARCHED TILE");
					return;
				}
				// if(tile.IsClaimed ()) { Debug.Log("ERROR: Trying to claim an already claimed tile"); }
				tile.SetAsClaimed ();
			}
		}
	}

	public static bool IsSearched (SearchTile searchTile) {
		foreach (Tile tile in zone.tiles) {
			if (tile.GetID () == searchTile.id) {
				return tile.IsSearched ();
			}
		}
		// Debug.Log("ERROR | Cannot find world tile.");
		return false;
	}

	public static void MarkTileAsSearched (SearchTile searchTile) {
		bool allSearched = true;
		foreach (Tile tile in zone.tiles) {
			if (tile.GetID () == searchTile.id) {
				if(!tile.IsSearched())
					numberOfTilesSearched++;

				tile.SetAsSearched ();
			}
			if(!tile.IsSearched()) allSearched = false;
		}

		if (allSearched) {
			//WorldStateManager.EndTrial ();
			taskCompleted = true;
		}
	}

	public static Tile GetNextSearchSpot(Vector3 pos){
		Tile g = zone.ClosestUnsearchedTile(pos);
		return g;
	}

	private float GetCommunicationDelay () {
		return AVE_COMM_DELAY;
	}

	// Update is called once per frame
	void Update () {
		if(!trialOngoing)
		{
			return;
		}

		timeElapsed += Time.deltaTime;

		//Trial timeout
		if (timeElapsed >= TrialRunner.MAX_TRIAL_RUN_TIME || taskCompleted || 0 == currentAgents.Count) {
			Debug.Log ("Timer expired for trial");
			if(0 == currentAgents.Count){
		//		Debug.Log ("Well... all the drones exploded... so that sucks...");
			}
			WorldStateManager.EndTrial ();
			return;
		}






		foreach (Packet p in packets) {
			p.sendTime -= 1 * Time.deltaTime;
			if (p.sendTime <= 0) {
				// If the packet has a null destination, send to all agents
				if (p.dst == null) {
					foreach (Agent a in currentAgents) {
						// Check if src and dst are not the same
						if (p.src.Equals (a)) continue;
						a.knowledgeBase.incoming.Enqueue (new Packet(p.src, a, p.message));
						toRm.Add (p);
					}
				}
			}
		}

		foreach (Packet p in toRm) {
			packets.Remove (p);
			p.CleanUp();
		}
		toRm.Clear ();


		while (packetQueue.Count != 0) {
			Packet p = (Packet) packetQueue.Dequeue ();
			p.sendTime = GetCommunicationDelay();
			packets.Add (p);
		}

	}
}

/*
	From Unity's reference manual:
    By default, the Awake, OnEnable and Update functions of different scripts are called
	in the order the scripts are loaded (which is arbitrary). However, it is possible to
	modify this order using the Script Execution Order settings.
*/

