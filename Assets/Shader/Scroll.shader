Shader "Scroll" 
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader 
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
        }

        Pass 
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert(appdata_full v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.viewDir = normalize(_WorldSpaceCameraPos - mul((float3x3)unity_ObjectToWorld, v.vertex));
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv += float2(_Time.y * 0.1, 0);
                return o;
            }

            fixed4 frag(v2f i) : COLOR 
            {
                float val = 0.95 - abs(dot(i.viewDir, i.normal.yz));
                float val2 = 1.5 - abs(dot(i.viewDir, i.normal.zx));

                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 finalColor = _Color * val * val2 * texColor * 1.7;
                finalColor.a *= _Color.a;

                return finalColor;
            }
            ENDCG
        }
    }
}
