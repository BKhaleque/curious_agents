using System.Collections.Generic;
using Agents;
using UnityEngine;

public class Evolve : MonoBehaviour
{

    public int numOfGenerations;
    public int initialPopSize;


    private Environment_Genome[] popSize;

   // private Global_Agent evaluator;


    //int noOfSectors;

    public GameObject[] assetsToSpawn;
    public MeshRenderer MeshRenderer;
    //private NavMeshSurface surface;

    void Start()
    {
        //yield return new WaitForSeconds(2f);
        //noOfSectors = 1;
        //evaluator = gameObject.GetComponent<Global_Agent>();
        popSize = new Environment_Genome[initialPopSize];
        InitialiseInitialPop();
        evolve();
    }

    void evolve()
    {

        //evaluate initial pop
        var pop = DeepCopy(popSize);

        for (var i = 0; i<numOfGenerations; i++)
        {
            var newPop = new Environment_Genome[initialPopSize];
            for(var j = 0; j<initialPopSize; j++)
            {
                //pick parents
                 //var parents = PickParents(pop);
                //mate parents
                 //newPop[j] = crossOver(parents[0], parents[1]);
                //evaluate old pop
                //evaluator.EvaluateMesh(newPop[j]);
            }
            //Replace old pop (have some elitism too)

            //pop = newPop;

        }
        //evaluator.visionEvaluation();


        // Environment_Genome testMesh = new Environment_Genome();
        // testMesh.hillsProp = 0.5f;
        // testMesh.areaOfInfluence = 20;
        // testMesh.totalNumOfMidPoints = 20;
        // testMesh.totalNumOfHighPoints = 25;
        // testMesh.totalZSize = 400;
        // testMesh.totalXSize = 400;
        // testMesh.totalNumOfLandmarks = 20;
        // testMesh.midValueModifier = 0.5f;
        // testMesh.highValueModifier = 30;
        //evaluator.EvaluateMesh(pop[0]);
        //var highestFitness = popSize[3];
		// for(var i = 1; i<pop.Length;i++){
		// 	evaluator.EvaluateMesh(pop[i]);
		// 	if(pop[i].score > highestFitness.score)
		// 		highestFitness = pop[i];
		// }
        //evaluator.DrawMesh(highestFitness);
		Debug.Log("Mesh Rendered");
		//Debug.Log(highestFitness.score);
    }

    private static Environment_Genome[] DeepCopy(IReadOnlyList<Environment_Genome> pop)
    {
        var copy = new Environment_Genome[pop.Count];
        for(var i = 0; i < pop.Count; i++)
        {
            copy[i] = pop[i];
        }
        return copy;
    }

    private static Environment_Genome[] PickParents(IReadOnlyList<Environment_Genome> pop)
    {
        //Run a tournament and pick the parents
        var parents = new Environment_Genome[2];
        var potentialParents = new Environment_Genome[3];
        potentialParents[0] = pop[Random.Range(0, pop.Count)]; //Random for now
        potentialParents[1] = pop[Random.Range(0, pop.Count)];
		while(potentialParents[1].Equals(potentialParents[0])){
			 potentialParents[1] = pop[Random.Range(0, pop.Count)];
		}
		potentialParents[2] = pop[Random.Range(0, pop.Count)];
		while(potentialParents[2].Equals(potentialParents[1])|| potentialParents[2].Equals(potentialParents[0])){
			 potentialParents[2] = pop[Random.Range(0, pop.Count)];
		}
		var firstPotentialPicked = false;
		var secondPotentialPicked = false;
		var thirdPotentialPicked = false;
		if(potentialParents[0].score >= potentialParents[1].score && potentialParents[0].score >= potentialParents[2].score){
			parents[0] = potentialParents[0];
			firstPotentialPicked = true;
		}
		else if(potentialParents[1].score >= potentialParents[0].score && potentialParents[1].score>=potentialParents[2].score){
			parents[0] = potentialParents[1];
			secondPotentialPicked = true;
		}
		else if(potentialParents[2].score >= potentialParents[1].score && potentialParents[2].score>=potentialParents[0].score){
			parents[0] = potentialParents[2];
			thirdPotentialPicked = true;
		}

		if(firstPotentialPicked){
			if(potentialParents[1].score >= potentialParents[2].score)
				parents[1] = potentialParents[1];
			else
				parents[1] = potentialParents[2];
		}
		if(secondPotentialPicked){
			if(potentialParents[0].score >= potentialParents[2].score)
				parents[1] = potentialParents[0];
			else
				parents[1] = potentialParents[2];
		}

		if (!thirdPotentialPicked) return parents;
		if(potentialParents[0].score >= potentialParents[1].score)
			parents[1] = potentialParents[0];
		else
			parents[1] = potentialParents[1];

		return parents;
    }

