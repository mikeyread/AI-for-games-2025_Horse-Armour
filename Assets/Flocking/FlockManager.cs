using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Rendering;

public class FlockingManager : MonoBehaviour
{
    //Array containing every flock entity
    public FlockingEntity[] flock;
    public FlockingEntity flockMember;


    //Value of average position of all flock members
    protected Vector3 averagePosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //Spawns in all of the flock entities, and assigns itself as a reference to them
        for (int i = 0; i < flock.Length; i++)
        {
            flock[i]=((FlockingEntity)Instantiate(flockMember, transform.position + (new Vector3(Random.value, Random.value, Random.value))*50, transform.rotation));
            flock[i].manager = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Calculating average position of every cube
        Vector3 averagePosition = new Vector3 (0,0,0);
        foreach (var entity in flock)
        {
            averagePosition += entity.transform.position;
        }
        Vector3 centrePosition = averagePosition / flock.Length;
        //Debug.Log(averagePosition);



        //Cohesion calculation - Takes the Average position of all cubes, calculates each cubes unit vector towards it,
        //then adds a force to make them head there
        foreach (var entity in flock)
        {
            //Unit vector from current cube in loop to the centre
            Vector3 unitVecTowardsCentre = (1 / (centrePosition - entity.transform.position).magnitude) * (centrePosition - entity.transform.position);

            //Adding an impulse to head towards the centre
            entity.rBody.AddForce(unitVecTowardsCentre);
            Debug.Log(unitVecTowardsCentre);
        }



        



    }
}
