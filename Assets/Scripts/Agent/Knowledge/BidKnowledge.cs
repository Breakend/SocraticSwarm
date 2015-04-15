using UnityEngine;
using System.Collections;

public class BidKnowledge {

	private int numTiles;
	private ArrayList openTiles;
	private ArrayList tilesInAuction;
	private ArrayList claimedTiles;
	private ArrayList searchedTiles;
	private Sender sender;
	private int myId;

	private SearchTileBid canidateBid = null;
	private SearchTile canidateTile = null;
	private KnowledgeBase kb;

	//TODO: convert all lists to arrays by index to improve performance
	public void CleanUp(){
		sender.CleanUp ();
		sender = null;
		canidateBid = null;
		canidateTile = null;
		kb = null;
		foreach (SearchTile s in openTiles) {
			s.CleanUp();
		}
		openTiles.Clear ();
		openTiles = null;
		foreach (SearchTile s in tilesInAuction) {
			s.CleanUp();
		}
		tilesInAuction.Clear ();
		tilesInAuction = null;
		foreach (SearchTile s in claimedTiles) {
			s.CleanUp();
		}
		claimedTiles.Clear ();
		claimedTiles = null;
		foreach (SearchTile s in searchedTiles) {
			s.CleanUp();
		}
		searchedTiles.Clear ();
		searchedTiles = null;

	}

	public BidKnowledge (RectZone zone, Sender sender, int myId, KnowledgeBase kb) {
		openTiles = new ArrayList ();
		foreach (Tile tile in zone.tiles) {
			openTiles.Add (new SearchTile (tile.GetID (), tile.GetCenter ()));
			numTiles++;
		}
		tilesInAuction = new ArrayList ();
		claimedTiles = new ArrayList ();
		searchedTiles = new ArrayList ();
		this.sender = sender;
		this.myId = myId;
		this.kb = kb;
	}

	public SearchTileBid CandidateBid{
		get{ return canidateBid;}
	}

	public SearchTile CandidateTile{
		get{ return canidateTile;}
	}

	public void SetCandidateBid(SearchTileBid b){
		this.canidateBid = b;
	}

	public void SetCandidateTile(SearchTile t){
		this.canidateTile = t;
	}

	public void ResetCandidateAndGoalTile (){
		ResetCandidateTile ();
		this.kb.ResetGoalTile ();
	}

	public void ResetCandidateTile(){
		this.canidateTile = null;
	}

	public void Update () {
		ArrayList toRm = new ArrayList ();
		foreach (SearchTile tile in tilesInAuction) {
			tile.Update ();
			if (tile.GetState () == SearchTile.TileState.CLAIMED) {
				toRm.Add (tile);
			}
		}

		foreach (SearchTile tile in toRm) {
			this.MoveTile(tile, this.tilesInAuction, this.claimedTiles);
		}

		toRm.Clear ();

		foreach (SearchTile tile in claimedTiles) {
			tile.Update ();
			if (tile.GetState () == SearchTile.TileState.OPEN) {
				toRm.Add (tile);
			}
		}

		foreach (SearchTile tile in toRm) {
//			Debug.Log ("A tile has timed out");
			this.MoveTile(tile, this.claimedTiles, this.openTiles);
			if(tile.GetWinnerAgentId() == this.kb.agent.ID){
				WorldStateManager.MarkTileAsOpen (tile);
				this.ResetCandidateAndGoalTile();
//				Debug.Log ("I'm giving up my bid");
			}

			tile.acceptedBid = null;
		}


		/* BIDDING LOGIC HERE */
		if (this.kb.GoalTile == null) {
			// If there is no tile bidded on yet
			if (this.canidateTile == null) {
				this.SendBidForNextBestTile ();
			} else {
				// If the auction has ended for the tile I bid on
				if (canidateTile.GetState () == SearchTile.TileState.CLAIMED) {
					//Debug.Log("AUCTION END");
					// Check if I was the winner
					if (canidateTile.GetWinnerAgentId () == this.kb.agent.ID) {
						this.sender.SendClaimed (canidateTile.id, canidateTile.acceptedBid.agentId, canidateTile.acceptedBid.value);
//						Debug.Log("Agent " + this.kb.agent.ID + " WON tileid = " + canidateTile.id + " bid=" + this.kb.TempGoal.acceptedBid.value);
						this.kb.SetGoalTile(this.kb.TempGoal);
						this.ResetCandidateTile();
						WorldStateManager.MarkTileAsClaimed (this.kb.GoalTile);
					} else { // Otherwise, bid on a new tile
						this.SendBidForNextBestTile ();
					}
				} else if (canidateTile.GetState () == SearchTile.TileState.SEARCHED) {
					this.SendBidForNextBestTile ();
				}
			}
		} else {
			//Debug.Log("I have a goal!");
		}

	}

