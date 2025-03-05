using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Flocking
{

    [System.Serializable]
    public struct FlockingWeights
    {
        //
        public float TendencyWeight;
        //How Important staying aligned in direction is - Higher = less turning / random rotation
        public float AlignmentWeight;
        //How much random movement occurs - Higher = more variability between actors
        public float NoiseWeight;

        public static FlockingWeights Default()
        {
            return new FlockingWeights
            {
                TendencyWeight = 1,
                AlignmentWeight = 1,
                NoiseWeight = 1
            };
        }
    }

    [BurstCompile]
    public struct CopyTransformJob : IJobParallelForTransform
    {

        [ReadOnly]
        public NativeArray<float4x4> Src;

        public void Execute(int index, TransformAccess transform)
        {
            var m = Src[index];

            transform.localPosition = m.Position();
            transform.rotation = new quaternion(m);
        }
    }

    [BurstCompile]
    public unsafe struct AverageCenterJob : IJob
    {

        [ReadOnly]
        public NativeArray<float4x4> Matrices;

        [NativeDisableUnsafePtrRestriction]
        public float3* Center;

        public void Execute()
        {
            var center = float3.zero;
            for (int i = 0; i < Matrices.Length; i++)
            {
                float4x4 m = Matrices[i];
                center += m.Position();
            }

            *Center = center /= Matrices.Length;
        }
    }

    [BurstCompile]
    public struct CopyMatrixJob : IJobParallelFor
    {

        [WriteOnly]
        public NativeArray<float4x4> Dst;

        [ReadOnly]
        public NativeArray<float4x4> Src;

        public void Execute(int index)
        {
            Dst[index] = Src[index];
        }
    }

    [BurstCompile]
    public struct BoidJob : IJob
    {

        public FlockingWeights Weights;
        public float Time;
        public float DeltaTime;
        public float MaxDist;
        public float Speed;
        public float RotationSpeed;
        public int Size;
        public float3 Goal;

        [ReadOnly]
        public NativeArray<float> NoiseOffsets;

        [ReadOnly]
        public NativeArray<float4x4> Src;

        [WriteOnly]
        public NativeArray<float4x4> Dst;

        public void Execute()
        {
            for (int m = 0; m < Size; m++)
            {
                var current = Src[m];
                var currentPos = current.Position();
                var perceivedSize = Size - 1;

                Debug.Log("Size: " + perceivedSize);

                var separation = float3.zero;
                var alignment = float3.zero;
                var cohesion = float3.zero;
                var tendency = math.normalizesafe(Goal - currentPos) * Weights.TendencyWeight;

                for (int i = 0; i < Size; i++)
                {
                    if (i == m)
                    {
                        continue;
                    }

                    var b = Src[i];
                    var other = b.Position();

                    // Perform separation
                    separation += TransformExtensions.SeparationVector(currentPos, other, MaxDist);

                    // Perform alignment
                    alignment += b.Forward();

                    // Perform cohesion
                    cohesion += other;
                }

                var avg = 1f / perceivedSize;

                alignment *= avg;
                cohesion *= avg;
                cohesion = math.normalizesafe(cohesion - currentPos);
                var direction = separation +
                                 Weights.AlignmentWeight * alignment +
                                 cohesion +
                                 Weights.TendencyWeight * tendency;

                var targetRotation = current.Forward().QuaternionBetween(math.normalizesafe(direction));
                var finalRotation = current.Rotation();

                if (!targetRotation.Equals(current.Rotation()))
                {
                    finalRotation = math.lerp(finalRotation.value, targetRotation.value, RotationSpeed * DeltaTime);
                }

                var pNoise = math.abs(noise.cnoise(new float2(Time, NoiseOffsets[m])) * 2f - 1f);
                var speedNoise = Speed * (1f + pNoise * Weights.NoiseWeight * 0.9f);
                var finalPosition = currentPos + current.Forward() * speedNoise * DeltaTime;

                Dst[m] = float4x4.TRS(finalPosition, finalRotation, new float3(1));
            }
        }
    }

    [BurstCompile]
    public struct BatchedflockingJob : IJobParallelFor
    {

        public FlockingWeights Weights;
        public float Time;
        public float DeltaTime;
        public float MaxDist;
        public float Speed;
        public float RotationSpeed;
        public int Size;
        public float3 Goal;

        [ReadOnly]
        public NativeArray<float> NoiseOffsets;

        [ReadOnly]
        public NativeArray<float4x4> Src;

        [WriteOnly]
        public NativeArray<float4x4> Dst;

        public void Execute(int index)
        {
            var current = Src[index];
            float3 currentPos = current.Position();
            int perceivedSize = Size - 1;

            float3 separation = float3.zero;
            float3 alignment = float3.zero;
            float3 cohesion = float3.zero;
            var tendency = math.normalizesafe(Goal - currentPos) * Weights.TendencyWeight;

            for (int i = 0; i < Size; i++)
            {
                if (i == index)
                {
                    continue;
                }

                var b = Src[i];
                float3 other = b.Position();

                // Perform separation
                separation += TransformExtensions.SeparationVector(currentPos, other, MaxDist);

                //if (currentPos <= )
                //{

                //}
                // Perform alignment
                alignment += b.Forward();

                // Perform cohesion
                cohesion += other;
            }

            var avg = 1f / perceivedSize;

            alignment *= avg;
            cohesion *= avg;
            cohesion = math.normalizesafe(cohesion - currentPos);
            var direction = separation +
                             Weights.AlignmentWeight * alignment +
                             cohesion +
                             Weights.TendencyWeight * tendency;

            var targetRotation = current.Forward().QuaternionBetween(math.normalizesafe(direction));
            var finalRotation = current.Rotation();

            if (!targetRotation.Equals(current.Rotation()))
            {
                finalRotation = math.lerp(finalRotation.value, targetRotation.value, RotationSpeed * DeltaTime);
            }

            var pNoise = math.abs(noise.cnoise(new float2(Time, NoiseOffsets[index])) * 2f - 1f);
            var speedNoise = Speed * (1f + pNoise * Weights.NoiseWeight * 0.9f);
            var finalPosition = currentPos + current.Forward() * speedNoise * DeltaTime;

            Dst[index] = float4x4.TRS(finalPosition, finalRotation, new float3(1));
        }
    }
}