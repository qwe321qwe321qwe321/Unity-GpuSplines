# Unity-GpuLines
A faster spline/line renderer for Unity using the GPU with mainly focus on the performance of modifying the fixed-number control points.

This project is inspired by the [gpuspline](https://github.com/simonboily/gpuspline) by simonboily.

# Demo
## Jobified move points
* Control points: 14000
* Splines: 2000
* Batches: 15

## Non-jobified add points, change color, move points
* Control points: 83160
* Splines: 6930
* Batches: 615

# Features
- Fast spline/line rendering using the GPU.
- Extremely low cost of modifying the control points.
  * Jobified APIs for modifying the control points.
- Support automatically batching of multiple splines/lines with the same properties. (line type, color, etc.)
- Support for the following line types:
  - Linear
  - Catmull-Rom
- Support for the following drawing modes:
  - [Graphics.DrawMesh](https://docs.unity3d.com/ScriptReference/Graphics.DrawMesh.html)
  - [Graphics.DrawProcedural](https://docs.unity3d.com/ScriptReference/Graphics.DrawProcedural.html)
    * DrawProcedural is faster than DrawMesh from preparing vertices in cpu, But it is only available on platforms that support compute buffers.

# Environment
- Unity 2019.4.40f1
- .NET 4.x
- Burst 1.6.6
- Jobs 0.2.10-preview.13

I did not test on other versions, but it should work on the versions that support Burst and Jobs.