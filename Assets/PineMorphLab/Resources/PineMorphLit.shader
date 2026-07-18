Shader "PineMorph/Lit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _FiberAngle ("Fiber Angle", Range(0,90)) = 0
        _FiberStrength ("Fiber Strength", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 180

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                half3 worldNormal : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
                half2 uv : TEXCOORD2;
            };

            fixed4 _Color;
            half _Metallic;
            half _Glossiness;
            half _FiberAngle;
            half _FiberStrength;

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                half3 normal = normalize(input.worldNormal);
                half3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
                half3 halfDirection = normalize(lightDirection + viewDirection);
                half diffuse = saturate(dot(normal, lightDirection));
                half specularPower = lerp(12.0h, 96.0h, _Glossiness);
                half specular = pow(saturate(dot(normal, halfDirection)), specularPower);
                half3 ambient = ShadeSH9(half4(normal, 1.0h));
                half3 baseLighting = ambient + _LightColor0.rgb * (0.18h + diffuse * 0.82h);
                half3 specularColor = lerp(0.04h.xxx, _Color.rgb, _Metallic);
                half angle = radians(_FiberAngle);
                half2 materialPosition = input.uv * half2(1.55h, 4.6h);
                half fiberCoordinate = materialPosition.x * cos(angle)
                    + materialPosition.y * sin(angle);
                half fiberBand = smoothstep(0.82h, 0.98h, abs(sin(fiberCoordinate * 48.0h)));
                half3 surfaceColor = _Color.rgb * lerp(1.0h, 1.22h, fiberBand * _FiberStrength);
                half3 color = surfaceColor * baseLighting + specularColor * specular * _Glossiness;
                return fixed4(color, _Color.a);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
