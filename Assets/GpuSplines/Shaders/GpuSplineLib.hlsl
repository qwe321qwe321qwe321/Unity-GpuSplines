#ifndef GPU_SPLINE_LIB
#define GPU_SPLINE_LIB


float4 _ControlPoints[1000];

inline void spline_catmull_rom(const float3 cp0, const float3 cp1, const float3 cp2, const float3 cp3, const float t, out float3 position, out float3 tangent)
{
    const float3 base0 = -cp0 + cp3 + (cp1 - cp2) * 3;
    const float3 base1 = 2*cp0 - 5*cp1 + 4*cp2 - cp3;

    position = 0.5 * (base0 * (t*t*t) + base1 * (t*t) + (-cp0+cp2)*t + 2 * cp1);
    tangent = base0 * (t * t * 1.5) + base1 * t + 0.5 * (cp2 - cp0);
}

inline void spline_linear(const float3 cp1, const float3 cp2, const float t, out float3 position, out float3 tangent)
{
    position = lerp(cp1, cp2, t);
    tangent = cp2 - cp1;
}

float3 ComputeSplineVertex(const int cp0_idx, const float t, const half leftOrRightVertex)
{
    float4 cp1 = _ControlPoints[cp0_idx + 1];
    float4 cp2 = _ControlPoints[cp0_idx + 2];
    #if CATMULLROM
    float4 cp0 = _ControlPoints[cp0_idx];
    float4 cp3 = _ControlPoints[cp0_idx + 3];
    #endif

    float3 position, tangent;
    #if CATMULLROM
    spline_catmull_rom(cp0, cp1, cp2, cp3, t, position, tangent);
    #else
    spline_linear(cp1, cp2, t, position, tangent);
    #endif
    
    // width = cp.w
    const float width = lerp(cp1.w, cp2.w, t);
    // extend half width.
    tangent = normalize(tangent) * width * 0.5;
    // Perpendicular to the tangent and view direction.
    float3 orthogonal = cross(tangent, normalize(_WorldSpaceCameraPos.xyz - position));
    return position + lerp(orthogonal, -orthogonal, leftOrRightVertex);
}

#endif