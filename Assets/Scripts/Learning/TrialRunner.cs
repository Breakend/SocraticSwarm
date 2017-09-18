using UnityEngine;
using System.Collections;

public class TrialRunner : MonoBehaviour {

	public static TrialRunner instance;
	public static TrialRunner GetSingletonInstance(){
				return instance;
		}

	// For simulated annealing
	private static float COOL_OFF_RATE = 0.9f;
	private static float STARTING_TEMP;
	private float[] ti_os;
	private static float CostStartingTemp;
	private ArrayList outputStrs = new ArrayList ();
	private float temperature;
	private float[] current_gradients;
	private float[] max_gradients;
	private float C_I;
	private float[] temperatures;
	private double sum_eval;
	private const bool TO_LEARN = true;
	private string logfiletimestamp;

	private double global_minimum = double.MaxValue;
	private ArrayList bestWeights;

	// Attach this as script i guess
	public WorldStateManager worldStateManager;

	private int maxTrials = 50;
	private ArrayList allEvals = new ArrayList();
	private ArrayList allWeightsAttempted = new ArrayList();
	private ArrayList allTimes = new ArrayList();
	private ArrayList allCollisions = new ArrayList();
	private ArrayList allTilesSearched = new ArrayList();
	public static float MAX_TRIAL_RUN_TIME = 200.0f;
	public static System.Type[] costsToUse = { System.Type.GetType ("AltAltitudeCost"),
		System.Type.GetType ("AltNearnessCollisionCost"),
		System.Type.GetType ("CollisionCost"),
		System.Type.GetType ("AltGoalCost")};

	ArrayList trialWeights;
	ArrayList prevTrialWeights;

	private double minimumSoFar = double.MaxValue;

	private int numOfTrialsCompleted = -1;
	private int[] numGeneratedParameters;
	private int numberAcceptedTrials = 0;

	public void CoolOffAllParameterTempsASA(){
		int i = 0;
		foreach (Weight w in trialWeights) {
			CoolOffParameterTempsASA(i++);
		}
	}
	//ASA Eq. 4.34

	public float c_i{
		get{return C_I;}
	}

	public void calculate_c_i(){
		float m_i = .5f;
		float n_i = .5f;
		float c_i = m_i * Mathf.Exp (-n_i/ (float) temperatures.Length);
		C_I = c_i;
	}

	public void CoolOffParameterTempsASA(int i){
		temperatures[i] = STARTING_TEMP * Mathf.Exp(-c_i* Mathf.Pow (numGeneratedParameters[i], (1.0f/(float)(temperatures.Length))));
	}

	public void CoolOffCostTempASA(){
		temperature = STARTING_TEMP * Mathf.Exp(-c_i * Mathf.Pow (numberAcceptedTrials, (1.0f/(float)(temperatures.Length))));
	}

	public void CoolOffAllParameterTempsModifiedASA(){
		int i = 0;
		foreach (Weight w in trialWeights) {
			CoolOffParameterTempsModifiedASA(i++);
		}
	}

	public void CoolOffParameterTempsModifiedASA(int i){
		temperatures[i] = ti_os[i] * Mathf.Pow(.9f, numOfTrialsCompleted);
	}

	public void CoolOffCostTempsModifiedASA(){
		temperature = CostStartingTemp * Mathf.Pow(.9f, numOfTrialsCompleted);
	}


	public void GenerateNextPointsAccordingToModifiedASA(){
		int i = 0;
		foreach(Weight w in trialWeights){
			//Constrain the uniformly distributed random variable according to the temperature
			double currentWeight = GetCurrentBestWeightValueAt(i);
			Debug.Log ("Best current weight" + currentWeight);
			float constraintMin = (float) (w.weightMin - currentWeight)* temperatures[i]/ti_os[i] * .5f;
			Debug.Log ("constraint min weight" + constraintMin);
		
			float constraintMax = (float) (w.weightMax - currentWeight)* temperatures[i]/ti_os[i] * .5f;
			Debug.Log ("constraint max weight" + constraintMax);
			float gamma = Random.Range(constraintMin, constraintMax);
			Debug.Log ("gamma " + gamma);
			w.weightValue = (double)Mathf.Clamp((float)currentWeight + gamma, 0.0f, 1.0f);
			numGeneratedParameters[i++] += 1;
		}
	}

	public void GenerateNextPointsAccordingToASA(){
		int i = 0;
		foreach(Weight w in trialWeights){
			float u = Random.Range(0.0f, 1.0f);
			//TODO: gamma's are genereated until they meet the constraints of being between weightMin and weightMax so add a loop here
			double gamma = w.weightMax + 1f;
			double p = w.weightMin - 1f;
			u = Random.Range (0.0f, 1.0f);
                        gamma = Mathf.Sign(u - .5f)*temperatures[i]*(Mathf.Pow((1.0f+(1.0f/temperatures[i])), Mathf.Abs(2.0f*u-1.0f))-1.0f);
			Debug.Log ("gamma " + gamma);
			p = GetCurrentBestWeightValueAt(i) + gamma * (w.weightMax - w.weightMin);
			w.weightValue = (double)Mathf.Clamp((float)p, 0.0f, 1.0f);
			numGeneratedParameters[i++] += 1;
		}
	}

