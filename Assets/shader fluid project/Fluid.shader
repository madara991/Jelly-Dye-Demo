// StableFluids - A GPU implementation of Jos Stam's Stable Fluids on Unity
// https://github.com/keijiro/StableFluids

Shader "Hidden/StableFluids"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _VelocityField("Velocity Field", 2D) = "white" {}
        _DyeColor("Dye Color", Color) = (1,1,1,1) // Add a new property for the dye color
    }

        CGINCLUDE

#include "UnityCG.cginc"

            sampler2D _MainTex;
        float4 _MainTex_TexelSize;

        sampler2D _VelocityField;

        float2 _ForceOrigin;
        float _ForceExponent;

        float4 _DyeColor; // Declare the dye color

        half4 frag_advect(v2f_img i) : SV_Target
        {
            float time = _Time.y;
            float deltaTime = unity_DeltaTime.x;

            float2 aspect = float2(_MainTex_TexelSize.y * _MainTex_TexelSize.z, 1);
            float2 aspect_inv = float2(_MainTex_TexelSize.x * _MainTex_TexelSize.w, 1);

            float2 delta = tex2D(_VelocityField, i.uv).xy * aspect_inv * deltaTime;
            float3 color = tex2D(_MainTex, i.uv - delta).xyz;

            // Use the color parameter
            float3 dye = _DyeColor.rgb;

            float2 pos = (i.uv - 0.5) * aspect;
            float amp = exp(-_ForceExponent * distance(_ForceOrigin, pos));
            color = lerp(color, dye, saturate(amp * 100));

            return half4(color, 1);
        }

            half4 frag_render(v2f_img i) : SV_Target
        {
            half3 rgb = tex2D(_MainTex, i.uv).rgb;
            return half4(GammaToLinearSpace(rgb), 1);
        }

            ENDCG

            SubShader
        {
            Cull Off
                Pass
            {
                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_advect
                ENDCG
            }
                Pass
            {
                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_render
                ENDCG
            }
        }
}
