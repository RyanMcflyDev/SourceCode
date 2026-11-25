// outline drawn round whatever byte has selected, used by SelectionOutline on copies of the target's meshes
// pushes the mesh out from the middle of the object rather than along the normals,
// so hard edged low poly meshes (crates) don't split open at the corners
Shader "Source Code/Selection Outline"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.4, 2, 1, 1)
        _Width ("Width", Float) = 0.06
        _PulseSpeed ("Pulse Speed", Float) = 4
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
        // world space middle of the object, set per copy from SelectionOutline
        [HideInInspector] _OutlineCentre ("Outline Centre", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            // only the back of the pushed out shell shows, so it peeks out round the edges of the real object
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Width;
                float _PulseSpeed;
                float _PulseAmount;
                float4 _OutlineCentre;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 outward = positionWS - _OutlineCentre.xyz;
                // a vertex right at the middle has no direction, fall back to its normal
                outward = dot(outward, outward) > 1e-6 ? normalize(outward) : TransformObjectToWorldNormal(input.normalOS);
                positionWS += outward * _Width;
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half pulse = 1.0 - _PulseAmount * (0.5 + 0.5 * sin(_Time.y * _PulseSpeed));
                return half4(_Color.rgb * pulse, 1.0);
            }
            ENDHLSL
        }
    }
}
