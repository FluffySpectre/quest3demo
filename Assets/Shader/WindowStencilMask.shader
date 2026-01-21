Shader "XR/WindowStencilMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }

        Pass
        {
            ColorMask 0
            ZWrite Off

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }
}