using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Camera))]
public class RayCastingCamera : MonoBehaviour, IRayCastingCamera
{
#if UNITY_EDITOR
	private const int SHOW_RAYS_IN_SCREEN_VIEW_MAXIMUM_COUNT = 100;
	public bool showRaysInSceneView = false;
#endif

	// Screen options.
	public int width = 256;
	public int height = 256;
	public Color backgroundColor = new Color32(49, 77, 121, 0);

	// Projection options.
	public float fieldOfView = 60;

	// Orthographic options.
	public bool orthographic = false;
	public float orthographicSize = 5;

	private RayCastingRayData[] rayDatas;
	private RenderTexture renderTarget;

	public RayCastingRayData[] RayDatas { get { return rayDatas; } }
	public RenderTexture RenderTarget { get { return renderTarget; } }
	public int Width { get { return width; } }
	public int Height { get { return height; } }
	public Color BackgroundColor { get { return backgroundColor; } }

	private void Start()
	{
		SetResolution(width, height);
	}

	private void SetResolution(int width, int height)
	{
		Debug.Assert(width > 0 && height > 0, "Dimensions must be higher than zero");

		this.width = width;
		this.height = height;

		rayDatas = new RayCastingRayData[width * height];

		// Creating our render texture that will be used for ray casting.
		renderTarget = new RenderTexture(width, height, 0);
		renderTarget.enableRandomWrite = true;
		renderTarget.Create();
	}

	// Camera calls this function, so we have to be sure that this game object contains camera.
	private void OnRenderImage
		(
		RenderTexture source, /* Contains all the things we rendered so far */
		RenderTexture destination /* Target render target, that is the screen in forward rendering */
		)
	{
		// We render all our stuff into "renderTarget".
		Render();

		// This call is really simple, this what it does:
		// - Takes rectangle mesh.
		// - Sets orthgraphic view, that maps rectangle perfectly on all screen.
		// - Sets Blit material for rectangle, that acts as Unlit shader, only samples _MainTex colors.
		// (Basicaly copies _MainTex colors on itself)
		// - Renders rectangle into render target we specify (In this case "destination").
		// In this example render target "destination" is our screen, so overall it takes "renderTarget" and puts on the screen.
		// If you familiar with graphics API, idea is very similar to Present function call.
		Graphics.Blit(renderTarget, destination);
	}

	public void Render()
	{
		// It might be a bit confusing that we need executer for rendering.
		// However with this our code is more clean and split, not we can have different varations of executers.
		var executer = RayCastingExecuterGPU.Instance;
		if (executer != null)
			executer.Render(this);
	}

	public void CalculateRays()
	{
		// So at some point you can think orthograpic rendering as opposite to projection rendering.
		//
		// In perspective rendering:
		// - All rays origin is same.
		// - All rays direction is different.
		//
		// In orhtographic rendering:
		// - All rays origin is different.
		// - All rays direction is same.

		// Here we calculate rays on cpu.
		if (orthographic)
		{
			CalculateRaysForOrthographic();
		}
		else
		{
			CalculateRaysForPerspective();
		}
	}

	private void CalculateRaysForPerspective()
	{
		// Calculate the field of view transformation.
		// TODO: Cache.
		var degress = fieldOfView * Mathf.Deg2Rad / 2;
		var sizeOfOffset = Mathf.Tan(degress);
		var aspectRatio = (float)Screen.width / Screen.height;

		// In projection rendering position is same for all rays.
		var position = transform.position; 

		for (int j = 0; j < height; j++)
		{
			for (int i = 0; i < width; i++)
			{
				// Firstly we transform from texture space into normalized texture space.
				var pixelNormalizedX = ((i + 0.5f)) / width;
				var pixelNormalizedY = ((j + 0.5f)) / height;

				// Then we offset the space from [0:1] to [-1:1]
				var pixelScreenX = pixelNormalizedX * 2 - 1;
				var pixelScreenY = pixelNormalizedY * 2 - 1;

				// Taking field of view into calculation and aspect ration in case screen is not uniform.
				var pixelCameraX = pixelScreenX * sizeOfOffset * aspectRatio;
				var pixelCameraY = pixelScreenY * sizeOfOffset;

				var direction = new Vector3(pixelCameraX, pixelCameraY, 1);
				direction = transform.rotation * direction.normalized; // To world space.

				var rayData = new RayCastingRayData();
				rayData.position = position;
				rayData.direction = direction;
				rayDatas[j * width + i] = rayData;
			}
		}
	}

	private void CalculateRaysForOrthographic()
	{
		var aspectRatio = (float)width / height;

		// In orthographic rendering direction is same for all rays.
		var direction = transform.rotation * Vector3.forward; 

		for (int j = 0; j < height; j++)
		{
			for (int i = 0; i < width; i++)
			{
				// Firstly we transform from texture space into normalized texture space.
				var pixelNormalizedX = ((i + 0.5f)) / width;
				var pixelNormalizedY = ((j + 0.5f)) / height;

				// Then we offset the space from [0:1] to [-1:1]
				var pixelScreenX = pixelNormalizedX * 2 - 1;
				var pixelScreenY = pixelNormalizedY * 2 - 1;

				// Taking field of view into calculation and aspect ration in case screen is not uniform.
				var pixelCameraX = pixelScreenX * aspectRatio * orthographicSize;
				var pixelCameraY = pixelScreenY * orthographicSize;

				var position = new Vector3(pixelCameraX, pixelCameraY, 0);
				position += transform.position; // To world space.

				var rayData = new RayCastingRayData();
				rayData.position = position;
				rayData.direction = direction;
				rayDatas[j * width + i] = rayData;
			}
		}
	}

	private void Update()
	{
		// Only for debug purpose, should be excluded in release build.
		#if UNITY_EDITOR

		if (showRaysInSceneView)
		{
			// This is a bit hard coded, but what it does.
			// Controls the ray count, so we maximum only see SHOW_RAYS_IN_SCREEN_VIEW_MAXIMUM_COUNT.
			var rayCount = (float)rayDatas.Length;
			var rayStride = Mathf.Min(1, SHOW_RAYS_IN_SCREEN_VIEW_MAXIMUM_COUNT / rayCount);
			float rayCounter = 0;

			for (int i = 0; i < rayCount; i++)
			{
				rayCounter += rayStride;

				// Checks if rayCounter have enough value to draw one ray.
				if (rayCounter > 1f)
				{
					Debug.DrawRay(rayDatas[i].position, rayDatas[i].direction * 10);
					rayCounter -= 1;
				}
			}
		}

		#endif
	}
}
