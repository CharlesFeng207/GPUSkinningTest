Shader "Unlit/MyUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"            

    sampler2D _MainTex;
    float4 _MainTex_ST;

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        // sample the texture
        fixed4 col = tex2D(_MainTex, i.uv);
        return col;
    }

    fixed4 frag_xray(v2f i) : SV_Target
	{
		return fixed4(0, 0, 1, 0.5);
	}
    
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        // Pass
		// {
		// 	Blend SrcAlpha One
		// 	ZWrite Off
		// 	ZTest Greater
		// 	CGPROGRAM
		// 	#pragma vertex vert
		// 	#pragma fragment frag_xray
		// 	#pragma multi_compile_instancing
		// 	#pragma multi_compile ROOTON_BLENDOFF ROOTON_BLENDON_CROSSFADEROOTON ROOTON_BLENDON_CROSSFADEROOTOFF ROOTOFF_BLENDOFF ROOTOFF_BLENDON_CROSSFADEROOTON ROOTOFF_BLENDON_CROSSFADEROOTOFF
		// 	ENDCG
		// }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            ENDCG
        }
    }
}
