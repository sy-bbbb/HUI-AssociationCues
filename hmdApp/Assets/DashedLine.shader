Shader "Unlit/DashedLine"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _DashSize("Dash Size", Float) = 5
        _GapSize("Gap Size", Float) = 5
        _Thickness("Thickness", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _DashSize;
            float _GapSize;
            float _Thickness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float len = _DashSize + _GapSize;
                float pattern = fmod(i.uv.x * 1000, len);
                if (pattern > _DashSize)
                    discard;

                return _Color;
            }
            ENDCG
        }
    }
}
