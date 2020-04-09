Shader "Custom/Radial Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount("Amount of radial blur applied", Float) = 0.0
    }

    SubShader
    {
        // Single shader pass, can have multiple, can name them as well
        Pass
        {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
            #pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Data passed through structs in shaders
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

            // Vertex shader that returns o for use in fragment shader
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float _BlurAmount;

            static int SampleCount = 50;
            static float Falloff = 3.0; // lower the number the less the falloff - starts the effect closer to center of screen

            fixed4 frag (v2f i) : SV_Target
            {                
                float2 destCoord = i.uv; // get uv coordinates of screen
                float2 direction = normalize(destCoord - 0.5); // make effect come from center of screen outwards

                float2 velocity = direction * _BlurAmount * pow(length(destCoord - 0.5), Falloff); // simulate the velocity of the blur
                float inverseSampleCount = 1.0 / float(SampleCount); // inverse for sample count, starts smaller closer to screen

                // Increment blur amount further from screen
                float3x2 increments = float3x2
                (
                    velocity * 1.0 * inverseSampleCount,
                    velocity * 2.0 * inverseSampleCount,
                    velocity * 4.0 * inverseSampleCount
                );

                float3 FinalColor = 0;
                float3x2 offsets = 0;

                // Chromatic aberration
                for (int j = 0; j < SampleCount; j++)
                {
                    FinalColor.r += tex2D(_MainTex, destCoord + offsets[0]).r;
                    FinalColor.g += tex2D(_MainTex, destCoord + offsets[1]).g;
                    FinalColor.b += tex2D(_MainTex, destCoord + offsets[2]).b;

                    offsets -= increments;
                }

                return float4(FinalColor / float(SampleCount), 1.0);
            }
            ENDCG
        }
    }
}
