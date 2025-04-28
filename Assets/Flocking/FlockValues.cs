using UnityEngine;

namespace Flocking
{
    public abstract class FlockValues : MonoBehaviour
    {

        public FlockingWeights Weights = FlockingWeights.Default();
        //How close can they get before seperating
        public float SeparationDistance = 10f;
        //Radius of initial spawn locations
        public float Radius = 20f;
        //How many entities to spawn
        public int flockSize = 512;
        //Speed
        public float MaxSpeed = 6f;
        //RotationSpeed
        public float RotationSpeed = 4f;

        [Header("Goal Setting")]
        public Transform Destination;

    }
}