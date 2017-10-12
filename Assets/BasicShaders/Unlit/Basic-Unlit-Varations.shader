// Simple unlit shader with some varations

Shader "Tests/Basic/UnlitVarations" // Shader name that will be used to find in unity UI
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

		// https://docs.unity3d.com/ScriptReference/MaterialPropertyDrawer.html
		[KeywordEnum(NormalColor, InvertedColor)] _ColorType("Color type", float) = 0
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
			// I know it complicates things a bit for learning purpose
			// but just imagine that unity copies all the functions and structs from file "AllCodeRequiredForUnlit.cginc"
			// into this file.
			#include "AllCodeRequiredForUnlit.cginc"

			// Here we specify the shader function names
			#pragma vertex vertBasic // Specifies the vertex function name
			#pragma fragment fragBasicVarations // Specifies the fragment function name

			// This line produces multiple shader varations
			#pragma multi_compile _COLORTYPE_NORMALCOLOR _COLORTYPE_INVERTEDCOLOR

			ENDCG // Ends the CG code
		}
	}
}
