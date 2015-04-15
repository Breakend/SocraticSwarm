using UnityEngine;
using System.Collections;

public class Tile {

	private int id;
	private const float HEIGHT = 3;
	private GameObject gameObject;
	private Vector3[] tileVectors;
	private Vector3 center;
	private bool searched;
	private bool claimed;
	Texture2D texture;

	public void CleanUp(){
		this.gameObject = null;
		this.tileVectors = null;
	}

	public Tile (int id, float xmin, float zmin, float size) {
		this.id = id;
		this.tileVectors = GetTileVectors(xmin, zmin, size); 
		this.center = GetXZCenter(xmin, zmin, size);
		this.center.y += 40;
		this.searched = false;

		string name = "Tile-" + xmin + "-" + zmin;
		this.gameObject = InitTile(name, this.tileVectors, HEIGHT, Color.red);
	
	}

	public int GetID () {
		return this.id;
	}

	public void SetAsHasBid () {
		Texture2D tex = new Texture2D (1, 1);
		tex.SetPixel (0, 0, Color.cyan);
		tex.Apply();
		if(texture) UnityEngine.Object.Destroy (texture);
		texture = tex;
		this.gameObject.renderer.material.mainTexture = tex;
		this.gameObject.renderer.material.color = Color.yellow;
	}

	// Called when an agent has searched the tile
	public void SetAsSearched () {
		Texture2D tex = new Texture2D (1, 1);
		tex.SetPixel (0, 0, Color.green);
		tex.Apply();
		if(texture) UnityEngine.Object.Destroy (texture);
		texture = tex;
		this.gameObject.renderer.material.mainTexture = tex;
		this.gameObject.renderer.material.color = Color.green;
		this.searched = true;
	}

	public void SetAsClaimed () {
		Texture2D tex = new Texture2D (1, 1);
		tex.SetPixel (0, 0, Color.yellow);
		tex.Apply();
		if(texture) UnityEngine.Object.Destroy (texture);
		texture = tex;
		this.gameObject.renderer.material.mainTexture = tex;
		this.gameObject.renderer.material.color = Color.yellow;
		this.claimed = true;
	}

	public void SetAsOpen(){
		Texture2D tex = new Texture2D (1, 1);
		tex.SetPixel (0, 0, Color.red);
		tex.Apply();
		if(texture) UnityEngine.Object.Destroy (texture);
		texture = tex;
		this.gameObject.renderer.material.mainTexture = tex;
		this.gameObject.renderer.material.color = Color.red;
		this.claimed = false;
		this.searched = false;
	}

	public bool IsClaimed(){
		return this.claimed;
	}

	public bool IsSearched() {
		return this.searched;
	}

	public Vector3 GetCenter() {
		return this.center;
	}
	
	private Vector3[] GetTileVectors(float xmin, float zmin, float size) {
		Vector3[] vectors = new Vector3[4];
		vectors[0] = new Vector3(xmin, 0, zmin);
		vectors[1] = new Vector3(xmin + size, 0, zmin);
		vectors[2] = new Vector3(xmin + size, 0, zmin + size);
		vectors[3] = new Vector3(xmin, 0, zmin + size);
		return vectors;
	}

	private Vector3 GetXZCenter(float xmin, float zmin, float size) {
		return SetHeightAboveTerrain(new Vector3(xmin + (size/2), 0, zmin + (size/2)), HEIGHT);
	}

	private GameObject InitTile(string name, Vector3[] input, float height, Color color) {
		GameObject plane = new GameObject(name);
		MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
		meshFilter.mesh = CreateTileMesh(input,height);
		MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		renderer.material.shader = Shader.Find ("Particles/Additive");
		Texture2D tex = new Texture2D(1, 1);
		tex.SetPixel(0, 0, color);
		tex.Apply();
		if(texture) UnityEngine.Object.Destroy (texture);
		renderer.material.mainTexture = tex;
		texture = tex;
		renderer.material.color = color;
		return plane;
	}

	private Mesh CreateTileMesh(Vector3[] input, float height) {
		for (int i = 0; i < input.Length; i++) {
			input[i] = SetHeightAboveTerrain(input[i], height);
		}
		
		Mesh m = new Mesh();
		m.name = "QuadMesh";
		
		m.vertices = input;
		m.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (0, 1),
			new Vector2 (1, 1),
			new Vector2 (1, 0)
		};
		m.triangles = new int[] { 0, 1, 2, 0, 2, 3};
		m.RecalculateNormals();
		
		return m;
	}

	private Vector3 SetHeightAboveTerrain(Vector3 vec3, float height) {
		RaycastHit hit;
		Ray downRay;
		vec3.y = 400;
		downRay = new Ray(vec3, -Vector3.up);
		if (Physics.Raycast(downRay, out hit)) {
			//Debug.Log("Hit distance = " + hit.distance);
			vec3.y = 400 + height - hit.distance;
		}
		return vec3;
	}

	public void DestroyTile(){
		if(texture) UnityEngine.Object.Destroy (texture);
		texture = null;
		UnityEngine.Object.Destroy (this.gameObject);
	}

	private Vector3 ToVector3(Vector2 vector2) {
		return new Vector3(vector2.x, 0, vector2.y);
	}

}
