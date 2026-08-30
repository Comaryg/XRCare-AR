Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { 
            "Queue" = "Geometry-1"
            "RenderType" = "Opaque"
        }

        Pass
        {
            ZWrite On
            ZTest LEqual
            ColorMask 0
        }
    }

    Fallback Off
}