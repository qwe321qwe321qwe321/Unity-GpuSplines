using PeDev.GpuSplines;
using Stella3D;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Random = Unity.Mathematics.Random;

namespace PeDev {
    public class TransformAccessArrayTest : MonoBehaviour {
	    public bool createPrimitive = true;
	    public float initialRadius = 10f;
	    public int objectCount = 1000;
	    public bool randomMove = false;
	    public float moveSpeed = 5f;
	    public int splineCount = 10;
	    public float splineWidthRandomMin = 0.1f;
	    public float splineWidthRandomMax = 0.5f;

	    private readonly List<GameObject> m_ObjectLists = new List<GameObject>();
	    private TransformAccessArray m_TransformAccessArray;
	    private JobHandle m_JobHandle;
	    private MoveUpdateJob m_MoveUpdateJob;
	    private Random m_Random;

	    private NativeArray<BatchSplineInput> m_Splines;
	    private SharedArray<Vector3, float3> m_SharedPositions;

	    private GpuSplineContext m_Context => GpuSplineManager.Instance.Context;

	    private void Awake() {
		    // m_Context = new GpuSplineContext()
			   //  .SetDrawMode(GpuSplineContext.DrawMode.DrawMesh)
			   //  .SetOptimizeLinearVertices(true);
	    }

	    private void Start() {
		    if (m_TransformAccessArray.isCreated) {
			    m_TransformAccessArray.Dispose();
		    }
		    m_TransformAccessArray = new TransformAccessArray(objectCount);
		    m_SharedPositions = new SharedArray<Vector3, float3>(objectCount);
		    Vector3[] positions = m_SharedPositions;
		    for (int i = 0; i < objectCount; i++) {
			    GameObject newObject = createPrimitive ? GameObject.CreatePrimitive(PrimitiveType.Cube) : new GameObject();
			    newObject.name = i.ToString();
			    newObject.transform.SetParent(this.transform);
			    newObject.transform.localPosition = UnityEngine.Random.insideUnitSphere * initialRadius;
			    m_ObjectLists.Add(newObject);
			    m_TransformAccessArray.Add(newObject.transform);
			    positions[i] = newObject.transform.position;
		    }

		    m_Random = new Random((uint)UnityEngine.Random.Range(0, 10000));


		    m_Splines = new NativeArray<BatchSplineInput>(splineCount, Allocator.Persistent);
		    int[] slots = GetSplitSlots(splineCount, objectCount, 2);
		    int startIndex = 0;
		    for (int i = 0; i < splineCount; i++) {
			    int numControlPoints = Mathf.Clamp(slots[i], 2, 998);
			    Debug.Log($"startIndex = {startIndex}, num = {numControlPoints}");
			    var entity = m_Context.AddSpline(
				    positions, startIndex, 
				    numControlPoints, true, 10, 
				    UnityEngine.Random.Range(splineWidthRandomMin, splineWidthRandomMax), Color.yellow, SplineType.CatmullRom);
			    m_Splines[i] = new BatchSplineInput() {
				    entity = entity, startIndex = startIndex, numControlPoints = numControlPoints,
			    };

			    startIndex += numControlPoints;
		    }
	    }

	    private static int[] GetSplitSlots(int slotCount, int arrayLength, int minSize) {
		    if (slotCount == 1) {
			    return new[] { arrayLength };
		    }
		    int[] split = new int[slotCount - 1];
		    int randomMax = arrayLength - minSize * slotCount;
		    for (int i = 0; i < split.Length; i++) {
			    split[i] = UnityEngine.Random.Range(0, randomMax);
		    }
		    split = split.OrderBy(x => x).ToArray();

		    int[] slots = new int[slotCount];
		    slots[0] = split[0];
		    for (int i = 1; i < slots.Length - 1; i++) {
			    slots[i] = split[i] - split[i - 1];
		    }
		    slots[slotCount - 1] = randomMax - split[slotCount - 2];

		    for (int i = 0; i < slots.Length; i++) {
			    slots[i] += minSize;
		    }
		    return slots;
	    }

	    private void OnDestroy() {
		    m_Splines.Dispose();
		    m_TransformAccessArray.Dispose();
	    }

	    private void Update() {
		    if (!m_JobHandle.IsCompleted) {
			    m_JobHandle.Complete();
		    }
		    Profiler.BeginSample("Update Jobs"); 

		    Profiler.BeginSample("Job Schedules");
		    if (randomMove) {
			    m_JobHandle = new MoveUpdateJob() {
				    random = m_Random, 
				    deltaTime = Time.deltaTime * moveSpeed,
			    }.Schedule(m_TransformAccessArray);
		    }

		    m_JobHandle = new CopyTransformToArrayJob() {
			    nativePositions = m_SharedPositions
		    }.Schedule(m_TransformAccessArray, m_JobHandle);

		    var splineContextJobified = m_Context.BeginJobifiedContext(Allocator.TempJob);
		    m_JobHandle = new UpdateSplineJob() {
			    inputEntities = m_Splines,
			    inputControlPoints = m_SharedPositions,
			    insertFirstLastPoints = true,
			    splineContext = splineContextJobified,
		    }.Schedule(m_Splines.Length, 4, m_JobHandle);
		    Profiler.EndSample();

		    m_JobHandle.Complete();
		    m_Context.EndJobifiedContext(splineContextJobified);
		    Profiler.EndSample();

		    
		    Profiler.BeginSample("Spline Update"); 
		    m_Context.Update();
		    Profiler.EndSample();

		    m_Random.NextUInt();
	    }
	    
	    [BurstCompile]
	    struct MoveUpdateJob : IJobParallelForTransform {
		    public Random random;
		    public float deltaTime;
		    
		    public void Execute(int index, TransformAccess transform) {
			    float3 newPosition = (float3)transform.position + random.NextFloat3Direction() * deltaTime;
			    transform.position = newPosition;
		    }
	    }

	    [BurstCompile]
	    struct CopyTransformToArrayJob : IJobParallelForTransform {
		    public NativeArray<float3> nativePositions;

		    public void Execute(int index, TransformAccess transform) {
			    nativePositions[index] = transform.position;
		    }
	    }

	    struct BatchSplineInput {
		    public SplineEntity entity;
		    public int startIndex;
		    public int numControlPoints;
	    }
	    
	    [BurstCompile]
	    struct UpdateSplineJob : IJobParallelFor {
		    [ReadOnly]
		    public NativeArray<BatchSplineInput> inputEntities;
		    [ReadOnly]
		    public NativeArray<float3> inputControlPoints;
		    
		    public bool insertFirstLastPoints;
		    
		    public GpuSplineContext.JobifiedContext splineContext;

		    
		    public void Execute(int index) {
			    BatchSplineInput input = inputEntities[index];
			    SplineEntity entity = input.entity;
			    splineContext.ModifyPoints(entity, inputControlPoints, input.startIndex, input.numControlPoints, true);
		    }
	    }
    }
}