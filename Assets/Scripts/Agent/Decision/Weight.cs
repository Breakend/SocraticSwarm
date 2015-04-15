using UnityEngine;
using System.Collections;

public class Weight {
	public double weightValue;
	public double weightMin;
	public double weightMax;

	//TODO: add bounds that can be varied
	public Weight(){
		weightValue = 1.0;
		weightMin = 0.0;
		weightMax = 1.0;
	}

	public Weight(double value){
		weightValue = value;
		weightMin = 0.0;
		weightMax = 1.0;
	}
}
