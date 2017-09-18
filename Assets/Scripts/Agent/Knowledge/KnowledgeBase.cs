using UnityEngine;
using System.Collections;
using System;

public class KnowledgeBase  {

	public Agent agent;
	public Queue incoming;
	public ArrayList updates = new ArrayList ();
	public Sender sender;

	private Hashtable agentStates;
	private float timeToNextUpdate = 0;
	private const float T_UPDATE = 0.25f;
	private const bool FILTER_RANGE_PACKETS = false;
	private const bool RANDOM_DROPS = false;
	private System.Random random = new System.Random ();

	// Per
	//https://ai2-s2-pdfs.s3.amazonaws.com/51a0/0d715c49ead0c6c438cb904285c3f1a891b6.pdf
	// have a 200 meter range, with fuzzy droppout after 120 meters

	private const float MESH_NETWORK_RANGE = 200; 
	private ArrayList agents;

	private BidKnowledge bidKnowledge;
	private SearchTile tempGoal = null;
	private SearchTile goalTile = null;

	private Vector3 baseStationPosition;

	public void CleanUp(){
		agent = null;
		incoming.Clear ();
		incoming = null;
		foreach(AgentUpdate a in updates){
			a.CleanUp();
		}
		updates.Clear ();
		updates = null;
		sender.CleanUp ();
		sender = null;
		agentStates.Clear ();
		agentStates = null;
		if(agents != null) agents.Clear ();
		agents = null;
		bidKnowledge.CleanUp ();
		tempGoal = null;
		goalTile = null;
	}

	public Vector3 BaseStationPosition{
		get{ return baseStationPosition; }
	}

	public void SetGoalTile(SearchTile g){
		this.goalTile = g;
	}

	public void SetTempGoal(SearchTile g){
		this.tempGoal = g;
	}

	public SearchTile TempGoal{
		get { return this.tempGoal;}
	}

	public SearchTile GoalTile{
		get { return goalTile; }
	}

	public void ResetGoalTile(){
		goalTile = null;
	}

	public KnowledgeBase (Agent agent) {
		this.agent = agent;
		this.incoming = new Queue ();
		this.agentStates = new Hashtable ();
		this.sender = new Sender(agent);
		this.bidKnowledge = new BidKnowledge (WorldStateManager.zone, this.sender, this.agent.ID, this);
		//TODO: can make this variable so can have multiple base stations
		baseStationPosition = WorldStateManager.baseStationPosition;
	}

	public SearchTile GetCurrentGoal() {
		return this.goalTile;
	}

	public void SetCurrentGoalAsSearched () {
		this.sender.SendSearched (goalTile.id);
		this.bidKnowledge.MarkTileAsSearched (goalTile);
		this.bidKnowledge.ResetCandidateAndGoalTile ();
	}

	// Update is called once per frame
	public void UpdateKnowledgeBase () {

		bool newAgentUpdate = false;

		/* Process every incoming message */
		while (incoming.Count != 0) {
			Packet p = (Packet) incoming.Dequeue ();
			if(p.src.ID == this.agent.ID){
				// Debug.Log("ERROR: Received our own message");
				continue;
			}
			//Filter the packet out
			if(FILTER_RANGE_PACKETS && Vector3.Distance(p.src.sensorModule.gps.position, this.agent.sensorModule.gps.position) > MESH_NETWORK_RANGE){
				continue;
			}

			if (RANDOM_DROPS && random.NextDouble () > 0.9) {
				continue; // 10% chance of dropping packets randomly.
			}

			if (p.message.type == Message.MessageType.UPDATE) {
				this.ProcessAgentUpdate (p);
				newAgentUpdate = true;
			} else if (p.message.type == Message.MessageType.BID) {
				this.ProcessBid (p); // Debug.Log("PROCESSING BID");
			} else if (p.message.type == Message.MessageType.CLAIM) {
				this.ProcessClaim (p);
			} else if (p.message.type == Message.MessageType.SEARCHED) {
				this.ProcessSearched (p);
			}
			p.CleanUp();
		}

		/* If a new agent update has been recieved, the list should be resorted */
		if (newAgentUpdate) {
			updates = new ArrayList (agentStates.Values);
			// Get the list of updates and calculate each agents distance to the desired pos
			foreach (AgentUpdate update in updates) {
				update.distance = Vector3.Distance (this.agent.sensorModule.gps.position, update.lastUpdate.position);
			}

			// Sort the list
			updates.Sort (new AgentUpdateComparer ());
		}

		/* Update bidding knowledge */
		this.bidKnowledge.Update ();

	}

