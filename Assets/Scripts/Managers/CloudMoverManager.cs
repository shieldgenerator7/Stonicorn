using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//2025-01-10: written by following tutorial: https://www.youtube.com/watch?v=1VZaW4_quzI
public class CloudMoverManager: MonoBehaviour
{
    [Header("Shadow ground finding")]
    public float MAX_DISTANCE = 200;
    public float EXTRA_DISTANCE = 10;
    [Tooltip("How many in a batch, ideally a multiple of 2")]
    public int jobCount = 4;

    NativeList<float2> cloudPositions = new NativeList<float2>(Allocator.Persistent);
    NativeList<float2> shadowPositions = new NativeList<float2>(Allocator.Persistent);
    NativeList<float> shadowHeights = new NativeList<float>(Allocator.Persistent);

    List<CloudMover> cloudMovers = new List<CloudMover>();


    private void Start()
    {
        cloudPositions = new NativeList<float2>(Allocator.Persistent);
        shadowPositions = new NativeList<float2>(Allocator.Persistent);
        shadowHeights = new NativeList<float>(Allocator.Persistent);
    }


    private void Update()
    {
        //get cloud movers
        if (cloudMovers.Count == 0)
        {
            cloudMovers = Managers.Object.getObjects<CloudMover>()
                .FindAll(cm => cm.shadow);

            //early exit: no cloud movers
            if (cloudMovers.Count == 0)
            {
                return;
            }
        }

        //get cloud positions
        cloudPositions = cloudMovers
            .ConvertAll(cm => new float2(cm.transform.position.x, cm.transform.position.y))
            .ToNativeList(Allocator.Persistent);

        //cloud mover job stuff
        CloudMoverJob cloudMoverJob = new CloudMoverJob()
        {
            cloudPositions = cloudPositions,
            gravityCenter = Vector2.zero,
            maxRaycastDistance =  MAX_DISTANCE,
            extraShadowDistance = EXTRA_DISTANCE,

            shadowPositions = shadowPositions,
            shadowHeights = shadowHeights,
        };

        JobHandle cloudMoverJobHandle = cloudMoverJob.Schedule(cloudMovers.Count, jobCount);

        cloudMoverJobHandle.Complete();

        for(int i=0; i < cloudMovers.Count; i++)
        {
            cloudMovers[i].acceptJobState(shadowPositions[i], shadowHeights[i]);
        }
    }
}

public struct CloudMoverJob : IJobParallelFor
{
    [ReadOnly]
    public NativeList<float2> cloudPositions;
    [ReadOnly]
    public float2 gravityCenter;
    [ReadOnly]
    public float maxRaycastDistance;
    [ReadOnly]
    public float extraShadowDistance;

    [WriteOnly]
    public NativeList<float2> shadowPositions;
    [WriteOnly]
    public NativeList<float> shadowHeights;

    public void Execute(int index)
    {
        float2 cloudPosition = cloudPositions[index];
        float2 gravityVector = math.normalize(gravityCenter - cloudPosition);

        //find ground point
        Vector2 groundPoint = cloudPosition + (gravityVector * maxRaycastDistance);
        RaycastHit2D rch2d = Utility.RaycastQuestion(
            cloudPosition,
            gravityVector,
            maxRaycastDistance,
            rch2d => !rch2d.collider.isTrigger && !rch2d.collider.GetComponent<Rigidbody2D>()
            );
        groundPoint = rch2d.point;

        //extend shadow
        float distance = Vector2.Distance(groundPoint, cloudPosition) + extraShadowDistance;
        shadowPositions[index] = cloudPosition + (gravityVector * distance / 2);
    }
}
