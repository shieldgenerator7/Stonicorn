using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//2025-01-10: written by following tutorial: https://www.youtube.com/watch?v=1VZaW4_quzI
public class CloudMoverManager: MonoBehaviour
{
    public float speed = 0.2f;
    public Vector2 gravityCenter = Vector2.zero;
    [Header("Shadow ground finding")]
    public float MAX_DISTANCE = 200;
    public float EXTRA_DISTANCE = 10;
    [Tooltip("How many in a batch, ideally a multiple of 2")]
    public int jobCount = 4;
    public string layerName = "Ground";

    NativeArray<float2> cloudPositions;
    NativeArray<float2> groundPositions;
    NativeArray<float2> newCloudVelocities;
    NativeArray<float2> newCloudVectorUps;
    NativeArray<float2> shadowPositions;
    NativeArray<float> shadowHeights;

    List<CloudMover> cloudMovers = new List<CloudMover>();

    int layerMask;


    private void Start()
    {
        populateCloudMovers();
        layerMask = LayerMask.NameToLayer(layerName);
    }

    private void FixedUpdate()
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

        //cloud mover move job stuff
        CloudMoverMoveJob cloudMoverMoveJob = new CloudMoverMoveJob()
        {
            cloudPositions = cloudPositions,
            gravityCenter = Vector2.zero,
            speed = speed,

            newCloudVelocities = newCloudVelocities,
            newCloudVectorUps = newCloudVectorUps,
        };

        JobHandle cloudMoverShadowJobHandle = cloudMoverMoveJob.Schedule(cloudMovers.Count, jobCount);

        cloudMoverShadowJobHandle.Complete();

        for (int i = 0; i < cloudMovers.Count; i++)
        {
            cloudMovers[i].acceptMoveJobState(newCloudVelocities[i], newCloudVectorUps[i]);
        }
    }


    private void LateUpdate()
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

        //raycast job stuff
        int count = cloudMovers.Count;

        for (int i = 0; i < count; i++)
        {
            var cloudMover = cloudMovers[i];
            Vector2 cloudPosition = cloudMover.transform.position;
            Vector2 gravityVector = (gravityCenter - cloudPosition).normalized;

            //RaycastHit2D rch2d = Physics2D.Raycast(
            //    cloudPosition + (gravityVector * 10),
            //    gravityVector,
            //    MAX_DISTANCE,
            //    layerMask
            //);
            RaycastHit2D rch2d = Utility.RaycastQuestion(
                cloudPosition + (gravityVector * 10),
                gravityVector,
                MAX_DISTANCE,
                rch2d => rch2d.collider.gameObject.layer == layerMask
            );
            groundPositions[i] = rch2d.point;
        }

        //cloud mover shadow job stuff
        CloudMoverShadowJob cloudMoverShadowJob = new CloudMoverShadowJob()
        {
            cloudPositions = cloudPositions,
            groundPositions = groundPositions,
            gravityCenter = Vector2.zero,
            maxRaycastDistance = MAX_DISTANCE,
            extraShadowDistance = EXTRA_DISTANCE,
            layerMask = layerMask,

            shadowPositions = shadowPositions,
            shadowHeights = shadowHeights,
        };

        JobHandle cloudMoverShadowJobHandle = cloudMoverShadowJob.Schedule(cloudMovers.Count, jobCount);

        cloudMoverShadowJobHandle.Complete();

        for(int i=0; i < cloudMovers.Count; i++)
        {
            cloudMovers[i].acceptShadowJobState(shadowPositions[i], shadowHeights[i]);
        }
    }

    void populateCloudMovers()
    {
        cloudMovers = Managers.Object.getObjects<CloudMover>()
            .FindAll(cm => cm.shadow);
        int count = cloudMovers.Count;

        cloudPositions = new NativeArray<float2>(count, Allocator.Persistent);
        groundPositions = new NativeArray<float2>(count,Allocator.Persistent);
        newCloudVelocities = new NativeArray<float2>(count, Allocator.Persistent);
        newCloudVectorUps = new NativeArray<float2>(count, Allocator.Persistent);
        shadowPositions = new NativeArray<float2>(count, Allocator.Persistent);
        shadowHeights = new NativeArray<float>(count, Allocator.Persistent);
    }
}

public struct CloudMoverMoveJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float2> cloudPositions;
    [ReadOnly]
    public float2 gravityCenter;
    [ReadOnly]
    public float speed;

    [WriteOnly]
    public NativeArray<float2> newCloudVelocities;
    [WriteOnly]
    public NativeArray<float2> newCloudVectorUps;

    public void Execute(int index)
    {
        float2 cloudPosition = cloudPositions[index];
        float2 gravityVector = math.normalize(gravityCenter - cloudPosition);
        Vector2 sideVector = new Vector3(-gravityVector.y, gravityVector.x) / Mathf.Sqrt(gravityVector.x * gravityVector.x + gravityVector.y * gravityVector.y);
        newCloudVelocities[index] = sideVector.normalized * speed;
        newCloudVectorUps[index] = -gravityVector;
    }
}

public struct CloudMoverShadowJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float2> cloudPositions;
    [ReadOnly]
    public NativeArray<float2> groundPositions;
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
        Vector2 groundPoint = groundPositions[index];

        //extend shadow
        float distance = Vector2.Distance(groundPoint, cloudPosition) + extraShadowDistance;
        shadowPositions[index] = cloudPosition + (gravityVector * distance / 2);
        shadowHeights[index] = distance;
    }
}
