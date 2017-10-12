// Simple forward rendering shader.
// Forward rendering in nutshell:
// - For each object:
// -- Draws object with directional light, even if directional light is not present int scene (Pass "ForwardBase")
// - For each object:
// -- For each other light (not directional):
// --- Draws object with light on top (Pass "ForwardAdd")
//
// So forward lighting is really expensive with additional lights because it re-renders object for each light.

Shader "Tests/Basic/ForwardRenderingPath" // Shader name that will be used to find in unity UI
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
				// This pass describes that it must be used when we want to render directional light.
				// Even if the directional light is not present in the scene unity will use this pass with dummy directional.
				// https://docs.unity3d.com/Manual/SL-PassTags.html
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM // Indicates CG start (C for graphics)

			// Including all the functions and structs
			// i know it complicated things a bit, for learning purpose
			// but just imagine that unity copies all the functions and structs from file "AllCodeRequiredForUnlit.cginc"
			// into this file.
			#define UNITY_PASS_FORWARDBASE
			#include "AllCodeRequiredForForwardRenderingPath.cginc"

			// Here we specify the shader function names
			#pragma vertex vertForward // Specifies the vertex function name
			#pragma fragment fragForwardBase // Specifies the fragment function name

			#pragma multi_compile_fwdbase // Adds varation with shadows.

			ENDCG // Ends the CG code
		}

		Pass
		{
			Tags
			{
				// This pass is ued for point/spot light, as it describes it just adds color on top of the directional pass rendered.
				// For this very reason, forward rendering is extremly expensive if you have lots of lights. For every light on object
				// it re-renders the object.
				// https://docs.unity3d.com/Manual/SL-PassTags.html
				"LightMode" = "ForwardAdd"
			}
			
			// Every time we render object we will write its Z value of pixel into depth buffer.
			// In this way if another object is in the same place, we can compare its Z value wit hcurrent depth buffer value
			// to know which object is closer near the camera.
			// So for this example ZWrite Off indicates that we are not going to write object Z value int depth buffer.
			ZWrite Off 
			
			// Once the fragment function calculate final color, the "last" step is to combine the result color with current render target.
			// https://docs.unity3d.com/Manual/SL-Blend.html
			Blend One One
			
			CGPROGRAM // Indicates CG start (C for graphics)

			// Including all the functions and structs
			// i know it complicated things a bit, for learning purpose
			// but just imagine that unity copies all the functions and structs from file "AllCodeRequiredForUnlit.cginc"
			// into this file.
			#define UNITY_PASS_FORWARDADD
			#include "AllCodeRequiredForForwardRenderingPath.cginc"

			// Here we specify the shader function names
			#pragma vertex vertForward // Specifies the vertex function name
			#pragma fragment fragForwardAdd // Specifies the fragment function name

			#pragma multi_compile_fwdadd_fullshadows // Adds varations with shadows.

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
