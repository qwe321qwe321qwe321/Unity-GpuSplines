# Unity-GpuSplines
A faster spline/line renderer for Unity using the GPU, focusing mainly on the performance of **modifying control points**.

This project is inspired by the [simonboily/gpuspline](https://github.com/simonboily/gpuspline).

# Features
- Fast spline/line rendering using the GPU.
- Extremely low cost of modifying the control points.
    * Provide Jobified APIs to modify points.
    * Useful for **2D Rope** rendering, which is why I created this project:)
- Support automatically batching of multiple splines/lines with the same properties. (line type, color, etc.)
- Support for the following spline types:
    - Linear
    - Catmull-Rom
- Support for the following drawing modes:
    - [Graphics.DrawMesh](https://docs.unity3d.com/ScriptReference/Graphics.DrawMesh.html)
    - [Graphics.DrawProcedural](https://docs.unity3d.com/ScriptReference/Graphics.DrawProcedural.html)
        * DrawProcedural is faster than DrawMesh from preparing vertices in cpu, But it is only available on platforms that support compute buffers.

# Demo
## Jobified move points
* Control points: 14000
* Splines: 2000
* Batches: 15
* ~450 fps

https://github.com/qwe321qwe321qwe321/Unity-GpuLines/assets/23000374/4591f99e-202a-4e96-9493-4341d704d980

## Non-jobified add points, change color, move points
* Control points: ~83160
* Splines: ~6930
* Batches: ~615

https://github.com/qwe321qwe321qwe321/Unity-GpuLines/assets/23000374/32cf3b62-fc8b-4839-9221-0d55eaa6113f

# Environment
- Unity 2019.4.40f1
- .NET 4.x
- Burst 1.6.6
- Jobs 0.2.10-preview.13

I did not test on other versions, but it should work on the versions that support Burst and Jobs.

# Dependencies
- [stella3d/SharedArray](https://github.com/stella3d/SharedArray) - To reduce the cost of converting between NativeArray and managed array.
