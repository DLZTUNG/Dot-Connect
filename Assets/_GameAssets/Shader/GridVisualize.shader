Shader "Custom/GridCellBorder"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _BorderWidth ("Border Width", Range(0,0.5)) = 0.05
        _Alpha ("Alpha", Range(0,1)) = 1    // ⭐ Thêm alpha
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        // Cho phép trong suốt
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _MainColor;
            fixed4 _BorderColor;
            float _BorderWidth;
            float _Alpha;   // ⭐ Alpha

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;

                // Kiểm tra viền
                bool isBorder =
                    (i.uv.x < _BorderWidth || i.uv.x > 1 - _BorderWidth ||
                     i.uv.y < _BorderWidth || i.uv.y > 1 - _BorderWidth);

                col = isBorder ? _BorderColor : _MainColor;

                // ⭐ Nhân alpha
                col.a *= _Alpha;

                return col;
            }
            ENDCG
        }
    }

    FallBack "Transparent"
}
