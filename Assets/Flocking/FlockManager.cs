using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UIElements;
using URandom = UnityEngine.Random;

namespace Flocking
{
    //Unsafe class for pointers
    public unsafe class FlockManager : FlockValues
    {
        [SerializeField]
        NavMesh_Script navMesh;

        //
        public List<Vector3> pathNodes = null;
        private int currentNode = 0;
        private bool gotPath = false;


        public Transform FlockMember;
        public bool LockYPosition;

        private NativeArray<float> noiseOffsets;
        //Input data
        private NativeArray<float4x4> srcMatrices;
        //Output data
        private NativeArray<float4x4> dstMatrices;
        //Y levels of the ground below Boids
        private NativeArray<float> Ylevels;
        private float3[] vertices;

        private Transform[] transforms;
        private TransformAccessArray transformAccessArray;
        private JobHandle flockingHandle;
        private float3* center;
        private MeshFilter[] mfs;


        private void Start()
        {
            //Instantiating all boids
            transforms = new Transform[flockSize];
            srcMatrices = new NativeArray<float4x4>(transforms.Length, Allocator.Persistent);
            dstMatrices = new NativeArray<float4x4>(transforms.Length, Allocator.Persistent);
            noiseOffsets = new NativeArray<float>(transforms.Length, Allocator.Persistent);
            noiseOffsets = new NativeArray<float>(transforms.Length, Allocator.Persistent);
            Ylevels = new NativeArray<float>(transforms.Length, Allocator.Persistent);
            vertices = new float3[flockSize];

            for (int i = 0; i < flockSize; i++)
            {
                var pos = transform.position + URandom.insideUnitSphere * Radius;
                var rotation = Quaternion.Slerp(transform.rotation, URandom.rotation, 0.3f);
                transforms[i] = GameObject.Instantiate(FlockMember, pos, rotation) as Transform;
                srcMatrices[i] = transforms[i].localToWorldMatrix;
                noiseOffsets[i] = URandom.value * 10f;
            }

            //Create the transform access array with a cache of Transforms.
            transformAccessArray = new TransformAccessArray(transforms);

            //To pass from a Job struct back to our MonoBehaviour, we need to use a pointer. In newer packages there is
            //NativeReference<T> which serves the same purpose as a pointer. This allows us to write the position
            //back to our pointer so we can read it later in the main thread to use.
            center = (float3*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<float3>(),
                UnsafeUtility.AlignOf<float3>(),
                Allocator.Persistent);

            //Set the pointer to the float3 to be the default value, or float3.zero.
            UnsafeUtility.MemSet(center, default, UnsafeUtility.SizeOf<float3>());
        }

        private void OnDisable()
        {
            //Before this component is disabled, make sure that all the jobs are completed.
            flockingHandle.Complete();

            //Then we dispose all the NativeArrays we allocate.
            if (srcMatrices.IsCreated)
            {
                srcMatrices.Dispose();
            }

            if (dstMatrices.IsCreated)
            {
                dstMatrices.Dispose();
            }

            if (noiseOffsets.IsCreated)
            {
                noiseOffsets.Dispose();
            }

            if (transformAccessArray.isCreated)
            {
                transformAccessArray.Dispose();
            }

            if (center != null)
            {
                UnsafeUtility.Free(center, Allocator.Persistent);
                center = null;
            }
        }