	private void SendBidForNextBestTile () {
		SearchTile tile = this.bidKnowledge.GetNextBestTile (agent.sensorModule.gps.position, this.updates);
		// Check if there are any tiles left to be claimed
		if (tile == null) {
			//isDone = true;
			// if (this.bidKnowledge.IsMapSearched () ) { Debug.Log ("Agent "+this.agent.ID+" thinks the map is searched"); }
			this.bidKnowledge.ResetCandidateAndGoalTile();
			return;
		}
		this.ResetGoalTile ();
		this.tempGoal = tile;
		this.bidKnowledge.SetCandidateTile(tile);
		int tileId = tile.id;
		float bidAmount = this.bidKnowledge.BidEvaluation (agent.sensorModule.gps.position, tile.center, this.updates);

		SearchTileBid canidateBid = new SearchTileBid (this.agent.ID, bidAmount);
		this.bidKnowledge.SetCandidateBid (canidateBid);
		this.bidKnowledge.AddBidForTile (tileId, canidateBid);
		this.sender.SendBid (tileId, bidAmount);
	}

	public void SendAgentUpdate () {
		/* Check if the other agents should be updated */
		timeToNextUpdate -= 1 * Time.deltaTime;
		if (timeToNextUpdate < 0) {
			Vector3 goalPosition = this.goalTile != null ? this.goalTile.center : Vector3.zero;
			this.sender.SendUpdate (agent.sensorModule.gps.position, agent.flightController.velocity, agent.flightController.accleration, agent.flightController.destination, goalPosition);
			timeToNextUpdate = T_UPDATE;
		}
	}

	private void ProcessAgentUpdate (Packet p) {
		agentStates.Remove (p.src.ID);
		AgentUpdate agentUpdate = new AgentUpdate ((UpdateMessage)p.message);
		agentUpdate.UpdatePredictedCurrentState (WorldStateManager.AVE_COMM_DELAY);
		agentStates.Add (p.src.ID, agentUpdate);
	}

	public void ComputeNextHorizon (float time) {
		foreach (AgentUpdate update in this.updates) {
			update.UpdatePredictedCurrentState (time);
			update.distance = Vector3.Distance (this.agent.sensorModule.gps.position, update.lastUpdate.position);
		}
		updates.Sort (new AgentUpdateComparer ());
	}

	private void ProcessBid (Packet p) {
		BidMessage bidMsg = (BidMessage) p.message;
//		Debug.Log ("Processing bid for Agent " + p.src.ID + " on tile " + bidMsg.tileId + " with amount " + bidMsg.bidAmount);
		/* Determine if the agent should bid for a different tile */
		SearchTileBid recvBid = new SearchTileBid (p.src.ID, bidMsg.bidAmount);
		if (this.goalTile == null && this.bidKnowledge.CandidateTile != null && bidMsg.tileId == this.bidKnowledge.CandidateTile.id) { //&& this.canidateBid != null
			// If the recv bid is better than my bid, reset
			if (recvBid.IsBetterThan(this.bidKnowledge.CandidateBid)) {
				this.bidKnowledge.CandidateTile.SetAsClaimed(recvBid);
				this.bidKnowledge.ResetCandidateAndGoalTile();
			}
		} else if (this.goalTile != null && bidMsg.tileId == this.goalTile.id) {
			// If the recv bid is better than my claimed bid, give up on that tile and assert that the other
			//
			if (recvBid.IsBetterThan(this.goalTile.acceptedBid)) {
				this.goalTile.acceptedBid = recvBid;
				this.bidKnowledge.ResetCandidateAndGoalTile();
//				this.sender.SendClaimed (bidMsg.tileId, recvBid.agentId, recvBid.value);
			}
		}
		bidKnowledge.AddBidForTile (bidMsg.tileId, new SearchTileBid (p.src.ID, bidMsg.bidAmount));
	}

	private void ProcessClaim (Packet p) {
		ClaimMessage claimMsg = (ClaimMessage) p.message;
		this.bidKnowledge.RecvClaimedMessage (p.src.ID, claimMsg.tileId, claimMsg.agentId, claimMsg.bestBidAmount);
	}

	private void ProcessSearched (Packet p) {
		SearchedMessage searchedMsg = (SearchedMessage) p.message;
		this.bidKnowledge.RecvSearchedMessage (p.src.ID, searchedMsg.tileId);
	}

	/* Sorts agent updates by distances */
	class AgentUpdateComparer : IComparer  {
		int IComparer.Compare ( object x, object y )  {
			float xdist = ((AgentUpdate) x).distance;
			float ydist = ((AgentUpdate) y).distance;
			if (xdist > ydist) return 1;
			else if (xdist < ydist) return -1;
			return 0;
		}
	}
}


