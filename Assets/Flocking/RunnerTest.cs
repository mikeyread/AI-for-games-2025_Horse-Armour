using UnityEngine;

namespace Flocking
{
    public abstract class RunnerTest : MonoBehaviour
    {

        public FlockingWeights Weights = FlockingWeights.Default();
        public float SeparationDistance = 10f;
        public float Radius = 20f;
        public int flockSize = 512;
        public float MaxSpeed = 6f;
        public float RotationSpeed = 4f;

        [Header("Goal Setting")]
        public Transform Destination;

    }
}