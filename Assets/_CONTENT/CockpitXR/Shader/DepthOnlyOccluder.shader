Shader "XR/DepthOnlyOccluder"
{
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry-2"
            "RenderType" = "Opaque"
        }

        ZWrite On
        ZTest LEqual
        ColorMask 0

        Pass {}
    }
}