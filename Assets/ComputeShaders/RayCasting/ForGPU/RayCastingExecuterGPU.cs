using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public struct Vector3Uint
{
	public uint x;
	public uint y;
	public uint z;
}

// Ray casting with compute shaders.
public class RayCastingExecuterGPU : MonoBehaviour, IRayCastingExecuter
{
	private const int MAXIMUM_BODY_COUNT = 100;
	private const int MAXIMUM_VERTEX_COUNT = 1000;
	private const int MAXIMUM_TRIANGLE_COUNT = 1000;
	private const int MAXIMUM_MESH_COUNT = 100;
	private const int SHOW_RAYS_IN_SCREEN_VIEW_MAXIMUM_COUNT = 100;
	private const int THREAD_PER_GROUP = 32;
	private const string SHADER_KERNEL_NAME = "main";

	public static IRayCastingExecuter Instance { get; private set; }

	public ComputeShader shaderForRayCasting;

	private ComputeBuffer bodyDataBuffer;
	private ComputeBuffer vertexDataBuffer;
	private ComputeBuffer triangleDataBuffer;
	private ComputeBuffer meshDataBuffer;
	private ComputeBuffer rayDataBuffer;

	private int kernelIndexOfMain;
	private Vector3Uint threadsPerThreadGroup;

	private void Awake()
	{
		Debug.Assert(Instance == null, "Scene can only contain one executer.");

		Instance = this;
	}

	private void Start()
	{
		bodyDataBuffer = new ComputeBuffer(MAXIMUM_BODY_COUNT, Marshal.SizeOf(typeof(RayCastingBodyData)));
		vertexDataBuffer = new ComputeBuffer(MAXIMUM_VERTEX_COUNT, Marshal.SizeOf(typeof(RayCastingVertexData)));
		triangleDataBuffer = new ComputeBuffer(MAXIMUM_TRIANGLE_COUNT, Marshal.SizeOf(typeof(RayCastingTriangleData)));
		meshDataBuffer = new ComputeBuffer(MAXIMUM_MESH_COUNT, Marshal.SizeOf(typeof(RayCastingMeshData)));
		PrepareShaderForRendering();
	}

	private void PrepareShaderForRendering()
	{
		Debug.Assert(shaderForRayCasting != null);
		Debug.Assert(shaderForRayCasting.HasKernel(SHADER_KERNEL_NAME));

		// Find the main functiona that will be used for dispatch and setting the data.
		kernelIndexOfMain = shaderForRayCasting.FindKernel(SHADER_KERNEL_NAME);

		// Returns the thread group sizes from compute shader code "[numthreads(32, 32, 1)]".
		shaderForRayCasting.GetKernelThreadGroupSizes(kernelIndexOfMain,
			out threadsPerThreadGroup.x,
			out threadsPerThreadGroup.y,
			out threadsPerThreadGroup.z);

		Debug.Assert(
			threadsPerThreadGroup.x == THREAD_PER_GROUP &&
			threadsPerThreadGroup.y == THREAD_PER_GROUP &&
			threadsPerThreadGroup.z == 1, "Incorrect thread group values");
	}

	private void OnDestroy()
	{
		// Release the singleton
		Instance = null;

		// It is required to manualy destroy compute buffers.
		bodyDataBuffer.Release();
		vertexDataBuffer.Release();
		triangleDataBuffer.Release();
		meshDataBuffer.Release();
		if (rayDataBuffer != null)
			rayDataBuffer.Release();
	}

	public void Render(IRayCastingCamera camera)
	{
		Debug.Assert(camera != null);
		// TODO: Execute in different threads, these tasks area really cpu-heavy.

		// Update all compute buffers with mesh data.
		var bodyManager = RayCastingBodyManager.Instance; // We use singleton, because unity allows only one scene at the time.
		bodyManager.CalculateBodies();

		// Update all rays that will be used for casting.
		camera.CalculateRays();

		// Update all compute buffers with calulcate information on GPU.
		UpdateComputeBuffers(camera);

		// Update shader properties, with updated compute buffers.
		SetAllPropertiesForShader(camera);

		// Run shader on gpu.
		DispatchComputeShader(camera);
	}