	private void SendBidForNextBestTile () {
		SearchTile tile = this.GetNextBestTile (this.kb.agent.sensorModule.gps.position, this.kb.updates);
		// Check if there are any tiles left to be claimed
		if (tile == null) {
			//isDone = true;
//			Debug.Log ("No more tiles to bid on");
//			if (this.IsMapSearched () ) { Debug.Log ("Agent "+this.kb.agent.ID+" thinks the map is searched"); }
			this.ResetCandidateAndGoalTile();
			return;
		}
		this.ResetCandidateAndGoalTile ();
		this.kb.SetTempGoal( tile);
		this.canidateTile = tile;
		int tileId = canidateTile.id;
		float bidAmount = this.BidEvaluation (this.kb.agent.sensorModule.gps.position, this.canidateTile.center, this.kb.updates);

		this.canidateBid = new SearchTileBid (this.kb.agent.ID, bidAmount);
		this.AddBidForTile (tileId, this.canidateBid);
		this.sender.SendBid (tileId, bidAmount);
//		Debug.Log (this.kb.agent.ID + " Just Sent a bid");
	}

	public void AddBidForTile (int searchTileId, SearchTileBid bid) {
		/* Search for the tile in the set that is currently in auction */
		foreach (SearchTile tile in tilesInAuction) {
			if (searchTileId == tile.id) {
				// WorldStateManager.MarkTileAsHasBid (this);
				tile.AddBid (bid, this.myId == bid.agentId);
				return;
			}
		}
		/* This is the first bid for the tile */
		foreach (SearchTile tile in openTiles) {
			if (searchTileId == tile.id) {
				this.MoveTile(tile, this.openTiles, this.tilesInAuction);
				tile.AddBid (bid, this.myId == bid.agentId);
				WorldStateManager.MarkTileAsHasBid (tile);
				return;
			}
		}
		// If we can't find the tile, it's been claimed or searched
		SearchTile searchTile = this.FindTileWithId (this.claimedTiles, searchTileId);
		// If it's found as a claimed tile
		if (searchTile != null) {
			// Debug.Log("Agent " + this.myId + "  got a bid for a claimed tile");
			// Send a claimed message giving the agents belived state
			sender.SendClaimed (searchTile.id, searchTile.acceptedBid.agentId, searchTile.acceptedBid.value);
			return;
		}

		// Otherwise, the tile has been searched. Send searched message.
		searchTile = this.FindTileWithId (this.searchedTiles, searchTileId);
		if (searchTile != null) {
			// Debug.Log("Agent " + this.myId + "  got a bid for a searched tile");
			sender.SendSearched (searchTile.id);
			return;
		}
		// Debug.Log("ERROR -> Adding bid for tile that does not exist.");
	}

	public SearchTile GetNextBestTile (Vector3 currentPosition, ArrayList updates) {
		SearchTile bestTile = null;
		float highestBid = float.MinValue;
		foreach (SearchTile tile in tilesInAuction) {
			float gj = BidEvaluation(currentPosition, tile.center, updates);
			if (!IsStillBestTile (bestTile, highestBid, tile, gj)) {
				bestTile = tile;
				highestBid = gj;
			}
		}

		foreach (SearchTile tile in openTiles) {
			float gj = BidEvaluation(currentPosition, tile.center, updates);
			if (!IsStillBestTile (bestTile, highestBid, tile, gj)) {
				bestTile = tile;
				highestBid = gj;
			}
		}

		return bestTile;
	}

