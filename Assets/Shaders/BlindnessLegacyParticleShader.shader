Shader "BlindnessParticleShader (Legacy)" 
{
	Properties 
	{
		_MainTex ("Particle Texture", 2D) = "white" {}
		_WaveColor ("Wave Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_WaveDistance ("Wave Distance", Range(0.0,1)) = .2
		_WaveLength ("Wave Length", Range(0.0, 10.0)) = 1.0
	}

	SubShader 
	{

		Blend One One
		ZWrite Off

		Pass
		{
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members uv)
#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float depth : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float4 color : COLOR0;
			}; 

			float4 _NoiseTex_ST;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);			
				o.depth = lerp(o.pos.z, length(mul(UNITY_MATRIX_MV, v.vertex).xyz), .85);
				o.uv = v.texcoord.xy;
				o.color = v.color;
				return o;
			}

			float _WaveDistance;
			float _WaveLength;

			float4 _WaveColor;

			sampler2D _MainTex;

			float4 frag (v2f i) : COLOR
			{
				float time = pow(fmod(_Time.y * .75, 1.0), .75);	// doable in script
				float dispersion = _WaveLength * (1.15 - time);		// doable in script

				float depth = i.depth * _ProjectionParams.w;
				float dist = 1.0 - abs(depth - time) * dispersion;
				float4 color = tex2D(_MainTex, i.uv) * i.color;

				float detail = smoothstep(0.0, 1.0, dist) * dist * (1.0 - time) * 4.0 + .15;

				return _WaveColor * detail * color;
			}
			ENDCG

		}
		 
	} 

	FallBack "Mobile/Diffuse"
}