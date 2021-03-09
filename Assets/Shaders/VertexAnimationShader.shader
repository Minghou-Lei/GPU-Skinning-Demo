Shader "Custom/VertexAnimationShader"
{
    Properties
    {
        _MainTex("Texture",2D)="white"{}
        _PosTex("Position Texture",2D)="black"{}
        _NmlTex("Normal Texture",2D)="white"{}
        _Length("Animator lengh",float)=1
        _Offset("Offset time",float)=0
    }
    SubShader
    {
        Tags{"RenderType"="Opaque"}
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define TS _PosTex_TexelSize

            //输入网格数据
            struct appdata
            {
                float2 uv : TEXCOORD0;
            };

            //顶点到片元结构体
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex, _PosTex, _NmlTex;
            float4 _PosTex_TexelSize;
            float _Length, _Offset;

            //几何阶段（构建顶点到片元结构体）
            v2f vert(appdata v, uint vid : SV_VertexID)
            {
                float t = (_Time.y - _Offset) / _Length;
                float x = (vid + 0.5) / TS.z;
                

                float3 pos = tex2Dlod(_PosTex, float4(x, t, 0, 0));
                float3 normal = tex2Dlod(_NmlTex, float4(x, t, 0, 0));
                v2f o;
                //MVP
                o.vertex = UnityObjectToClipPos(pos);
                o.normal = UnityObjectToWorldNormal(normal);
                o.uv = v.uv;
                return o;
            }

            //光栅化阶段
            half4 frag(v2f i):SV_Target
            {
                half diff = dot(i.normal, float3(0, 1, 0))*0.5+0.5;
                half4 col = tex2D(_MainTex, i.uv);
                return col * diff;
            }
            ENDCG
        }
        
    }
    FallBack "Diffuse"
}