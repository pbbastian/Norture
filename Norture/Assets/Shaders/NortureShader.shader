Shader "Custom/NortureShader" {
  Properties {
    _Color("Color", Color) = (1,1,1,1)
    _MainTex("Albedo (RGB)", 2D) = "white" {}
    _Glossiness("Smoothness", Range(0,1)) = 0.5
    _Metallic("Metallic", Range(0,1)) = 0.0
    _Cube("Cubemap", CUBE) = "white" {}
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
      };

      half _Glossiness;
      half _Metallic;
      fixed4 _Color;

      void surf(Input IN, inout SurfaceOutputStandard o) {
        float3 objectPos = normalize(mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz);
        // Albedo comes from a texture tinted by color
        fixed4 c = texCUBE(_Cube, objectPos) /* float4(objectPos * 0.5 + 0.5, 1.0) */ * _Color;
        o.Albedo = c.rgb;
        // Metallic and smoothness come from slider variables
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
        o.Alpha = c.a;
      }
      ENDCG
    }
      FallBack "Diffuse"
}
