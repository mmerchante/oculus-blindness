Shader "BlindnessShader (Legacy)" 
{
	Properties 
	{
		_NoiseTex ("Noise", 2D) = "white" {}
		_WaveColor ("Wave Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_WaveDistance ("Wave Distance", Range(0.0,1)) = .2
		_WaveLength ("Wave Length", Range(0.0, 10.0)) = 1.0
	}

	SubShader 
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float depth : TEXCOORD0;
				float2 noiseUV : TEXCOORD1;
			}; 

			float4 _NoiseTex_ST;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);			
				o.depth = lerp(o.pos.z, length(mul(UNITY_MATRIX_MV, v.vertex).xyz), .85);
				o.noiseUV = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				return o;
			}

			float _WaveDistance;
			float _WaveLength;
			float _WaveTime;
			float _WaveDispersion;

			float4 _WaveColor;

			sampler2D _NoiseTex;

			float4 frag (v2f i) : COLOR
			{
				float time = pow(fmod(_Time.y * .75, 1.0), .75);	// doable in script
				float dispersion = _WaveLength * (1.15 - time);		// doable in script

				float4 noise = tex2D(_NoiseTex, tex2D(_NoiseTex, i.noiseUV + _Time.x).xy + _Time.x);
				float depth = i.depth * _ProjectionParams.w;
				float dist = 1.0 - abs(depth - time) * dispersion;

				float wave = (noise.x + noise.y) * .5;

				float detail = smoothstep(wave, wave + .5, dist) * dist * noise.x * (1.0 - time) * 3.0 + .1;

				return _WaveColor * detail;
			}
			ENDCG

		}
		 
	} 

	FallBack "Mobile/Diffuse"
}