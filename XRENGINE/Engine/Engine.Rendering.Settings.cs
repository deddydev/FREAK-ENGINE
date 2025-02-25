﻿using System.Numerics;
using XREngine.Core.Files;
using XREngine.Data.Colors;

namespace XREngine
{
    public static partial class Engine
    {
        public static partial class Rendering
        {
            public static event Action? SettingsChanged;

            private static EngineSettings _settings = new();
            /// <summary>
            /// The global rendering settings for the engine.
            /// </summary>
            public static EngineSettings Settings
            {
                get => _settings;
                set
                {
                    if (_settings == value)
                        return;

                    _settings = value;
                    SettingsChanged?.Invoke();
                }
            }

            /// <summary>
            /// Contains global rendering settings.
            /// </summary>
            public partial class EngineSettings : XRAsset
            {
                private Vector3 _defaultLuminance = new(0.299f, 0.587f, 0.114f);
                private bool _allowShaderPipelines = true;
                private bool _useIntegerUniformsInShaders = true;
                private bool _optimizeTo4Weights = false;
                private bool _optimizeWeightsIfPossible = true;
                private bool _tickGroupedItemsInParallel = true;
                private bool _recalcChildMatricesInParallel = true;
                private uint _lightProbeResolution = 512u;
                private bool _lightProbesCaptureDepth = false;
                private uint _lightProbeDepthResolution = 256u;
                private bool _allowBinaryProgramCaching = true;
                private bool _calculateBlendshapesInComputeShader = false;
                private bool _calculateSkinningInComputeShader = false;
                private string _defaultFontFolder = "Roboto";
                private string _defaultFontFileName = "Roboto-Medium.ttf";
                private bool _renderTransformDebugInfo = false;
                private bool _renderMesh3DBounds = false;
                private bool _renderMesh2DBounds = false;
                private bool _renderUITransformCoordinate = true;
                private bool _renderTransformLines = true;
                private bool _renderTransformPoints = false;
                private bool _renderTransformCapsules = false;
                private bool _visualizeDirectionalLightVolumes = false;

                /// <summary>
                /// The default luminance used for calculation of exposure, etc.
                /// </summary>
                public Vector3 DefaultLuminance
                {
                    get => _defaultLuminance;
                    set => SetField(ref _defaultLuminance, value);
                }

