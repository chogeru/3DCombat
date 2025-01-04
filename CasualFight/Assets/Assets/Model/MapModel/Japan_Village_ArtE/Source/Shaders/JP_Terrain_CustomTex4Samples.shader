// Made with Amplify Shader Editor v1.9.0.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "JP_Terrain_CustomTex4Samples"
{
	Properties
	{
		_SplatMap("SplatMap", 2D) = "white" {}
		_MainTex_1("MainTex_1", 2D) = "white" {}
		_MainTex_2("MainTex_2", 2D) = "white" {}
		_MainTex_3("MainTex_3", 2D) = "white" {}
		_MainTex_4("MainTex_4", 2D) = "white" {}
		_BackgroundForest("BackgroundForest", 2D) = "white" {}
		[HDR]_TerrainTint("TerrainTint", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry-100" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform sampler2D _SplatMap;
		uniform float4 _SplatMap_ST;
		uniform sampler2D _MainTex_1;
		uniform sampler2D _MainTex_2;
		uniform sampler2D _MainTex_3;
		uniform sampler2D _MainTex_4;
		uniform sampler2D _BackgroundForest;
		uniform float4 _TerrainTint;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_SplatMap = i.uv_texcoord * _SplatMap_ST.xy + _SplatMap_ST.zw;
			float4 tex2DNode1 = tex2D( _SplatMap, uv_SplatMap );
			float3 ase_worldPos = i.worldPos;
			float2 appendResult6 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 temp_output_5_0 = ( appendResult6 * 0.02 );
			float4 layeredBlendVar16 = tex2DNode1;
			float4 layeredBlend16 = ( lerp( lerp( lerp( lerp( tex2D( _MainTex_1, temp_output_5_0 ) , tex2D( _MainTex_2, temp_output_5_0 ) , layeredBlendVar16.x ) , tex2D( _MainTex_3, temp_output_5_0 ) , layeredBlendVar16.y ) , tex2D( _MainTex_4, temp_output_5_0 ) , layeredBlendVar16.z ) , tex2D( _BackgroundForest, ( appendResult6 * 0.005 ) ) , layeredBlendVar16.w ) );
			o.Albedo = layeredBlend16.rgb;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV19 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode19 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV19, 16.0 ) );
			float clampResult21 = clamp( fresnelNode19 , 0.0 , 1.0 );
			float4 layeredBlendVar36 = tex2DNode1;
			float layeredBlend36 = ( lerp( lerp( lerp( lerp( 1.0 , 1.0 , layeredBlendVar36.x ) , 1.0 , layeredBlendVar36.y ) , 1.0 , layeredBlendVar36.z ) , 3.0 , layeredBlendVar36.w ) );
			o.Emission = ( sqrt( clampResult21 ) * layeredBlend16 * _TerrainTint * layeredBlend36 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19002
-1913;-31;1920;951;414.8675;310.2936;1;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;4;-1314.957,450.9571;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;6;-1055.709,438.6695;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1219.92,869.827;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;0.02;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1032.388,1152.554;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;0;False;0;False;0.005;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-817.4926,874.6721;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FresnelNode;19;-12.74242,-285.6635;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;16;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-834.5682,648.933;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-103.8675,160.7064;Inherit;False;Constant;_Float2;Float 2;7;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1234,22.5;Inherit;True;Property;_SplatMap;SplatMap;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;18;-434.8033,788.3351;Inherit;True;Property;_BackgroundForest;BackgroundForest;5;0;Create;True;0;0;0;False;0;False;-1;89408355a02084c45ac87d73e8c4e409;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;17;-464.3279,585.6892;Inherit;True;Property;_MainTex_4;MainTex_4;4;0;Create;True;0;0;0;False;0;False;-1;89408355a02084c45ac87d73e8c4e409;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-472.7515,-26.12096;Inherit;True;Property;_MainTex_1;MainTex_1;1;0;Create;True;0;0;0;False;0;False;-1;89408355a02084c45ac87d73e8c4e409;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;15;-461.3152,378.7817;Inherit;True;Property;_MainTex_3;MainTex_3;3;0;Create;True;0;0;0;False;0;False;-1;89408355a02084c45ac87d73e8c4e409;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;-468,175.5;Inherit;True;Property;_MainTex_2;MainTex_2;2;0;Create;True;0;0;0;False;0;False;-1;89408355a02084c45ac87d73e8c4e409;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;21;281.8396,-253.0767;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;20;418.726,-118.8741;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;23;589.171,-328.8115;Inherit;False;Property;_TerrainTint;TerrainTint;6;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LayeredBlendNode;16;73.83052,419.4659;Inherit;False;6;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LayeredBlendNode;36;129.1734,50.72158;Inherit;False;6;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;1;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;665.6589,-70.56152;Inherit;False;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;3;-830,88.5;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1007.415,87.35774;Float;False;True;-1;7;ASEMaterialInspector;0;0;Standard;JP_Terrain_CustomTex4Samples;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;-100;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;4;1
WireConnection;6;1;4;3
WireConnection;34;0;6;0
WireConnection;34;1;35;0
WireConnection;5;0;6;0
WireConnection;5;1;7;0
WireConnection;18;1;34;0
WireConnection;17;1;5;0
WireConnection;2;1;5;0
WireConnection;15;1;5;0
WireConnection;13;1;5;0
WireConnection;21;0;19;0
WireConnection;20;0;21;0
WireConnection;16;0;1;0
WireConnection;16;1;2;0
WireConnection;16;2;13;0
WireConnection;16;3;15;0
WireConnection;16;4;17;0
WireConnection;16;5;18;0
WireConnection;36;0;1;0
WireConnection;36;5;37;0
WireConnection;22;0;20;0
WireConnection;22;1;16;0
WireConnection;22;2;23;0
WireConnection;22;3;36;0
WireConnection;3;0;1;1
WireConnection;3;1;1;2
WireConnection;3;2;1;3
WireConnection;3;3;1;4
WireConnection;0;0;16;0
WireConnection;0;2;22;0
ASEEND*/
//CHKSM=091EADEE61093848C5ADE9CE55A7DC57C9C087FE