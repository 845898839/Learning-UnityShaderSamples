using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// This class object is to create compute buffers with required data
// for raycasting.
public class RayCastingBodyManager : MonoBehaviour, IRayCastingBodyManager
{
	private const bool EXCLUDE_SAME_MESH = false;
	public static IRayCastingBodyManager Instance { get; private set; }

	private List<RayCastingBody> bodyList;
	private Dictionary<int, int> meshHashCodeToIndex;

	private List<RayCastingBodyData> bodyDataList;
	private List<RayCastingVertexData> vertexDataList;
	private List<RayCastingTriangleData> triangleDataList;
	private List<RayCastingMeshData> meshDataList;

	public List<RayCastingBodyData> BodyDataList { get { return bodyDataList; } }
	public List<RayCastingVertexData> VertexDataList { get { return vertexDataList; } }
	public List<RayCastingTriangleData> TriangleDataList { get { return triangleDataList; } }
	public List<RayCastingMeshData> MeshDataList { get { return meshDataList; } }
	public int BodyCount { get { return bodyList.Count; } }

	private void Awake()
	{
		//Debug.Assert(Instance == null, "Scene can only contain one body manager.");

		Instance = this;
		bodyList = new List<RayCastingBody>();
	}

	private void Start()
	{
		bodyDataList = new List<RayCastingBodyData>();
		vertexDataList = new List<RayCastingVertexData>();
		triangleDataList = new List<RayCastingTriangleData>();
		meshDataList = new List<RayCastingMeshData>();

		meshHashCodeToIndex = new Dictionary<int, int>();
	}

	private void OnDestroy()
	{
		// Release the singleton.
		//Instance = null;
	}

	public void CalculateBodies()
	{
		// Clean all lists.
		// It would be better to use Array, because unity doesn't have
		// solution to update compute buffer with List.
		bodyDataList.Clear();
		vertexDataList.Clear();
		triangleDataList.Clear();
		meshDataList.Clear();

		if (EXCLUDE_SAME_MESH)
			meshHashCodeToIndex.Clear();

		// Iterate the bodies and fill they mesh/triangles/vertex data to list on cpu.
		foreach (var body in bodyList)
		{
			var meshDataIndex = -1;
			if (meshHashCodeToIndex.ContainsKey(body.MeshHashCode) && EXCLUDE_SAME_MESH)
			{
				// Find mesh that its already in list.
				meshDataIndex = meshHashCodeToIndex[body.MeshHashCode];
			}
			else
			{
				// Creates mesh data and adds it to list.
				meshDataIndex = CreateAndAddMeshDataReturnIndex(body.MeshHashCode, body.transform, body.Triangles, body.Vertices);
			}

			var bodyData = new RayCastingBodyData();
			bodyData.meshDataIndex = meshDataIndex;
			bodyData.diffuseTextureIndex = -1;
			bodyDataList.Add(bodyData);
		}
	}

	private int CreateAndAddMeshDataReturnIndex(int meshHashCode, Transform transform, List<RayCastingTriangleData> triangleDataList,
		List<RayCastingVertexData> vertexDataList)
	{
		// Calculate vertices and add.
		var vertexOffsetBeforeNewVertices = this.vertexDataList.Count;
		for (int i = 0; i < vertexDataList.Count; i++)
		{
			var vertexData = vertexDataList[i];
			vertexData.position = transform.TransformPoint(vertexData.position);
			this.vertexDataList.Add(vertexData);
		}

		// Calculate triangles and add.
		var triangleOffsetBeforeNewTriangles = this.triangleDataList.Count;
		for (int i = 0; i < triangleDataList.Count; i++)
		{
			var triangleData = triangleDataList[i];
			// All scene vertices are batched into one Array, so we need offseting for this reason.
			triangleData.vertexIndex0 += vertexOffsetBeforeNewVertices;
			triangleData.vertexIndex1 += vertexOffsetBeforeNewVertices;
			triangleData.vertexIndex2 += vertexOffsetBeforeNewVertices;
			this.triangleDataList.Add(triangleData);
		}

		var meshData = new RayCastingMeshData();
		meshData.triangleIndexStart = triangleOffsetBeforeNewTriangles;
		meshData.triangleIndexEnd = triangleOffsetBeforeNewTriangles + triangleDataList.Count;

		var meshDataIndex = meshDataList.Count;
		if (EXCLUDE_SAME_MESH)
			meshHashCodeToIndex.Add(meshHashCode, meshDataIndex);
		meshDataList.Add(meshData);
		return meshDataIndex;
	}

	// Include body into calculation.
	// TODO: Cache.
	public void AddBody(RayCastingBody body)
	{
		Debug.Assert(!bodyList.Contains(body), "The body is already added.");

		bodyList.Add(body);
	}

	// TODO: Cache.
	public void RemoveBody(RayCastingBody body)
	{
		Debug.Assert(bodyList.Contains(body));

		bodyList.Remove(body);
	}
}
