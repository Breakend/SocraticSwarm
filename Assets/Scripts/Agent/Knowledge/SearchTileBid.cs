using UnityEngine;
using System.Collections;

public class SearchTileBid{
	
	public int agentId;
	public float value;
	
	public SearchTileBid (int agentId, float value) {
		this.agentId = agentId;
		this.value = value;
	}  

	public bool IsBetterThan(SearchTileBid bid) {
		if (bid == null) {
			Debug.Log ("ERROR: comparing Search Tile to null bid");
			return false;
		}

		if (this.value > bid.value) {
			return true;
		} else if (this.value == bid.value && this.agentId > bid.agentId) {
			return true;
		}
		return false;
	}
}