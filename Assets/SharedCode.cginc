// This is like a header file in c, called cginc (C for graphics include)
// And its going to contain all required functions and structs
// For our basic shaders
// It is really good practice to keep all the shaders stuff in cginc file

// These pre-processor macros required to avoid duplicated including
// https://gcc.gnu.org/onlinedocs/gcc-3.0.2/cpp_2.html (Check 2.4 Once-Only Headers)
#ifndef SHARED_CODE
#define SHARED_CODE

// You might already know that shader don't have include files
// This is unity created thing
// So what it does copies the source code to our shader
#include "UnityCG.cginc" // Include basic unity functions and built in values veriables
#include "Lighting.cginc" // Includes some lighting veriables
#include "AutoLight.cginc"

// One of the most popular lighting models.
// Idea is that if surface is facing the light it means the more rays/photons will hit the surface.
inline fixed3 CalculateLambert(float3 normal, float3 lightDirection, float3 lightColor, float lightIntensity)
{
	fixed3 directionalLightColor = lightColor * lightIntensity;
	fixed attenuation = max(0.0, dot(-lightDirection, normal)); // Naive lambert function.
	return directionalLightColor * attenuation;
}

inline fixed3 CalculatePointOrSpotLight(float3 worldNormal, float3 worldSpacePosition, float3 lightPosition,
	float3 lightColor, float lightAttenuation)
{
	float3 vertexToLightSource = worldSpacePosition - lightPosition;
	float3 lightDirection = normalize(vertexToLightSource);
	float squaredDistance = dot(vertexToLightSource, vertexToLightSource);

	// In point light/spot on top of the lambert we reduce intensity by the distance.
	// We do this because we assume that point lights usualy very close to the surface.
	float attenuation = 1.0 / (1.0 +
		lightAttenuation * squaredDistance);

	float3 diffuseReflection = CalculateLambert(worldNormal, lightDirection, lightColor, attenuation);
	return diffuseReflection;
}

// This is per vertex lighting color.
inline fixed3 CalculatePointLights(float3 normal, float3 worldSpacePosition)
{
	fixed3 pointLightColor = fixed3(0, 0, 0);
	for (int index = 0; index < 4; index++)
	{
		// Geting the light position in world space
		float3 lightPosition = float3(
			unity_4LightPosX0[index],
			unity_4LightPosY0[index],
			unity_4LightPosZ0[index]);

		pointLightColor += CalculatePointOrSpotLight(normal, worldSpacePosition, lightPosition, unity_LightColor[index].rgb, unity_4LightAtten0[index]);
	}

	return pointLightColor;
}

inline fixed3 CalculateDirectionalLight(float3 normal)
{
	// In directional we use plain lambert function, because we assume that directional light source
	// is usually extremly far away and its world position can't contribute to the light.

	// i.e. Idea is very similar, when you walk at night and see the moon, no matter how much you move on earth surface
	// moon will still keep visibly same relative position from you.
	return CalculateLambert(normal, -_WorldSpaceLightPos0.xyz, _LightColor0.rgb, _LightColor0.a);
}

inline fixed3 CalculateAmbientColorFromSolidColor()
{
	fixed3 ambientLight = UNITY_LIGHTMODEL_AMBIENT.rgb;
	return ambientLight;
}

// Unity have many ways to calculate it
// 1. Using solid color UNITY_LIGHTMODEL_AMBIENT.
// 2. Using gradient and pixel normal for gradient position.
// 3. Using spherical harmonics and then sampling it with normal.
// Spherical Harmonics - extremly strong technique, where light can be coded into light weight function.
// In nutshell to calculate it - takes cube map, blurs it and converts to SH.
// https://en.wikipedia.org/wiki/Spherical_harmonics
inline fixed3 CalculateAmbientColor()
{
	// Idea of ambient light, to simulate light bouncing/reflection.
	// In ray tracing we simple use more bounces.
	fixed3 ambientLight = CalculateAmbientColorFromSolidColor();
	return ambientLight;
}

#endif // SHARED_CODE