    //Randomly assign sizes to inital pop on x and z axis and assign default no of landmarks
    void InitialiseInitialPop()
    {
        for (var i = 0; i <initialPopSize; i++)
        {
            //random values for now
            popSize[i].totalXSize = Random.Range(400, 400);
            popSize[i].totalZSize = Random.Range(400, 400);
            popSize[i].totalNumOfMidPoints = Random.Range(20, 40);
            popSize[i].totalNumOfHighPoints = Random.Range(20, 40);
            popSize[i].totalNumOfLandmarks = Random.Range(0, 5);
            popSize[i].midValueModifier = Random.Range(0f, 1f);
            popSize[i].highValueModifier = Random.Range(-10f, 10f);
            popSize[i].hillsProp = Random.Range(0, 1f);
            popSize[i].areaOfInfluence = Random.Range(0, 40f);
            popSize[i].poissonDiscParams = new PoissonDiscParams(Random.Range(30, popSize[i].totalXSize),0,Random.Range(30, popSize[i].totalZSize),Random.Range(0, 200),assetsToSpawn, Random.Range(0, popSize[i].totalXSize), popSize[i].totalZSize, Random.Range(7, 12));
            popSize[i].score = 0f;
        }
    }

    Environment_Genome crossOver(Environment_Genome parent1, Environment_Genome parent2)
    {

        var child = new Environment_Genome
        {
            totalXSize = Random.Range(0, 100) > 50 ? parent1.totalXSize : parent2.totalXSize,
            totalZSize = Random.Range(0, 100) > 50 ? parent1.totalZSize : parent2.totalZSize,
            totalNumOfMidPoints = Random.Range(0, 100) > 50 ? parent1.totalNumOfMidPoints : parent2.totalNumOfMidPoints,
            totalNumOfHighPoints = Random.Range(0, 100) > 50 ? parent1.totalNumOfHighPoints : parent2.totalNumOfHighPoints,
            totalNumOfLandmarks = Random.Range(0, 100) > 50 ? parent1.totalNumOfLandmarks : parent2.totalNumOfLandmarks,
            hillsProp = Random.Range(0, 100) > 50 ? parent1.hillsProp : parent2.hillsProp,
            areaOfInfluence = Random.Range(0, 100) > 50 ? parent1.areaOfInfluence : parent2.areaOfInfluence,
            highValueModifier = Random.Range(0, 100) > 50 ? parent1.highValueModifier : parent2.highValueModifier,
            midValueModifier = Random.Range(0, 100) > 50 ? parent1.midValueModifier : parent2.midValueModifier
        };
        child.poissonDiscParams.assetsToSpawn= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.assetsToSpawn : parent2.poissonDiscParams.assetsToSpawn;
        child.poissonDiscParams.assetXSpread= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.assetXSpread : parent2.poissonDiscParams.assetXSpread;
        child.poissonDiscParams.assetYSpread= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.assetYSpread : parent2.poissonDiscParams.assetYSpread;
        child.poissonDiscParams.assetZSpread= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.assetZSpread : parent2.poissonDiscParams.assetZSpread;
        child.poissonDiscParams.numOfAssetsToSpawn= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.numOfAssetsToSpawn : parent2.poissonDiscParams.numOfAssetsToSpawn;
        child.poissonDiscParams.forestXSize= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.forestXSize : parent2.poissonDiscParams.forestXSize;
        child.poissonDiscParams.forestYSize= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.forestYSize : parent2.poissonDiscParams.forestYSize;
        child.poissonDiscParams.forestRadius= Random.Range(0, 100) > 50 ? parent1.poissonDiscParams.forestRadius : parent2.poissonDiscParams.forestRadius;
        child.score = 0f;
        return child;
    }

    void mutate()
    {
        //TODO
    }




}

public struct Environment_Genome
{
    //define size values and x and z coordinates
    [HideInInspector]
    public int totalXSize;
    public int totalZSize;
    public int totalNumOfMidPoints;
    public int totalNumOfHighPoints;
    //public Landmark[] landmarks; // Large important objects
    //public GameObject[] assets; // smaller objects to be laid out
    //public Sector[] sectors;
    public int totalNumOfLandmarks;
    public float midValueModifier;
    public float highValueModifier;

    public float hillsProp;
    public float areaOfInfluence;
    public PoissonDiscParams poissonDiscParams;
    public float score;

    // public float midValueModifier;
    // public float highValueModifier;

    // public float hillsProp;
    // public float areaOfInfluence;

}

public struct PoissonDiscParams
{
    public int assetXSpread;
    public int assetYSpread;
    public int assetZSpread;
    public int numOfAssetsToSpawn;
    public GameObject[] assetsToSpawn;
	public int forestXSize;
	public int forestYSize;
	public int forestRadius;
    //public string type;

    public PoissonDiscParams(int assetXSpread, int assetYSpread, int assetZSpread, int numOfAssetsToSpawn, GameObject[] assetsToSpawn, int forestXSize, int forestYSize, int forestRadius)
    {
        this.assetXSpread = assetXSpread;
        this.assetYSpread = assetYSpread;
        this.assetZSpread = assetZSpread;
        this.numOfAssetsToSpawn = numOfAssetsToSpawn;
        this.assetsToSpawn = assetsToSpawn;
		this.forestXSize = forestXSize;
		this.forestYSize = forestYSize;
		this.forestRadius = forestRadius;
        //this.type = type;
    }
}

public struct Landmark
{
    [HideInInspector]
    //public int x;
    //public int y;
    //public int z;
    public int importance;

    public string type;
    public GameObject[] gameObjects;

    private List<Vector2> points;


}
