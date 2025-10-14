Shader "Unlit/DepthShader"
{
    Properties
    {
        _RelativeViewDepthStart ("Relative View Depth Start", Float) = -1.0
        _RelativeViewDepthEnd ("Relative View Depth End", Float) = 1.0 
        _NearColor ("Near Color", Color) = (1,1,1,1) // Start color (Gradient)
        _FarColor ("Far Color", Color) = (0.5,0.5,0.5,0.5) // End color (Gradient)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float relativeViewDepth : TEXCOORD0;
            };

            float _RelativeViewDepthStart;
            float _RelativeViewDepthEnd;
            float4 _NearColor;
            float4 _FarColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                float3 objectWorldPos = TransformObjectToWorld(float3(0,0,0));

                float3 vertexViewPos = TransformWorldToView(worldPos);

                float3 objectViewPos = TransformWorldToView(objectWorldPos);

                OUT.relativeViewDepth = vertexViewPos.z - objectViewPos.z;

                OUT.positionCS = TransformWorldToHClip(worldPos);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float t = saturate((IN.relativeViewDepth - _RelativeViewDepthStart) / (_RelativeViewDepthEnd - _RelativeViewDepthStart));

                half4 finalColor = lerp(_NearColor, _FarColor, t);

                return finalColor;
            }
            ENDHLSL
        }
    }
}