        private unsafe void Update()
        {
            //At the start of the frame, we ensure that all the jobs scheduled are completed.
            flockingHandle.Complete();

            //Updating destination to next node in path as flock travels
            if (gotPath)
            {
                if(Vector3.Distance(pathNodes[currentNode], this.transform.position) <= 5)
                {
                    currentNode++;
                    Destination.position = pathNodes[currentNode];
                }
            }



            //Getting the nearset vertex (on x, z axis) and mapping the position to the Y value of it.
            //For some reason it will randomly decide to not work upon running the editor occasionally. Don't know why.
            //Just flick the bool on and off.
            if (LockYPosition)
            {
                //Using threaded raycasting for better performance
                var results = new NativeArray<RaycastHit>(dstMatrices.Length, Allocator.TempJob);
                var commands = new NativeArray<RaycastCommand>(dstMatrices.Length, Allocator.TempJob);


                for (int i = 0; i < dstMatrices.Length; i++)
                {
                    Vector3 origin = dstMatrices[i].Position();
                    Vector3 direction = new Vector3(0f, -1f, 0f);
                    //Used this algorithm to figure out layermask via bitshifting, not needed afterwards
                    //int layer = 3;
                    //int layermask = 1 << layer;
                    //layermask = ~layermask;
                    //Debug.Log("Layermask: " + layermask);

                    QueryParameters testquery = new QueryParameters(-9, false, QueryTriggerInteraction.UseGlobal, false);
                    commands[i] = new RaycastCommand(origin, direction, testquery);
                }
                JobHandle handletest = RaycastCommand.ScheduleBatch(commands, results, 1, 1, default(JobHandle));
                handletest.Complete();
                //Annoying to have to do the loop twice but necessary to ensure all threads are completed
                for (int i = 0; i < dstMatrices.Length; i++)
                {
                    Debug.DrawLine(dstMatrices[i].Position(), results[i].point);
                    //Debug.Log("Mesh = " +  results[i].collider.name);
                    if (results[i].collider != null)
                    {
                        Debug.Log("Not null mesh");
                        MeshCollider meshcolliderTest = results[i].collider as MeshCollider;

                        Mesh meshtest = meshcolliderTest.sharedMesh;
                        int[] triangles = meshtest.triangles;
                        Vector3 ClosestVert = meshtest.vertices[GetClosestVertex(results[i], triangles)];
                        Debug.Log("got closest vert");
                        Ylevels[i] = ClosestVert.y;
                        //hit
                    }
                    else
                    {
                        return;
                    }
                }
                results.Dispose();
                commands.Dispose();

                /*Old method for Y locking
                for (int i = 0; i < dstMatrices.Length; i++)
                {
                    Vector3 origin = dstMatrices[i].Position();
                    Vector3 direction = new Vector3(0f, -1f, 0f);
                    //int layer = 3;
                    //int layermask = 1 << layer;
                    //layermask = ~layermask;
                    //Debug.Log("Layermask: " + layermask);

                    QueryParameters testquery = new QueryParameters(-9, false, QueryTriggerInteraction.UseGlobal, false);
                    commands[i] = new RaycastCommand(origin, direction, testquery);


                    if (Physics.Raycast(dstMatrices[i].Position(), new Vector3(0, -1, 0), out RaycastHit hitinfo))
                    {
                        MeshCollider meshCollider = hitinfo.collider as MeshCollider;

                        if (meshCollider == null || meshCollider.sharedMesh == null)
                        {
                            //Debug.Log("Null mesh");
                            Ylevels[i] = 0;
                            return;
                        }

                        Mesh mesh = meshCollider.sharedMesh;
                        int[] triangles = mesh.triangles;
                        Vector3 closestVertex = mesh.vertices[GetClosestVertex(hitinfo, triangles)];

                        //Used for drawing debug gizmos
                        //vertices[i] = closestVertex;
                        Ylevels[i] = closestVertex.y;
                    }
                }*/
            }


            // Write the contents from the pointer back to our position.
            transform.position = *center;

            // Copy the contents from the NativeArray to our TransformAccess
            var copyTransformJob = new CopyTransformJob
            {
                Src = srcMatrices
            }.Schedule(transformAccessArray);

            // Use a separate single thread to calculate the average center of the flock.
            var avgCenterJob = new AverageCenterJob
            {
                Matrices = srcMatrices,
                Center = center,
            }.Schedule();

            JobHandle flockingJob;

            if (LockYPosition)
            {
                //Figure out grabbing the Y position to lock to surface
                flockingJob = new YLocked
                {
                    Weights = Weights,
                    Goal = new float3(Destination.position.x, 1000, Destination.position.z),
                    NoiseOffsets = noiseOffsets,
                    Time = Time.time,
                    DeltaTime = Time.deltaTime,
                    MaxDist = SeparationDistance,
                    Speed = MaxSpeed,
                    RotationSpeed = RotationSpeed,
                    Size = srcMatrices.Length,
                    Src = srcMatrices,
                    Dst = dstMatrices,
                    YInputs = Ylevels
                }.Schedule(transforms.Length, 32);
            }
            else
            {
                flockingJob = new BatchedFlockingJob
                {
                    Weights = Weights,
                    Goal = Destination.position,
                    NoiseOffsets = noiseOffsets,
                    Time = Time.time,
                    DeltaTime = Time.deltaTime,
                    MaxDist = SeparationDistance,
                    Speed = MaxSpeed,
                    RotationSpeed = RotationSpeed,
                    Size = srcMatrices.Length,
                    Src = srcMatrices,
                    Dst = dstMatrices
                }.Schedule(transforms.Length, 32);
            }
           


            // Combine all jobs to a single dependency, so we can pass this single dependency to the
            // CopyMatrixJob. The CopyMatrixJob needs to wait until all jobs are done so we can avoid
            // concurrency issues.
            var combinedJob = JobHandle.CombineDependencies(avgCenterJob, flockingJob, copyTransformJob);

            flockingHandle = new CopyMatrixJob
            {
                Dst = srcMatrices,
                Src = dstMatrices
            }.Schedule(srcMatrices.Length, 32, combinedJob);




        }

        public void UpdateDestination(List <Vector3> newDest)
        {
            Debug.Log("updated dest");
            pathNodes = newDest;
            currentNode = 0;

            Destination.transform.position = pathNodes[currentNode];
            gotPath = true;
        }



        //Finding nearest vertex to Raycast Hit - Not my own creation, credit goes to 'Bunny83'
        //https://discussions.unity.com/t/pinpointing-one-vertice-with-raycasthit/181509
        public static int GetClosestVertex(RaycastHit hitInfo, int[] triangles)
        {
            var b = hitInfo.barycentricCoordinate;
            int index = hitInfo.triangleIndex * 3;
            if (triangles == null || index < 0 || index + 2 >= triangles.Length)
                return -1;

            if (b.x > b.y)
            {
                if (b.x > b.z)
                    return triangles[index]; // x
                else
                    return triangles[index + 2]; // z
            }
            else if (b.y > b.z)
                return triangles[index + 1]; // y
            else
                return triangles[index + 2]; // z
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(Destination.position, 5f);
        }
    }
}