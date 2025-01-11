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

    NativeArray<float2> cloudPositions;
    NativeArray<float2> shadowPositions;
    NativeArray<float> shadowHeights;

    List<CloudMover> cloudMovers = new List<CloudMover>();


    private void Start()
    {
        populateCloudMovers();
    }


    private void Update()
    {
        //get cloud movers
        if (cloudMovers.Count == 0)
        {
            populateCloudMovers();

            //early exit: no cloud movers
            if (cloudMovers.Count == 0)
            {
                return;
            }
        }

        //get cloud positions
        cloudPositions = cloudMovers
            .ConvertAll(cm => new float2(cm.transform.position.x, cm.transform.position.y))
            .ToNativeArray(Allocator.Persistent);

        //cloud mover job stuff
        CloudMoverJob cloudMoverJob = new CloudMoverJob()
        {
            cloudPositions = cloudPositions,
            gravityCenter = Vector2.zero,
            maxRaycastDistance =  MAX_DISTANCE,
            extraShadowDistance = EXTRA_DISTANCE,
            layerMask = LayerMask.NameToLayer("Ground"),

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

    void populateCloudMovers()
    {
        cloudMovers = Managers.Object.getObjects<CloudMover>()
            .FindAll(cm => cm.shadow);
        int count = cloudMovers.Count;

        cloudPositions = new NativeArray<float2>(count, Allocator.Persistent);
        shadowPositions = new NativeArray<float2>(count, Allocator.Persistent);
        shadowHeights = new NativeArray<float>(count, Allocator.Persistent);
    }
}

public struct CloudMoverJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float2> cloudPositions;
    [ReadOnly]
    public float2 gravityCenter;
    [ReadOnly]
    public float maxRaycastDistance;
    [ReadOnly]
    public float extraShadowDistance;
    [ReadOnly]
    public int layerMask;

    [WriteOnly]
    public NativeArray<float2> shadowPositions;
    [WriteOnly]
    public NativeArray<float> shadowHeights;

    public void Execute(int index)
    {
        float2 cloudPosition = cloudPositions[index];
        float2 gravityVector = math.normalize(gravityCenter - cloudPosition);

        //find ground point
        Vector2 groundPoint = cloudPosition + (gravityVector * maxRaycastDistance);
            cloudPosition,
            gravityVector,
            maxRaycastDistance,
        RaycastHit2D rch2d = Physics2D.Raycast(
            layerMask
            );
        groundPoint = rch2d.point;

        //extend shadow
        float distance = Vector2.Distance(groundPoint, cloudPosition) + extraShadowDistance;
        shadowPositions[index] = cloudPosition + (gravityVector * distance / 2);
    }
}