	public void RandomlyGenerateNeighbour(){
		//Generate new weights for the next trial
		int whichWeightToChange = Random.Range (0, trialWeights.Count);
		float randomNewValue = (float) ((Weight)trialWeights [whichWeightToChange]).weightValue + Random.Range (-.1f, .1f);
		((Weight)trialWeights [whichWeightToChange]).weightValue = Mathf.Clamp(randomNewValue, (float) ((Weight)trialWeights [whichWeightToChange]).weightMin, (float) ((Weight)trialWeights [whichWeightToChange]).weightMax);
	}

	public double GetTrialWeightValueAt(int i){
		return ((Weight) trialWeights[i]).weightValue;
	}

	public double GetCurrentBestWeightValueAt(int i){
		return ((Weight) prevTrialWeights[i]).weightValue;
	}

	public void GeometricCoolOff(){
		//cool off
		if (temperature > 0)
			temperature = temperature * COOL_OFF_RATE;
	}

	public float evaluateRun(){
		float x = ((float)WorldStateManager.numberOfTilesSearched)/(float)WorldStateManager.GetNumberOfTiles();
		float unsearchedTileCost = 1.0f - Mathf.Pow(x, 2.0f); // 1.0f - Mathf.Pow(x-1.0f, 2.0f);
		float y = (float)WorldStateManager.numberOfCollisions;
		float agentCollisionCost = (y > 0) ? .25f + (y)*(.75f/(float) WorldStateManager.GetAgentsToGenerate) : 0f;
		float timeCost = (float) ((WorldStateManager.GetAgentsToGenerate == WorldStateManager.numberOfCollisions) ? 1.0 : (float)WorldStateManager.timeElapsed / (float)MAX_TRIAL_RUN_TIME);

		float unsearchedTileWeight = 4f;
		float timeWeight = 1f;
		float collisionWeight = 4f;


		return (unsearchedTileWeight*unsearchedTileCost + collisionWeight*agentCollisionCost + timeWeight*timeCost)*100f;
	}

	public void UpdateGradients(double eval){
		if(minimumSoFar == double.MaxValue) return;
		int i = 0;
		foreach (Weight w in trialWeights) {
			//Every 100 trails or so as in:
			// http://download.springer.com/static/pdf/378/chp%253A10.1007%252F978-3-642-27479-4_4.pdf?auth66=1427569020_45eda008ce9c78b1a8b9a77e1b1a01c0&ext=.pdf
			current_gradients[i] = (float) ((minimumSoFar - eval)/(w.weightValue - GetCurrentBestWeightValueAt(i)));
			if(current_gradients[i] > max_gradients[i]) max_gradients[i] = current_gradients[i];
			i++;
		}
	}