                /// <summary>
                /// Shader pipelines allow for dynamic combination of shaders at runtime, such as mixing and matching vertex and fragment shaders.
                /// When this is off, a new shader program must be compiled for each unique combination of shaders.
                /// </summary>
                public bool AllowShaderPipelines
                {
                    get => _allowShaderPipelines;
                    set => SetField(ref _allowShaderPipelines, value);
                }
                /// <summary>
                /// When true, the engine will use integers in shaders instead of floats when needed.
                /// </summary>
                public bool UseIntegerUniformsInShaders
                {
                    get => _useIntegerUniformsInShaders;
                    set => SetField(ref _useIntegerUniformsInShaders, value);
                }
                /// <summary>
                /// When true, the engine will optimize the number of bone weights used per vertex if any vertex uses more than 4 weights.
                /// Will reduce shader calculations at the expense of skinning quality.
                /// </summary>
                public bool OptimizeSkinningTo4Weights
                {
                    get => _optimizeTo4Weights;
                    set => SetField(ref _optimizeTo4Weights, value);
                }
                /// <summary>
                /// This will pass vertex weights and indices to the shader as elements of a vec4 instead of using SSBO remaps for more straightforward calculation.
                /// Will not result in any quality loss and should be enabled if possible.
                /// </summary>
                public bool OptimizeSkinningWeightsIfPossible
                {
                    get => _optimizeWeightsIfPossible;
                    set => SetField(ref _optimizeWeightsIfPossible, value);
                }
                /// <summary>
                /// When items in the same group also have the same order value, this will dictate whether they are ticked in parallel or sequentially.
                /// Depending on how many items are in a singular tick order, this could be faster or slower.
                /// </summary>
                public bool TickGroupedItemsInParallel
                {
                    get => _tickGroupedItemsInParallel;
                    set => SetField(ref _tickGroupedItemsInParallel, value);
                }
                /// <summary>
                /// If true, when calculating matrix hierarchies, the engine will calculate a transform's child matrices in parallel.
                /// </summary>
                public bool RecalcChildMatricesInParallel
                {
                    get => _recalcChildMatricesInParallel;
                    set => SetField(ref _recalcChildMatricesInParallel, value);
                }
                /// <summary>
                /// The default resolution of the light probe color texture.
                /// </summary>
                public uint LightProbeResolution
                {
                    get => _lightProbeResolution;
                    set => SetField(ref _lightProbeResolution, value);
                }
                /// <summary>
                /// If true, the light probes will also capture depth information.
                /// </summary>
                public bool LightProbesCaptureDepth
                {
                    get => _lightProbesCaptureDepth;
                    set => SetField(ref _lightProbesCaptureDepth, value);
                }
                /// <summary>
                /// If true, the engine will cache compiled binary programs for faster loading times on next startups until the GPU driver is updated.
                /// </summary>
                public bool AllowBinaryProgramCaching 
                {
                    get => _allowBinaryProgramCaching;
                    set => SetField(ref _allowBinaryProgramCaching, value);
                }
                /// <summary>
                /// If true, the engine will render the bounds of each 3D mesh.
                /// Useful for debugging, but should be disabled in production builds.
                /// </summary>
                public bool RenderMesh3DBounds 
                {
                    get => _renderMesh3DBounds;
                    set => SetField(ref _renderMesh3DBounds, value);
                }
                /// <summary>
                /// If true, the engine will render the bounds of each UI mesh.
                /// Useful for debugging, but should be disabled in production builds.
                /// </summary>
                public bool RenderMesh2DBounds
                {
                    get => _renderMesh2DBounds;
                    set => SetField(ref _renderMesh2DBounds, value);
                }             
                /// <summary>
                /// If true, the engine will render all transforms in the scene as lines and points.
                /// </summary>
                public bool RenderTransformDebugInfo
                {
                    get => _renderTransformDebugInfo;
                    set => SetField(ref _renderTransformDebugInfo, value);
                }
                public bool RenderUITransformCoordinate
                {
                    get => _renderUITransformCoordinate;
                    set => SetField(ref _renderUITransformCoordinate, value);
                }
                public bool RenderTransformLines
                {
                    get => _renderTransformLines;
                    set => SetField(ref _renderTransformLines, value);
                }
                public bool RenderTransformPoints
                {
                    get => _renderTransformPoints;
                    set => SetField(ref _renderTransformPoints, value);
                }
                public bool RenderTransformCapsules
                {
                    get => _renderTransformCapsules;
                    set => SetField(ref _renderTransformCapsules, value);
                }
                public bool VisualizeDirectionalLightVolumes
                {
                    get => _visualizeDirectionalLightVolumes;
                    set => SetField(ref _visualizeDirectionalLightVolumes, value);
                }
                /// <summary>
                /// If true, the engine will calculate blendshapes in a compute shader rather than the vertex shader.
                /// Performance gain or loss may vary depending on the GPU.
                /// </summary>
                public bool CalculateBlendshapesInComputeShader
                {
                    get => _calculateBlendshapesInComputeShader;
                    set => SetField(ref _calculateBlendshapesInComputeShader, value);
                }
                /// <summary>
                /// If true, the engine will calculate skinning in a compute shader rather than the vertex shader.
                /// Performance gain or loss may vary depending on the GPU.
                /// </summary>
                public bool CalculateSkinningInComputeShader
                {
                    get => _calculateSkinningInComputeShader;
                    set => SetField(ref _calculateSkinningInComputeShader, value);
                }
                /// <summary>
                /// The name of the default font's folder within the engine's font directory.
                /// </summary>
                public string DefaultFontFolder 
                {
                    get => _defaultFontFolder;
                    set => SetField(ref _defaultFontFolder, value);
                }
                /// <summary>
                /// The name of the font file within the DefaultFontFolder directory.
                /// TTF or OTF files are supported, and the extension should be included in the string.
                /// </summary>
                public string DefaultFontFileName 
                {
                    get => _defaultFontFileName;
                    set => SetField(ref _defaultFontFileName, value);
                }
                public ColorF4 QuadtreeIntersectedBoundsColor { get; set; } = ColorF4.LightGray;
                public ColorF4 QuadtreeContainedBoundsColor { get; set; } = ColorF4.Yellow;
                public ColorF4 OctreeIntersectedBoundsColor { get; set; } = ColorF4.LightGray;
                public ColorF4 OctreeContainedBoundsColor { get; set; } = ColorF4.Yellow;
                public ColorF4 Bounds2DColor { get; set; } = ColorF4.LightLavender;
                public ColorF4 Bounds3DColor { get; set; } = ColorF4.LightLavender;
                public ColorF4 TransformPointColor { get; set; } = ColorF4.Orange;
                public ColorF4 TransformLineColor { get; set; } = ColorF4.LightRed;
                public ColorF4 TransformCapsuleColor { get; set; } = ColorF4.LightOrange;
                public bool AllowSkinning { get; set; } = true;
                public bool AllowBlendshapes { get; set; } = true;
                public bool RemapBlendshapeDeltas { get; set; } = true;
                public bool UseAbsoluteBlendshapePositions { get; set; } = false;
                public bool LogVRFrameTimes { get; set; } = false;
            }
        }
    }
}