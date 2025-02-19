using UnityEngine;

namespace ThousandAnt.Boids
{
    public abstract class RunnerTest : MonoBehaviour
    {

        public BoidWeights Weights = BoidWeights.Default();
        public float SeparationDistance = 10f;
        public float Radius = 20f;
        public int Size = 512;
        public float MaxSpeed = 6f;
        public float RotationSpeed = 4f;

        [Header("Goal Setting")]
        public Transform Destination;

    }
}