	public void EndTrial(){
		double eval = evaluateRun ();
		allEvals.Add(eval);
		string weightstring = "";
		foreach (Weight w in trialWeights) {
			weightstring += w.weightValue + "\t";
		}
		allTimes.Add((double)WorldStateManager.timeElapsed );
		allTilesSearched.Add(WorldStateManager.numberOfTilesSearched);
		allCollisions.Add(WorldStateManager.numberOfCollisions);
		weightstring += "\n";
		System.IO.File.AppendAllText("./log_" + logfiletimestamp + "evals.dat", "" + numOfTrialsCompleted + "\t" + eval + "\n");
		System.IO.File.AppendAllText("./log_" + logfiletimestamp + "weights.dat", "" + numOfTrialsCompleted + "\t" + weightstring );
		System.IO.File.AppendAllText("./log_" + logfiletimestamp + "temperatures.dat", "" + numOfTrialsCompleted + "\t" + temperature + "\t" + temperatures[0] +"\n" );
		System.IO.File.AppendAllText("./log_" + logfiletimestamp + "tiles_searched.dat", "" + numOfTrialsCompleted + "\t" + WorldStateManager.numberOfTilesSearched + "\n");
		System.IO.File.AppendAllText("./log_" + logfiletimestamp + "times.dat", "" + numOfTrialsCompleted + "\t" + WorldStateManager.timeElapsed +"\n");
		System.IO.File.AppendAllText("./log_" + logfiletimestamp + "collisions.dat", "" + numOfTrialsCompleted + "\t" + WorldStateManager.numberOfCollisions+"\n");
		Debug.Log("Trial Evaluation:" + eval);
		Debug.Log("Cost Temperature:" + temperature);

		// Simulated annealing temperature
		float delta = (float)eval - (float)minimumSoFar;

		float trueProbability = Mathf.Min(Mathf.Exp (- (float)delta / temperature), 1.0f);
		bool keepResult = trueProbability > Random.Range(0.0f, 1.0f);
		UpdateGradients(eval);
		if (eval < minimumSoFar || (keepResult)) {
			weightstring = "";
			foreach (Weight w in trialWeights) {
				weightstring += w.weightValue + "\t";
			}
			weightstring += "\n";
			System.IO.File.AppendAllText("./log_" + logfiletimestamp + "accepted_evals.txt", "" + numOfTrialsCompleted + "\t" + eval + "\n");
			System.IO.File.AppendAllText("./log_" + logfiletimestamp + "accepted_weights.txt", "" + numOfTrialsCompleted + "\t" + weightstring );

			minimumSoFar = eval;
			copyOverTrialWeightsToCache ();
			sum_eval += eval;
			outputStrs.Add (numOfTrialsCompleted + " " + eval + " " + sum_eval/numberAcceptedTrials);
			numberAcceptedTrials++;
			if(eval < global_minimum){
				global_minimum = eval;
				bestWeights.Clear ();
				foreach (Weight w in trialWeights) {
					bestWeights.Add (new Weight(w.weightValue));
				}

				weightstring = "";
				foreach (Weight w in bestWeights) {
					weightstring += w.weightValue + "\t";
				}
				weightstring += "\n";
				System.IO.File.AppendAllText("./log_" + logfiletimestamp + "best_evals.txt", "" + numOfTrialsCompleted + "\t" + global_minimum + "\n");
				System.IO.File.AppendAllText("./log_" + logfiletimestamp + "best_weights.txt", "" + numOfTrialsCompleted + "\t" + weightstring );
			}
//			every few trials reanneal
			 if(numberAcceptedTrials % 25 == 0){
			 	//TODO: reanneal
			 	ReannealAllParameters ();
			 	ReannealCostTemperatures ();
			 }
		} else {
			copyOverCacheWeightsToCurrent ();
		}

		//cool off

		CoolOffCostTempsModifiedASA ();
		CoolOffAllParameterTempsModifiedASA ();
		System.GC.Collect();
		if(TO_LEARN){
			GenerateNextPointsAccordingToModifiedASA ();
		}

	}

	//ASA Eq 4.40 & 4.39
	private float ParameterTemperatureAnnealingSchedule(float T0, float k){
		return T0 * Mathf.Exp(-c_i* Mathf.Pow (k, (1.0f/(float)(temperatures.Length))));
	}

	private void ReannealCostTemperatures(){
		numberAcceptedTrials = (int)(Mathf.Pow (Mathf.Log(CostStartingTemp/temperature)/c_i, temperatures.Length));
		CostStartingTemp = (float)minimumSoFar;
	}

	private void ReannealAllParameters(){
		int i = 0;
		foreach (Weight w in trialWeights) {
			//Every 100 trails or so as in:
			// http://download.springer.com/static/pdf/378/chp%253A10.1007%252F978-3-642-27479-4_4.pdf?auth66=1427569020_45eda008ce9c78b1a8b9a77e1b1a01c0&ext=.pdf
				temperatures[i] = ParameterTemperatureAnnealingSchedule(ti_os[i], Mathf.Abs(max_gradients[i]/current_gradients[i]));

				//TODO: rename this to Ki to match paper
//			TODO: there might be issues with int to float conversion
				numGeneratedParameters[i] = (int) (Mathf.Pow (Mathf.Log(ti_os[i]/temperatures[i])/c_i, temperatures.Length));
				ti_os[i] = temperatures[i]; //Set T0i to unity

			i++;
		}
	}

