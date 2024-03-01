# Unity-GpuSplines
A faster spline/line renderer for Unity using the GPU, Job Systems + Burst Compiler, focusing mainly on the performance of **modifying control points**.

This project is inspired by the [simonboily/gpuspline](https://github.com/simonboily/gpuspline).

- [Unity-GpuSplines](#unity-gpusplines)
  - [Features](#features)
  - [Demo](#demo)
    - [Jobified modifying points](#jobified-modifying-points)
    - [Non-jobified adding points, changing color, modifying points](#non-jobified-adding-points-changing-color-modifying-points)
    - [Rope simulation (Jobified)](#rope-simulation-jobified)
  - [Installation](#installation)
  - [Dependencies](#dependencies)
  - [Getting Started](#getting-started)
  - [Environment](#environment)
  - [How I achieved the low cost of modifying control points?](#how-i-achieved-the-low-cost-of-modifying-control-points)


## Features
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
      * DrawProcedural is faster than DrawMesh since it does not need to prepare the mesh vertices in the CPU, but it is only available on platforms that support compute buffers.

## Demo
### Jobified modifying points
* Control points: 14000
* Splines: 2000
* Batches: 15
* ~450 fps
* [`TransformAccessArrayTest.cs`](./Assets/GpuSplines.Sample/TransformAccessArrayTest.cs)

https://github.com/qwe321qwe321qwe321/Unity-GpuLines/assets/23000374/4591f99e-202a-4e96-9493-4341d704d980

### Non-jobified adding points, changing color, modifying points
* Control points: ~83160
* Splines: ~6930
* Batches: ~615
* [`GpuLinesTestComponent.cs`](./Assets/GpuSplines.Sample/GpuLinesTestComponent.cs).

https://github.com/qwe321qwe321qwe321/Unity-GpuLines/assets/23000374/32cf3b62-fc8b-4839-9221-0d55eaa6113f

### Rope simulation (Jobified) 
* Control points: ~21890
* Splines: ~2189
* ~470 fps in [Bionic Bay](https://store.steampowered.com/app/1928690/Bionic_Bay/?utm_source=github&utm_campaign=reveal&utm_medium=gpusplines)

https://github.com/qwe321qwe321qwe321/Unity-GpuSplines/assets/23000374/cef63a54-2368-4187-9d99-539f7bf3eaab

## Installation
- Package Manager -> Add package from git URL `https://github.com/qwe321qwe321qwe321/Unity-GpuSplines.git?path=Assets/GpuSplines`.
- Or download the repository and copy the `Assets/GpuSplines` folder to your project.

## Dependencies
- [stella3d/SharedArray](https://github.com/stella3d/SharedArray) - To reduce the cost of converting between NativeArray and managed array.

## Getting Started
Nah... I will write it later. You can refer to the `Assets/GpuSplines.Sample/Samples.unity` scene for now.

## Environment
- Unity 2019.4.40f1
- .NET 4.x
- Burst 1.6.6
- Jobs 0.2.10-preview.13

I did not test on other versions, but it should work on the versions that support Burst and Jobs.

## How I achieved the low cost of modifying control points?
For the typical implement of line renderers, they need to update the mesh vertices on the CPU when the control points are modified. 
This is a high cost operation, especially when the number of control points is large.

I learned a trick from [simonboily/gpuspline](https://github.com/simonboily/gpuspline), which is to keep the mesh itself unchanged and only update the control points in the shader by maintaining a vector array(the maximum size is 1000). This is a low cost operation, and it is the key to the low cost of modifying control points.

After that, I improved the performance of modifying control points by:
- Constructing the Job-friendly structures for the whole Gpu spline system and provided the Jobified APIs to modify the control points to maximize the benefit from the Unity Job System and Burst Compiler.
- Optimizing the vertices data and giving an `Graphics.DrawProcedural` way to draw.
- Using the SharedArray to reduce the cost of converting between NativeArray and managed array.
