using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

//2025-01-10: written by following tutorial: https://www.youtube.com/watch?v=1VZaW4_quzI
public class CloudMoverManager: MonoBehaviour
{
    [Header("Shadow ground finding")]
    public float MAX_DISTANCE = 200;
    public float EXTRA_DISTANCE = 10;

    NativeList<float2> cloudPositions = new NativeList<float2>(Allocator.Persistent);
    NativeList<float2> shadowPositions = new NativeList<float2>(Allocator.Persistent);
    NativeList<float> shadowHeights = new NativeList<float>(Allocator.Persistent);

    private void Start()
    {
        cloudPositions = new NativeList<float2>(Allocator.Persistent);
        shadowPositions = new NativeList<float2>(Allocator.Persistent);
        shadowHeights = new NativeList<float>(Allocator.Persistent);
    }


    private void Update()
    {
        List<CloudMover> cloudMovers = Managers.Object.getObjects<CloudMover>()
            .FindAll(cm => cm.shadow);
        NativeList<float2> cloudPositions = cloudMovers
            .ConvertAll(cm => new float2(cm.transform.position.x, cm.transform.position.y))
            .ToNativeList(Allocator.Persistent);

        CloudMoverJob cloudMoverJob = new CloudMoverJob()
        {
            cloudPositions = cloudPositions,
            gravityCenter = Vector2.zero,
            maxRaycastDistance =  MAX_DISTANCE,
            extraShadowDistance = EXTRA_DISTANCE,

            shadowPositions = shadowPositions,
            shadowHeights = shadowHeights,
        };

        JobHandle cloudMoverJobHandle = cloudMoverJob.Schedule(cloudMovers.Count, 4);

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
