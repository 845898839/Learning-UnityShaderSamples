using UnityEngine;
using System.Collections.Generic;

public struct RayCastingVertexData
{
	public Vector3 position;
	public Vector3 normal;
	public Vector2 uv;
}

public struct RayCastingTriangleData
{
	public int vertexIndex0;
	public int vertexIndex1;
	public int vertexIndex2;
}

public struct RayCastingBodyData
{
	public int diffuseTextureIndex;
	public int meshDataIndex;
}

public struct RayCastingMeshData
{
	public int triangleIndexStart;
	public int triangleIndexEnd;
}

// Class that contains all scene bodies.
public interface IRayCastingBodyManager
{
	List<RayCastingBodyData> BodyDataList { get; }
	List<RayCastingVertexData> VertexDataList { get; }
	List<RayCastingTriangleData> TriangleDataList { get; }
	List<RayCastingMeshData> MeshDataList { get; }
	int BodyCount { get; }

	void AddBody(RayCastingBody body);
	void RemoveBody(RayCastingBody body);
	void CalculateBodies();
}