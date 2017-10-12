using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class RayCastingBody : MonoBehaviour
{
	private List<RayCastingTriangleData> triangles;
	private List<RayCastingVertexData> vertices;
	private int meshHashCode;

	public List<RayCastingTriangleData> Triangles { get { return triangles; } }
	public List<RayCastingVertexData> Vertices { get { return vertices; } }
	public int MeshHashCode { get { return meshHashCode; } }

	private void Start()
	{
		triangles = new List<RayCastingTriangleData>();
		vertices = new List<RayCastingVertexData>();

		var meshFilter = GetComponent<MeshFilter>();
		var mesh = meshFilter.sharedMesh;
		GenerateVerticesAndTriangles(mesh);
	}

	private void OnEnable()
	{
		RayCastingBodyManager.Instance.AddBody(this);
	}

	private void OnDisable()
	{
		RayCastingBodyManager.Instance.RemoveBody(this);
	}

	private void GenerateVerticesAndTriangles(Mesh mesh)
	{
		meshHashCode = mesh.GetHashCode();

		// Generating the triangles
		for (int i = 0; i < mesh.triangles.Length; i+=3)
		{
			var triangle = new RayCastingTriangleData();
			triangle.vertexIndex0 = mesh.triangles[i + 0];
			triangle.vertexIndex1 = mesh.triangles[i + 1];
			triangle.vertexIndex2 = mesh.triangles[i + 2];
			triangles.Add(triangle);
		}

		// Generating the vertices
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			var vertex = new RayCastingVertexData();
			vertex.position = mesh.vertices[i];
			vertex.normal = mesh.normals[i];
			vertex.uv = mesh.uv[i];
			vertices.Add(vertex);
		}
	}
}
