Shader "Hidden/HighContrastGrayscale"
{
    Properties
    {
        _MainTex ("Screen", 2D) = "white" {}
        _Contrast ("Contrast", Range(1, 4)) = 2
        _Brightness ("Brightness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Contrast;
            float _Brightness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Standard luminance weights (perceived brightness).
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                // High contrast: remap around midpoint so darks and lights separate more.
                float mid = _Brightness;
                luminance = (luminance - mid) * _Contrast + mid;
                luminance = saturate(luminance);
                return fixed4(luminance, luminance, luminance, col.a);
            }
            ENDCG
        }
    }
    Fallback Off
}
