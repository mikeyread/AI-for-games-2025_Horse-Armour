using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Unity.Burst;
using Unity.Mathematics;

//https://docs.unity3d.com/2022.3/Documentation/Manual/JobSystemParallelForJobs.html
//https://github.com/ThousandAnt/ta-boids/blob/master/Assets/Scripts/ThousandAnt.Boids/Boids.cs

public class FlockingManager : MonoBehaviour
{
    public struct testJob : IJob
    {
        public float a;
        public float b;
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = a + b;
        }
    }
    NativeArray<float> result;
    JobHandle handle;




    //Array containing every flock entity
    public FlockingEntity[] flock;
    public FlockingEntity flockMember;

    //The radius at which flocking entities can detect eachother
    //public float detectionRadius;

    //Value of average position of all flock members
    protected Vector3 averagePosition = Vector3.zero;
    protected Vector3 centrePosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //Spawns in all of the flock entities, and assigns itself as a reference to them
        for (int i = 0; i < flock.Length; i++)
        {
            flock[i]=((FlockingEntity)Instantiate(flockMember, transform.position + (new Vector3(Random.value, Random.value, Random.value))*50, transform.rotation));
            flock[i].ID = i;
            flock[i].manager = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Calculating average position of every cube
        //Vector3 averagePosition = new Vector3 (0,0,0);
        //foreach (var entity in flock)
        //{

        //    //This could probably work, but would need to move functionality into each entitiy instead of it being in the manager
        //    foreach (var otherEntitiy in flock)
        //    {
        //        if ((entity != otherEntitiy) && ((entity.transform.position - otherEntitiy.transform.position).magnitude < detectionRadius))
        //        {
        //            averagePosition += entity.transform.position;
        //        }
        //    }


        //    //averagePosition += entity.transform.position;
        //}
        //centrePosition = averagePosition / flock.Length;
        ////Debug.Log(centrePosition);



        //Perform
        //Coherence();

        //Seperation();

        //Alignment();




        for (int i = 0; i < 4000; i++)
        {
            result = new NativeArray<float>(1, Allocator.TempJob);

            testJob jobData = new testJob
            {
                a = 10,
                b = 10,
                result = result
            };

            // Schedule the job
            handle = jobData.Schedule();
        }

   




    }

    private void LateUpdate()
    {
        handle.Complete(); 

        result.Dispose();
    }



    void Coherence()
    {
        //Cohesion calculation - Takes the Average position of all cubes, calculates each cubes unit vector towards it,
        //then adds a force to make them head there
        foreach (var entity in flock)
        {




            //Unit vector from current cube in loop to the centre
            Vector3 unitVecTowardsCentre = (1 / (centrePosition - entity.transform.position).magnitude) * (centrePosition - entity.transform.position);

            //Adding an impulse to head towards the centre
            //entity.rBody.AddForce(unitVecTowardsCentre);
            //Debug.Log(unitVecTowardsCentre);
        }


        //for (int i = 0; i < flock.length; i++) 
        //{
        //    foreach



        //}

    }



}
