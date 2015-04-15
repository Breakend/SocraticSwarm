using UnityEngine;
using System.Collections;

public class RectZone {

	public Tile[] tiles;
	public float minX;
	public float maxX;
	public float minZ;
	public float maxZ;

	public void CleanUp(){
		tiles = null;
	}

	public RectZone(float xmin, float zmin, float xmax, float zmax, float tileSize) {
		float xlen = (xmax - xmin), zlen = (zmax - zmin);
		if (xlen % tileSize != 0 || zlen % tileSize != 0) {
			Debug.LogError("RectZone : invalid inputs, cannot make perfect squares...");
		}

		this.minX = xmin;
		this.maxX = xmax;
		this.minZ = zmin;
		this.maxZ = zmax;

		this.tiles = CreateTiles(xmin, zmin, xmax, zmax, tileSize);
	}

	public Tile ClosestUnsearchedTile(Vector3 pos) {
		float minDist = float.MaxValue;
		Tile result = null;
		for (int i = 0; i < this.tiles.Length; i++) {
			if (!this.tiles[i].IsSearched() & !this.tiles[i].IsClaimed()) {
				float dist = Vector3.Distance(pos, this.tiles[i].GetCenter());
				if (dist < minDist) {
					minDist = dist;
					result = this.tiles[i];
				}
			}

		}
		return result;
	}

	private Tile[] CreateTiles(float xmin, float zmin, float xmax, float zmax, float tileSize) {
		float xlen = (xmax - xmin), zlen = (zmax - zmin);
		int xtilelen = (int)(xlen / tileSize), ztilelen = (int)(zlen / tileSize);

		Tile[] t = new Tile[xtilelen * ztilelen];
		int index = 0;
		for (float x = xmin; x < xmax; x += tileSize) {
			for (float z = zmin; z < zmax; z += tileSize) {
				t[index++] = new Tile(index, x+tileSize*.05f, z+tileSize*.05f, tileSize-tileSize*.1f);
			}
		}
		return t;
	}

	private Vector3[] ToVector3Array(Vector2[] vectors2) {
		Vector3[] vectors3 = new Vector3[vectors2.Length];
		for (int i = 0; i < vectors2.Length; i++) {
			vectors3[i] = ToVector3(vectors2[i]);
		}
		return vectors3;
	}

	private Vector3 ToVector3(Vector2 vector2) {
		return new Vector3(vector2.x, 0, vector2.y);
	}

	public void DestroyTiles(){
		foreach (Tile t in tiles) {
			t.DestroyTile();
			t.CleanUp();
		}
	}

}
