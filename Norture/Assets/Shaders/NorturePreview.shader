Shader "Custom/NorturePreview" {
  Properties {
    _Cube("Cubemap", CUBE) = "white" {}
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
  }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      LOD 200

      CGPROGRAM
      // Physically based Standard lighting model, and enable shadows on all light types
      #pragma surface surf Standard fullforwardshadows

      // Use shader model 3.0 target, to get nicer looking lighting
      #pragma target 3.0

      sampler2D _MainTex;
      samplerCUBE _Cube;

      struct Input {
        float2 uv_MainTex;
        float3 worldPos;
        float3 worldNormal;
      };

      float3 rotateToNormal(float3 normal, float3 v) {
        float a = 1.0/(1.0 + normal.z);
        float b = -normal.x*normal.y*a;
        return float3(1.0 - normal.x*normal.x*a, b, -normal.x)*v.x
             + float3(b, 1.0 - normal.y*normal.y*a, -normal.y)*v.y
             + normal*v.z;
      }

      void surf(Input IN, inout SurfaceOutputStandard o) {
        // float3 objectPos = normalize(mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz);
        // Albedo comes from a texture tinted by color
        //float3 normal = rotateToNormal(IN.worldNormal, -normalize(_WorldSpaceLightPos0.xyz));
        float3 normal = IN.worldNormal;
        fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex);
        fixed4 light = texCUBE(_Cube, normal);
        fixed4 c = albedo * light;
        o.Albedo = 0.0;
        // o.Normal = 0.0;
        o.Emission = c.rgb;
        // Metallic and smoothness come from slider variables
        o.Metallic = 0.0;
        o.Smoothness = 0.0;
        o.Alpha = c.a;
      }
      ENDCG
    }
      FallBack "Diffuse"
}
