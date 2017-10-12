using UnityEngine;
using System.Collections;

public struct RayCastingRayData
{
	public Vector3 position; // Yea I know it usually called origin.
    public Vector3 direction;
}

// Instance of ray casting camera, used for rendering.
public interface IRayCastingCamera
{
	RayCastingRayData[] RayDatas { get; }
	RenderTexture RenderTarget { get; }
	int Width { get; }
	int Height { get; }
	Color BackgroundColor { get; }

	void CalculateRays();
}