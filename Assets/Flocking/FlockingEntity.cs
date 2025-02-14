using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
//using System;       //Used for array functions like .Clear


public class FlockingEntity : MonoBehaviour
{
    //Renderer to change objects colour
    Renderer ren;
    
    //Rigidbody component to allow easier implementation of movement (essentially just so I can use .AddForce();
    public Rigidbody rBody;
    //To store the reference to the flock manager
    public FlockingManager manager;
    //Radius in which the members can see eachother
    private float DetectionRadius = 50;
    //Radius in which the members will move away from eachother
    private float protectedRadius = 20;


    //ID
    public int ID;

    //Speed of the entities
    public float speed = 0.2f;


    private Vector3 finalVec;

    
    private Vector3 velocity;

    //hm
    private Vector3 direction = Vector3.zero;
    private Quaternion rotationQuat = new Quaternion (0f,0f,0f,1f);

    
    //Distance between this and another entity
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

    //Function to return random value from a given enum (used for colour randomization)
    //void randomColour()
    //{
    //    Random rnd = new Random();
    //    int test = rnd.Next(0, 10);
    //}


    private colour ownColour = colour.Magenta;

    private colour[] nearbyMembersColour;


    //Amount of flock members nearby
    private int amountNearby = 0;
    //The average position between them
    private Vector3 positionAverage = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<Renderer>();

        //ownColour = RandomEnumValue<colour>();

        //switch (ownColour)
        //{
        //    case colour.Red:
        //        {
        //            ren.material.color = Color.red;
        //            break;
        //        }
        //}




        ren.material.color = Color.blue;
        //Could add scale serialized float to test
        //velocity = new Vector3(Random.value * 1, Random.value * 1, Random.value * 1);
        rBody.AddForce(velocity, ForceMode.VelocityChange);
        
    }

    // Update is called once per frame
    void Update()
    {
        //Resetting Variables
        positionAverage = Vector3.zero;
        amountNearby = 0;
        finalVec = Vector3.zero;




        foreach(var entity in manager.flock)
        {

            //Only starts cohesion calculations once within a certain range of another entity
            distBetween = Vector3.Distance(entity.transform.position, transform.position);

            if ((entity != this) && (distBetween < DetectionRadius))
                {




                //nearbyMembersColour[amountNearby] = entity.ownColour;


                //For some reason the members seem to gravitate towards (0,0,0)?
                //if (distBetween >= DetectionRadius - 15)
                //{
                //    positionAverage += entity.transform.position;
                //}
                //else
                //{
                //    //Seperation
                //    positionAverage -= entity.transform.position;  
                //}



                //Setting colour to show it is following another entity
                //ren.material.color = Color.red;


                amountNearby++;

                //Alignment isn't given it's full name because it looks nicer staggered here, fight me.
                Vector3 align = ComputeAlignment(entity, amountNearby);
                Vector3 cohesion = ComputeCohesion(entity, amountNearby);
                Vector3 seperation = computeSeperation(entity, amountNearby);
                //Obstacle avoidance


                finalVec = align + cohesion + seperation;

                finalVec.Normalize();



                //velocity += finalVec * Time.deltaTime * speed;

                rBody.AddForce(finalVec);

                //transform.position += velocity;

                rBody.MoveRotation(Quaternion.LookRotation(velocity, Vector3.up));

                //Debug.Log("Distance to other: " + Vector3.Distance(entity.transform.position, transform.position));
            }


        }

        ////Finding average position of all members nearby
        //if (amountNearby != 0)
        //{
        //    positionAverage = positionAverage / amountNearby;
        //    Vector3 unitVecTowardsCentre = (1 / (positionAverage - transform.position).magnitude * (positionAverage - transform.position));

        //    if (positionAverage == new Vector3(0, 0, 0))
        //    {
        //        Debug.Log("ID: " + ID + "Velocity: " + unitVecTowardsCentre);
        //        ren.material.color = Color.yellow;
        //    }


        //    //rBody.MoveRotation(Quaternion.LookRotation(unitVecTowardsCentre, Vector3.up));
        //    //rBody.AddForce(unitVecTowardsCentre);
        //}


    }



    Vector3 ComputeAlignment(FlockingEntity flockMember, int amountNear)
    {
        Vector3 alignment = Vector3.zero;

        if (amountNear == 0)
        {
            return Vector3.zero;
        }


        alignment.x += flockMember.rBody.velocity.x;
        alignment.y += flockMember.rBody.velocity.y;
        alignment.z += flockMember.rBody.velocity.z;


        alignment.x /= amountNear;
        alignment.y /= amountNear;
        alignment.z /= amountNear;

        alignment.Normalize();

        return alignment * Time.deltaTime;
    }



    Vector3 ComputeCohesion(FlockingEntity flockMember, int amountNear)
    {
        Vector3 cohesion = Vector3.zero;


        cohesion.x += flockMember.transform.position.x;
        cohesion.y += flockMember.transform.position.y;
        cohesion.z += flockMember.transform.position.z;



        cohesion.x /= amountNear;
        cohesion.y /= amountNear;
        cohesion.z /= amountNear;



        cohesion = new Vector3(cohesion.x - transform.position.x, cohesion.y - transform.position.y, cohesion.z - transform.position.z);


        cohesion.Normalize();

        return cohesion * Time.deltaTime;
    }

    Vector3 computeSeperation(FlockingEntity flockMember, int amountNear)
    {
        Vector3 seperation = Vector3.zero;

        seperation.x += transform.position.x - flockMember.transform.position.x;
        seperation.y += transform.position.y - flockMember.transform.position.y;
        seperation.z += transform.position.z - flockMember.transform.position.z;



        seperation.x *= -1;
        seperation.y *= -1;
        seperation.z *= -1;



        return seperation * Time.deltaTime;
    }




}