	private void UpdateComputeBuffers(IRayCastingCamera camera)
	{
		var bodyManager = RayCastingBodyManager.Instance;

		// Lets check if we don't exceed the buffer capcities.
		Debug.Assert(bodyDataBuffer.count >= bodyManager.BodyDataList.Count, 
			"There is to many bodies, you can change the maximum value in this script");
		Debug.Assert(vertexDataBuffer.count >= bodyManager.VertexDataList.Count,
			"There is to many vertices, you can change the maximum value in this script");
		Debug.Assert(triangleDataBuffer.count >= bodyManager.TriangleDataList.Count,
			"There is to many triangles, you can change the maximum value in this script");
		Debug.Assert(meshDataBuffer.count >= bodyManager.MeshDataList.Count,
			"There is to many meshes, you can change the maximum value in this script");

		// Update the buffers on gpu.
		SetDataForRayDataBuffer(camera);
		// TODO: make other buffers resizable too.
		bodyDataBuffer.SetData(bodyManager.BodyDataList.ToArray());
		vertexDataBuffer.SetData(bodyManager.VertexDataList.ToArray());
		triangleDataBuffer.SetData(bodyManager.TriangleDataList.ToArray());
		meshDataBuffer.SetData(bodyManager.MeshDataList.ToArray());
	}

	private void SetDataForRayDataBuffer(IRayCastingCamera camera)
	{
		// There could be multiple resolution cameras, so in this function we decide if we need bigger buffer.
		var width = camera.Width;
		var height = camera.Height;
		var totalPixelCount = width * height;

		if (rayDataBuffer == null || rayDataBuffer.count < totalPixelCount)
		{
			rayDataBuffer = new ComputeBuffer(totalPixelCount, Marshal.SizeOf(typeof(RayCastingRayData)));
		}

		if (rayDataBuffer != null && rayDataBuffer.count < totalPixelCount)
		{
			rayDataBuffer.Release();
			rayDataBuffer = new ComputeBuffer(totalPixelCount, Marshal.SizeOf(typeof(RayCastingRayData)));
		}

		rayDataBuffer.SetData(camera.RayDatas);
	}

	// TODO: Cache.
	private void SetAllPropertiesForShader(IRayCastingCamera camera)
	{
		var bodyManager = RayCastingBodyManager.Instance;

		// All this data came from body manager.
		shaderForRayCasting.SetBuffer(kernelIndexOfMain, "vertexDataBuffer", vertexDataBuffer);
		shaderForRayCasting.SetBuffer(kernelIndexOfMain, "triangleDataBuffer", triangleDataBuffer);
		shaderForRayCasting.SetBuffer(kernelIndexOfMain, "meshDataBuffer", meshDataBuffer);
		shaderForRayCasting.SetBuffer(kernelIndexOfMain, "bodyDataBuffer", bodyDataBuffer);
		shaderForRayCasting.SetInt("bodyDataCount", bodyManager.BodyCount);
		
		// This data came from camera.
		shaderForRayCasting.SetBuffer(kernelIndexOfMain, "rayDataBuffer", rayDataBuffer);
		shaderForRayCasting.SetInt("width", camera.Width);
		shaderForRayCasting.SetInt("height", camera.Height);
		shaderForRayCasting.SetTexture(kernelIndexOfMain, "renderTarget", camera.RenderTarget);
		shaderForRayCasting.SetVector("backgroundColor", camera.BackgroundColor);
	}

	private void DispatchComputeShader(IRayCastingCamera camera)
	{
		var width = camera.Width;
		var height = camera.Height;
		var threadGroupSizeX = width / (int)threadsPerThreadGroup.x;
		var threadGroupSizeY = height / (int)threadsPerThreadGroup.y;
		// We diving from THREAD_PER_GROUP, because here we specify not the total thread count, but thread group count.
		shaderForRayCasting.Dispatch(kernelIndexOfMain, threadGroupSizeX, threadGroupSizeY, 1);
	}
}