	private bool IsStillBestTile (SearchTile bestTile, float highestBid, SearchTile tile, float gj) {
		if (bestTile == null) { return false; }
		if (gj >= highestBid) {
			if (tile.GetState () == SearchTile.TileState.OPEN) {
				return false;
			} else {
				if ((new SearchTileBid(this.myId,gj)).IsBetterThan(tile.GetHighestBid ())) {
					return false;
				}
			}
		}
		return true;
	}


	public float BidEvaluation(Vector3 currentPosition, Vector3 goal, ArrayList updates) {

		//TODO: really we should optimize these as well....
		float nearnessWeight = 0.1f;
		float distanceWeight = 0.9f;
		float fadingFactor = 0.5f;
		float communicationRange = 150;
		Cost nearnessCost = this.kb.agent.decisionManager.GetCostManager().GetCostFromName("AltNearnessCollisionCost");
		if(nearnessCost == null) Debug.Log("ERROR: Nearness cost is null");
		nearnessCost.SetDesiredPosition(goal);
		float nearness = (float)nearnessCost.GetCostValue();

		// double nearness = 0.0;
		// for (int i = 0; i < updates.Count; i++) {
		// 	double distToGoal = Vector3.Distance (goal, ((AgentUpdate)updates [i]).lastUpdate.position);
		// 	nearness += Mathf.Pow (fadingFactor, i+1) * Mathf.Exp ((float)(distToGoal/communicationRange));
		// }

		// inverse nearness
		if (nearness != 0.0f)
			nearness = (1.0f-nearness);

		float distance = Vector3.Distance (goal, currentPosition);
		if (distance != 0.0f){
			distance = 1.0f/distance;
		}

		return nearnessWeight * ((float) (nearness)) + distanceWeight * distance;
	}

	public SearchTile GetClaimedTile (SearchTile tile) {
		int index = claimedTiles.BinarySearch(tile);
		return (SearchTile) claimedTiles[index];
	}


	public void MarkTileAsSearched (SearchTile tile) {
		tile.SetAsSearched ();
		if (this.claimedTiles.Contains (tile)) {
			this.MoveTile(tile, this.claimedTiles, this.searchedTiles);
		} else if (this.tilesInAuction.Contains (tile)) {
			this.MoveTile(tile, this.tilesInAuction, this.searchedTiles);
		} else if (this.openTiles.Contains (tile)) {
			this.MoveTile(tile, this.openTiles, this.searchedTiles);
		}
	}


