// Simple deferred rendering shader.
// Deferred rendering in nutshell:
// - For each object:
// -- Draws its color into first color buffer (a.k.a GBuffer)
// -- Draws its specular and smoothness into second buffer
// -- Draws its normal into third color buffer
// -- Draws its emission into fourth color buffer
// (This is done in single pass per object, because specific new gpu feature allows to draw with single
// draw color to multiple color buffers) (Pass "Deferred")
// - For directional light, it renders full screen quad and calculate color using our 4 colors buffers.
// - For other lights:
// -- It creates small mesh (That bounds our light range) and draws on top of it with required color buffers.
//
// So if you remember the forward rendering, calculates light per object. It means if we have 10 objects that are affects by 4 lights.
// We going to have 10*4=40 draw calls.
//
// In deferred rendering context we would only have 10+4=14 drawcalls. Because lights are calculate after all the objects are rendered,
// almost like image effects. So for this very reason, deferred lighting is really fast with light calculations.

Shader "Tests/Basic/DeferredRenderingPath" // Shader name that will be used to find in unity UI
{
	// These where we describe all the shader properties
	// they will be only used in material UI, in other words
	// they are note required for shader to work.
	// You can easily supply them threw the C# code to material
	Properties
	{
		// Syntax of propertie is a bit confusing, so lets take a look at _Color("Main Color", Color) = (1,1,1,1)
		// _Color - veriable name, it must be same in the code
		// unity looks for it in code and tries to connect.
		// "Main Color" - this is the title that will be used in material UI.
		// Color - describes the type.
		//  = (1,1,1,1) - initiale/default value
		_Color("Main Color", Color) = (1,1,1,1)

		_MainTex("Main Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Tags
			{
				// Default deferred rendering pass.
				// https://docs.unity3d.com/Manual/SL-PassTags.html
				"LightMode" = "Deferred"
			}

			CGPROGRAM // Indicates CG start (C for graphics)

			// Including all the functions and structs
			// i know it complicated things a bit, for learning purpose
			// but just imagine that unity copies all the functions and structs from file "AllCodeRequiredForUnlit.cginc"
			// into this file.
			#define UNITY_PASS_DEFERRED
			#include "AllCodeRequiredForDeferredRenderingPath.cginc"

			// Here we specify the shader function names
			#pragma vertex vertDeferred // Specifies the vertex function name
			#pragma fragment fragDeferred // Specifies the fragment function name

			#pragma multi_compile_prepassfinal

			ENDCG // Ends the CG code
		}

		// Our shader caster that we created.
		Pass
		{
			Tags
			{
				// Describes that this should be used to calculate shadow casting.
				// https://docs.unity3d.com/Manual/SL-PassTags.html
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM // Indicates CG start (C for graphics)

			// Including all the functions and structs
			// i know it complicated things a bit, for learning purpose
			// but just imagine that unity copies all the functions and structs from file "AllCodeRequiredForShadowCaster.cginc"
			// into this file.
			#include "../ShadowCaster/AllCodeRequiredForShadowCaster.cginc"

			// Here we specify the shader function names
			#pragma vertex vertShadowCaster // Specifies the vertex function name
			#pragma fragment fragShadowCaster // Specifies the fragment function name

			#pragma multi_compile_shadowcaster // Compiles multiple versions for directional/point/spot light shadows.

			ENDCG // Ends the CG code
		}
	}
}
