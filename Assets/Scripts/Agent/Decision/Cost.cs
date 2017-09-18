using UnityEngine;
using System.Collections;

public class Cost : System.Object {

	protected Vector3 desiredPosition;
	protected KnowledgeBase kb;
	protected SensorModule sm;

	protected ArrayList weights;
	protected Weight mainWeight;
	protected string costName;

	public string name{
		get{return costName;}
	}

	public void CleanUp(){
		kb = null;
		sm = null;
		weights.Clear ();
		weights = null;
		mainWeight = null;
	}

	public void SetName(string name){
		this.costName = name;
	}

	public bool satisfiesAsConstraint(float comparedValue){
		return true;
	}

	public Cost(KnowledgeBase kb, SensorModule sm){
		weights = new ArrayList ();
		mainWeight = new Weight ();
		weights.Add (mainWeight);
		this.sm = sm;
		this.kb = kb;
	}

	public void SetDesiredPosition(Vector3 pos){
		desiredPosition = pos;
	}

	// TODO: only vary the main weights or subweights (i.e. like the fading factor for nearness)

	public Weight GetMainWeight(){
		return mainWeight;
	}

	public ArrayList GetAllWeights(){
		return weights;
	}

	public virtual double GetCostValue(){
		return 0.0;
	}

	public double GetWeightedCostValue(){
		return mainWeight.weightValue * this.GetCostValue ();
	}
}
