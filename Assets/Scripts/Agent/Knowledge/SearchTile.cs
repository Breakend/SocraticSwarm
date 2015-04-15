using UnityEngine;
using System.Collections;

public class SearchTile {

	public const float AUCTION_TIME = 0.5f;

	//Timeout for searching a tile.
	// if an agent doesn't search in time then this agent thinks that it's given up
	public const float SEARCH_TIMEOUT_TIME = 60.0f;

	public enum TileState {OPEN, HASBID, CLAIMED, SEARCHED};
	public int id;
	private TileState state;
	public Vector3 center;
	public float auctionEndTime;
	private float searchTimeout;

	private ArrayList bids = null;
	public SearchTileBid acceptedBid = null;

	public void CleanUp(){
		if (bids != null) {
			bids.Clear();
			bids = null;
		}
		acceptedBid = null;
	}

	public SearchTile (int id, Vector3 center) {
		this.id = id;
		this.state = TileState.OPEN;
		this.center = center;
	}


	public void Update () {
		if (this.state == TileState.HASBID) {
			auctionEndTime -= Time.deltaTime;
			if (auctionEndTime < 0) {
				SetAsClaimed(this.GetHighestBid());
			}
		} else if (this.state == TileState.CLAIMED) {
			searchTimeout += Time.deltaTime;
			if (searchTimeout >= SEARCH_TIMEOUT_TIME) {
				// Debug.Log ("Timed out for searching/");
				this.state = TileState.OPEN;
				this.bids = null;

				//Note reset accepted bid when moving this back to open tiles. in bidknowledge
				//TODO: should probably fix that...
			}
		} else if (this.state == TileState.SEARCHED) {
			this.searchTimeout = 0.0f;
		}
	}

	public TileState GetState () {
		return this.state;
	}

	public void AddBid (SearchTileBid bid, bool isMyBid) {
		/* If this is the first bid */
		if (bids == null) {
			bids = new ArrayList ();
			auctionEndTime = AUCTION_TIME;
			this.state = TileState.HASBID;
		}
		else{
			auctionEndTime += .25f * AUCTION_TIME;
		}
		//if (isMyBid) {
//			auctionEndTime = Mathf.Max(3.0f, AUCTION_TIME);
		// Extend the timeout if receive a second bid.
		//}
		bids.Add (bid);
	}

	public int GetWinnerAgentId () {
		return this.acceptedBid.agentId;
	}

	public void SetAsClaimed (SearchTileBid bestBid) {
		acceptedBid = bestBid;
		state = TileState.CLAIMED;
		searchTimeout = 0.0f;
	}

	public void SetAsSearched () {
		state = TileState.SEARCHED;
		searchTimeout = 0.0f;
	}

	public SearchTileBid GetHighestBid () {
		SearchTileBid topBid = (SearchTileBid) bids[0];
		foreach (SearchTileBid bid in bids) {
			if (bid.IsBetterThan (topBid)) {
				topBid = bid;
			}
		}
		return topBid;
	}

}