	public void RecvClaimedMessage (int srcId, int searchTileId, int agentId, float bestBidAmount) {
		if(agentId == this.myId) return;

		SearchTileBid recvClaimBid = new SearchTileBid(agentId, bestBidAmount);
		SearchTile tile = this.FindTileWithId (this.tilesInAuction, searchTileId);
		if (tile != null) {
			// If the received claim is the best bid, mark the tile as claimed
			if (recvClaimBid.IsBetterThan(tile.GetHighestBid ())) {
				tile.SetAsClaimed (new SearchTileBid(agentId, bestBidAmount));
				this.MoveTile(tile, this.tilesInAuction, this.claimedTiles);
				if((CandidateTile != null && CandidateTile.id == tile.id) ||
					(this.kb.GoalTile != null && this.kb.GoalTile.id == tile.id)){
					this.ResetCandidateAndGoalTile();
				}
			} else {
				// Get the highest bid for the tile claimed
				tile.SetAsClaimed (tile.GetHighestBid ());
				this.MoveTile(tile, this.tilesInAuction, this.claimedTiles);
				// If I have claimed this tile let others know
				if((CandidateTile != null && CandidateTile.id == tile.id) ||
					(this.kb.GoalTile != null && this.kb.GoalTile.id == tile.id)){
					WorldStateManager.MarkTileAsClaimed (tile);
					this.sender.SendClaimed (tile.id, tile.acceptedBid.agentId, tile.acceptedBid.value);
				}
			}
			return;
		}

		tile = this.FindTileWithId (this.openTiles, searchTileId);
		if (tile != null) {
			tile.SetAsClaimed (new SearchTileBid(agentId, bestBidAmount));
			this.MoveTile(tile, this.openTiles, this.claimedTiles);
			return;
		}

		// Check if the tile has been claimed
		tile = this.FindTileWithId (this.claimedTiles, searchTileId);
		if (tile != null) {
			if (tile.acceptedBid.IsBetterThan(recvClaimBid)) {
				if((CandidateTile != null && CandidateTile.id == tile.id) ||
					(this.kb.GoalTile != null && this.kb.GoalTile.id == tile.id)){
					//Debug.Log("Recv claimed for tile "+tile.id+", I agent "+ this.myId+" have claimed.");
					this.sender.SendClaimed (tile.id, tile.acceptedBid.agentId, tile.acceptedBid.value);
				}
			} else {
				// The recv claim is better, so update knowledge
				if((CandidateTile != null && CandidateTile.id == tile.id) ||
					(this.kb.GoalTile != null && this.kb.GoalTile.id == tile.id)){
					ResetCandidateAndGoalTile();
				}

				tile.acceptedBid.agentId = agentId;
				tile.acceptedBid.value = bestBidAmount;
			}
		}

		// Otherwise, the tile has been claimed by another agent or searched already
		tile = this.FindTileWithId (this.searchedTiles, searchTileId);
		if (tile != null) {
			this.sender.SendSearched (searchTileId);
			return;
		}
	}

	public void RecvSearchedMessage (int srcId, int searchTileId) {
		SearchTile tile = this.FindTileWithId (this.claimedTiles, searchTileId);
		if (tile != null) {
			// if (!WorldStateManager.IsSearched (tile)) { Debug.Log ("ERROR : Received Search Message for something that the World State hasn't had searched yet... But you think is claimed"); }
			tile.SetAsSearched ();
			this.MoveTile(tile, this.claimedTiles, this.searchedTiles);
			return;
		}

		tile = this.FindTileWithId (this.tilesInAuction, searchTileId);
		if (tile != null) {
			// if (!WorldStateManager.IsSearched (tile)) { Debug.Log ("ERROR : Received Search Message for something that the World State hasn't had searched yet... But you think is in auction"); }
			tile.SetAsSearched ();
			this.MoveTile(tile, this.tilesInAuction, this.searchedTiles);
			return;
		}

		tile = this.FindTileWithId (this.openTiles, searchTileId);
		if (tile != null) {
			// if (!WorldStateManager.IsSearched (tile)) { Debug.Log ("ERROR : Received Search Message for something that the World State hasn't had searched yet... But you think is open"); }
			tile.SetAsSearched ();
			this.MoveTile(tile, this.openTiles, this.searchedTiles);
			return;
		}
	}

	public bool IsMapSearched () {
		// Debug.Log ("Agent" + this.myId+ "[" + this.openTiles.Count + "]"
		//            + "[" + this.tilesInAuction.Count + "]"
		//            + "[" + this.claimedTiles.Count + "]"
		//            + "[" + this.searchedTiles.Count + "]");
		return this.numTiles == this.searchedTiles.Count;
	}

	private bool MoveTile(SearchTile tile, ArrayList src, ArrayList dst) {
		if (src.Contains (tile)) {
			dst.Add (tile);
			src.Remove (tile);
			return true;
		}
		// Debug.Log ("ERROR | Trying to remove a tile not found.");
		return false;
	}

	private SearchTile FindTileWithId (ArrayList al, int searchTileId) {
		foreach (SearchTile tile in al) {
			if (searchTileId == tile.id) {
				return tile;
			}
		}
		return null;
	}

}
