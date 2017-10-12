// Simple unlit shader

Shader "Tests/Basic/ShadowCaster" // Shader name that will be used to find in unity UI
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
				// Describes when the rendering should happen
				// https://docs.unity3d.com/Manual/SL-PassTags.html
				"LightMode" = "Always" 
			}

			CGPROGRAM // Indicates CG start (C for graphics)

			// Including all the functions and structs
			// i know it complicated things a bit, for learning purpose
			// but just imagine that unity copies all the functions and structs from file "AllCodeRequiredForUnlit.cginc"
			// into this file.
			#include "../Unlit/AllCodeRequiredForUnlit.cginc"

			// Here we specify the shader function names
			#pragma vertex vertBasic // Specifies the vertex function name
			#pragma fragment fragBasic // Specifies the fragment function name

			ENDCG // Ends the CG code
		}

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
			#include "AllCodeRequiredForShadowCaster.cginc"

			// Here we specify the shader function names
			#pragma vertex vertShadowCaster // Specifies the vertex function name
			#pragma fragment fragShadowCaster // Specifies the fragment function name

			#pragma multi_compile_shadowcaster

			ENDCG // Ends the CG code
		}
	}
}
