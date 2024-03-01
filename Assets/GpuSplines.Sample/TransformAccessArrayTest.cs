using PeDev.GpuSplines;
using Stella3D;
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

namespace PeDev.GpuSplines.Sample {
    public class TransformAccessArrayTest : MonoBehaviour {
	    public Color lineColor = Color.white;
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
	    private Random m_Random;

	    public bool useNestedArrayInput = false;
	    private NativeArray<BatchSplineInput> m_Splines;
	    private SharedArray<Vector3, float3> m_SharedPositions;
	    
	    private NativeArray<BatchSplineInputWithArray> m_Splines2;
	    private NativeArray<float3>[] m_SplineControlPoints2;
	    private NativeArray<int> m_IndexSplines;
	    private NativeArray<int> m_IndexControlPoints;

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
		    
		    m_Splines2 = new NativeArray<BatchSplineInputWithArray>(splineCount, Allocator.Persistent);
		    m_SplineControlPoints2 = new NativeArray<float3>[splineCount];

		    m_IndexSplines = new NativeArray<int>(objectCount, Allocator.Persistent);
		    m_IndexControlPoints = new NativeArray<int>(objectCount, Allocator.Persistent);
		    
		    int[] slots = GetSplitSlots(splineCount, objectCount, 2);
		    
		    int startIndex = 0;
		    for (int i = 0; i < splineCount; i++) {
			    int numControlPoints = Mathf.Clamp(slots[i], 2, 998);
			    //Debug.Log($"startIndex = {startIndex}, num = {numControlPoints}");
			    var entity = m_Context.AddSpline(
				    positions, startIndex, 
				    numControlPoints, true, 10, 
				    UnityEngine.Random.Range(splineWidthRandomMin, splineWidthRandomMax), lineColor, SplineType.CatmullRom);

			    m_Splines[i] = new BatchSplineInput() {
				    entity = entity, startIndex = startIndex, numControlPoints = numControlPoints
			    };
			    
			    m_SplineControlPoints2[i] = new NativeArray<float3>(numControlPoints, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			    for (int j = 0; j < numControlPoints; j++) {
				    m_SplineControlPoints2[i][j] = positions[startIndex + j];

				    m_IndexSplines[startIndex + j] = i;
				    m_IndexControlPoints[startIndex + j] = j;
			    }
			    m_Splines2[i] = new BatchSplineInputWithArray() {
				    entity = entity, inputControlPoints = m_SplineControlPoints2[i], numControlPoints = numControlPoints,
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
		    for (int i = 0; i < m_SplineControlPoints2.Length; i++) {
			    m_SplineControlPoints2[i].Dispose();
		    }
		    m_Splines2.Dispose();
		    m_TransformAccessArray.Dispose();
		    m_IndexSplines.Dispose();
		    m_IndexControlPoints.Dispose();
	    }

	    private void Update() {
		    if (!m_JobHandle.IsCompleted) {
			    m_JobHandle.Complete();
		    }
		    Profiler.BeginSample("Update Jobs"); 

		    Profiler.BeginSample("Job Schedules");
		    if (randomMove) {
			    // Randomly move.
			    // m_JobHandle = new RadnomMoveUpdateJob() {
				   //  random = m_Random, 
				   //  deltaTime = Time.deltaTime * moveSpeed,
			    // }.Schedule(m_TransformAccessArray);
			    
			    m_JobHandle = new MoveUpdateJob() {
				    speed = moveSpeed,
				    deltaTime = Time.deltaTime,
				    time = Time.time,
				    count = m_TransformAccessArray.length,
				    random = m_Random
			    }.Schedule(m_TransformAccessArray);
		    }

		   

		    var splineContextJobified = m_Context.BeginJobifiedContext(Allocator.TempJob);
		    if (useNestedArrayInput) {
			    // Copy transform.position to m_SharedPositions.
			    m_JobHandle = new CopyTransformPositionToBatchSplineInputWithArrayJob() {
				    destination = m_Splines2,
				    batchIndices = m_IndexSplines,
				    controlPointIndices = m_IndexControlPoints
			    }.Schedule(m_TransformAccessArray, m_JobHandle);
			    // Jobified update spline control points.
			    m_JobHandle = new ModifySplineControlPointsWithNestedArraysJob() {
				    inputs = m_Splines2,
				    insertFirstLastPoints = true,
				    splineContext = splineContextJobified,
			    }.Schedule(m_Splines.Length, 4, m_JobHandle);
		    } else {
			    // Copy transform.position to m_SharedPositions.
			    m_JobHandle = new CopyTransformPositionJob() {
				    destination = m_SharedPositions
			    }.Schedule(m_TransformAccessArray, m_JobHandle);
			    // Jobified update spline control points.
			    m_JobHandle = new ModifySplineControlPointsJob() {
				    inputEntities = m_Splines,
				    inputControlPoints = m_SharedPositions,
				    insertFirstLastPoints = true,
				    splineContext = splineContextJobified,
			    }.Schedule(m_Splines.Length, 4, m_JobHandle);
		    }
		   
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
	    struct RadnomMoveUpdateJob : IJobParallelForTransform {
		    public Random random;
		    public float deltaTime;
		    
		    public void Execute(int index, TransformAccess transform) {
			    float3 newPosition = (float3)transform.position + random.NextFloat3Direction() * deltaTime;
			    transform.position = newPosition;
		    }
	    }
	    
	    [BurstCompile]
	    struct MoveUpdateJob : IJobParallelForTransform {
		    public float speed;
		    public float deltaTime;
		    public float time;
		    public float count;
		    public Random random;
		    
		    private static float Hash1d(float u)
		    {
			    return math.frac(math.sin(u)*143.9f);	// scale this down to kill the jitters
		    }
		    
		    public void Execute(int index, TransformAccess transform) {
			    float3 pos = (float3)transform.position;
			    float3 dir = math.normalize(pos);
			    float3 tan = math.normalize(math.cross(dir, math.up()));
			    float rand = Hash1d((float)index / (count - 1));

			    float duration = math.lerp(1f, 2f, rand);
			    float sign = Mathf.PingPong(time / duration, 1f) * 2 - 1f;
			    dir = sign * dir;
			    tan = tan * (index % 2 == 0 ? 1.0f : -1.0f);
			    float spd = speed * math.lerp(0.1f, 10f, Hash1d(time / duration));
			    float3 newPosition = (float3)transform.position + (dir*speed + tan * speed) * deltaTime;
			    transform.position = newPosition;
		    }
	    }
    }
}