	public void LaunchNextTrial(){
		int numberOfWeights = 0;
		int weightIndex = 0;

		if(numOfTrialsCompleted >= maxTrials){
			Debug.Log ("Finished running " + numOfTrialsCompleted + " trials.");
			string s = "";
			foreach(string str in outputStrs){
				s += str;
				s += "\n";
			}
			Debug.Log (s);


			//Print out best weights
			s = "Best Weights with eval: " + global_minimum + "\n";
			foreach (System.Type t in costsToUse) {
				string typeString = t.FullName ;
				s += "\t" + typeString + ":\n";
				numberOfWeights = (int) t.GetMethod("GetNumberWeights", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, null);
				for(int i=0;i<numberOfWeights;i++){
					//				Debug.Log (weightIndex);

					s += "\t\tw" + i + " : " + ((Weight)bestWeights[weightIndex++]).weightValue + "\n";
				}
			}

			Debug.Log (s);

			double summ = 0.0;
			foreach(double c in allEvals){
				summ += c;
			}
			double mean = summ/allEvals.Count;
			double variances_sum = 0.0;
			foreach(double c in allEvals){
				double diff = c - mean;
				variances_sum += diff*diff;
			}

			Debug.Log("Mean of Evaluations: " + mean + "\nVariance: " + variances_sum/allEvals.Count);

		 	summ = 0.0;
			foreach(int c in allCollisions){
				summ += (double) c;
			}
			mean = summ/allCollisions.Count;
			variances_sum = 0.0;
			foreach(int c in allCollisions){
				double diff = (double)c - mean;
				variances_sum += diff*diff;
			}
			Debug.Log("Mean of Collisions: " + mean + "\nVariance: " + variances_sum/allCollisions.Count);


		 	summ = 0.0;
			foreach(int c in allTilesSearched){
				summ += (double) c;
			}
			mean = summ/allTilesSearched.Count;
			variances_sum = 0.0;
			foreach(int c in allTilesSearched){
				double diff = (double)c - mean;
				variances_sum += diff*diff;
			}
			Debug.Log("Mean of TilesSearched: " + mean + "\nVariance: " + variances_sum/allTilesSearched.Count);

		 	summ = 0.0;
			foreach(double c in allTimes){
				summ += c;
			}
			mean = summ/allTimes.Count;
			variances_sum = 0.0;
			foreach(double c in allTimes){
				double diff = c - mean;
				variances_sum += diff*diff;
			}
			Debug.Log("Mean of Times: " + mean + "\nVariance: " + variances_sum/allTimes.Count);

			return;
		}

		numOfTrialsCompleted += 1;

		string debugString = "Running trial: " + numOfTrialsCompleted + " with weights: \n";

		numberOfWeights = 0;
		weightIndex = 0;
		foreach (System.Type t in costsToUse) {
			string typeString = t.FullName ;
			debugString += "\t" + typeString + ":\n";
			numberOfWeights = (int) t.GetMethod("GetNumberWeights", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, null);
			for(int i=0;i<numberOfWeights;i++){
//				Debug.Log (weightIndex);

				debugString += "\t\tw" + i + " : " + ((Weight)trialWeights[weightIndex++]).weightValue + "\n";
			}
		}

		Debug.Log (debugString);

		// Default can be overwritten
		worldStateManager.SetGrid (new Vector2[] {
			new Vector2 (3800, 3400),
			new Vector2 (4200, 3400),
			new Vector2 (3800, 4000),
			new Vector2 (4200, 4000)
		}, 25);

		worldStateManager.RunTrial ();

	}

	private void copyOverTrialWeightsToCache(){
		prevTrialWeights.Clear ();
		foreach (Weight w in trialWeights) {
			prevTrialWeights.Add (new Weight(w.weightValue));
		}
	}

	private void copyOverCacheWeightsToCurrent(){
		trialWeights.Clear ();
		foreach (Weight w in prevTrialWeights) {
			trialWeights.Add (new Weight(w.weightValue));
		}
	}


	// Use this for initialization
	void Start () {
		numberAcceptedTrials = 0;
		logfiletimestamp = System.DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss_fff");
		//Maximum possible cost.
		// TODO: don't hard code this and maybe choose a better starting temp
		STARTING_TEMP = temperature = 1f/Mathf.Pow(.95f, maxTrials/2);
		CostStartingTemp = temperature;
		instance = this;
		trialWeights = new ArrayList ();
		prevTrialWeights = new ArrayList ();
		int totalNumberOfWeights = 0;
		int numberOfWeights;
		foreach (System.Type t in costsToUse) {
			numberOfWeights = (int) t.GetMethod("GetNumberWeights", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, null);

			double[] defaultWeightCosts = (double[]) t.GetMethod("GetDefaultWeightCosts", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, null);
			for(int i =0;i<numberOfWeights;i++){
				this.trialWeights.Add(new Weight(defaultWeightCosts[i]));
			}
			totalNumberOfWeights += numberOfWeights;
		}
		Debug.Log ("Number of weights testing: " + totalNumberOfWeights);
		Debug.Log (costsToUse.Length + " main weight(s) and " + (totalNumberOfWeights - costsToUse.Length) + " subweight(s)");

		copyOverTrialWeightsToCache ();
		temperatures = new float[trialWeights.Count];
		numGeneratedParameters = new int[trialWeights.Count];
		ti_os = new float[trialWeights.Count];
		for(int i=0;i< temperatures.Length;i++){
			ti_os[i] = STARTING_TEMP;
		}
		current_gradients = new float[trialWeights.Count];
		max_gradients = new float[trialWeights.Count];
		for(int i=0;i< temperatures.Length;i++){
			temperatures[i] = temperature;
		}

		bestWeights = new ArrayList ();

		calculate_c_i ();
		this.LaunchNextTrial ();


	}

	// Update is called once per frame
	void Update () {

	}
}
