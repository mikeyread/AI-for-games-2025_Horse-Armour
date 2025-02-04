using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FlockingEntity : MonoBehaviour
{
    Renderer ren;

    public Rigidbody rBody;
    public FlockingManager manager;
    private float DetectionRadius = 20;

    private float distBetween = 0;


    //Each member will randomly pick a colour, then when someone else joins them they will share that colour. 
    //Which colour is chosen is determined by the amount of members with the same colour (they will always join the higher amount)
    //or if it's equal, random choice.
    private enum colour
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Magenta = 3
    };
    





    //Amount of flock members nearby
    private int amountNearby = 0;
    //The average position between them
    private Vector3 positionAverage = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<Renderer>();
        ren.material.color=Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        //Resetting Variables
        positionAverage = Vector3.zero;
        amountNearby = 0;


        foreach(var entity in manager.flock)
        {

            //Only starts cohesion calculations once within a certain range of another entity
            distBetween = Vector3.Distance(entity.transform.position, transform.position);
            if ((entity != this) && (distBetween < DetectionRadius))
                {
                positionAverage += entity.transform.position;

                ren.material.color = Color.red;
                amountNearby++;
                //Debug.Log("Distance to other: " + Vector3.Distance(entity.transform.position, transform.position));
                }


        }

        positionAverage = positionAverage / amountNearby;


        //Cohesion
        Vector3 unitVecTowardsCentre = (1 / (positionAverage - transform.position).magnitude * (positionAverage - transform.position));

        if (unitVecTowardsCentre != null)
        {
            rBody.AddForce(unitVecTowardsCentre);
        }

    }
}
