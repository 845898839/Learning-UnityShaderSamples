using UnityEngine;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class ChangeMeshTopology : MonoBehaviour
{
	public MeshTopology targetTopology;
	public Mesh sourceMesh;

	private void OnEnable()
	{
		if (sourceMesh == null)
			return;

		var mesh = GenerateMeshWithTopology(sourceMesh, targetTopology);
		SetMesh(mesh);
	}

	private Mesh GenerateMeshWithTopology(Mesh sourceMesh, MeshTopology targetTopology)
	{
		var mesh = new Mesh();
		mesh.vertices = sourceMesh.vertices;
		mesh.normals = sourceMesh.normals;
		mesh.uv = sourceMesh.uv;

		for (int i = 0; i < sourceMesh.subMeshCount; i++)
		{
			var sourceTopology = sourceMesh.GetTopology(i);
			var sourceIndices = sourceMesh.GetIndices(i);
			var targetIndices = new List<int>();
			ConvertTopologyAndFillTheIndicesStream(sourceIndices, sourceTopology, targetIndices, targetTopology);
			mesh.SetIndices(targetIndices.ToArray(), targetTopology, i);
		}
		return mesh;
	}

	private void ConvertTopologyAndFillTheIndicesStream(int[] sourceIndices, MeshTopology sourceTopology,
		List<int> targetIndices, MeshTopology topology)
	{
		if (sourceTopology == MeshTopology.Triangles && targetTopology == MeshTopology.Lines)
		{
			for (int j = 0; j < sourceIndices.Length; j += 3)
			{
				targetIndices.Add(sourceIndices[j]);
				targetIndices.Add(sourceIndices[j + 1]);
				targetIndices.Add(sourceIndices[j + 1]);
				targetIndices.Add(sourceIndices[j + 2]);
				targetIndices.Add(sourceIndices[j + 2]);
				targetIndices.Add(sourceIndices[j]);
			}
			return;
		}

		// TODO: Create other implementations
		throw new NotImplementedException();
	}

	private void SetMesh(Mesh mesh)
	{
		var meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}

	private void OnDisable()
	{
		SetMesh(sourceMesh);
	}
}
