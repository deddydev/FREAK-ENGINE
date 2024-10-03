﻿using System.Numerics;
using XREngine.Data.Colors;
using XREngine.Data.Geometry;
using XREngine.Data.Rendering;
using XREngine.Rendering;
using XREngine.Rendering.Models.Materials;
using XREngine.Scene;

namespace XREngine
{
    public static partial class Engine
    {
        /// <summary>
        /// This static class contains all rendering-related functionality.
        /// </summary>
        public static partial class Rendering
        {
            /// <summary>
            /// This static class dictates the current state of rendering.
            /// </summary>
            public static partial class State
            {
                public static BoundingRectangle RenderArea => PipelineState?.CurrentRenderRegion ?? BoundingRectangle.Empty;
                public static XRWorldInstance? RenderingWorld => RenderingViewport?.World;
                public static XRViewport? RenderingViewport => PipelineState?.WindowViewport;
                public static VisualScene? RenderingScene => PipelineState?.MainScene;
                public static XRCamera? RenderingCamera => PipelineState?.RenderingCamera;
                public static XRFrameBuffer? TargetOutputFBO => PipelineState?.OutputFBO;
                public static XRMaterial? OverrideMaterial => PipelineState?.OverrideMaterial;

                internal static XRRenderPipelineInstance? CurrentPipeline { get; set; }
                public static XRRenderPipelineInstance.RenderingState? PipelineState => CurrentPipeline?.State;

                public static void ClearColor(ColorF4 color)
                    => AbstractRenderer.Current?.ClearColor(color);
                public static void ClearStencil(int v)
                    => AbstractRenderer.Current?.ClearStencil(v);
                public static void ClearDepth(float v)
                    => AbstractRenderer.Current?.ClearDepth(v);

                public static void Clear(bool color, bool depth, bool stencil)
                    => AbstractRenderer.Current?.Clear(color, depth, stencil);

                public static void BindFrameBuffer(EFramebufferTarget fboTarget, int bindingId)
                    => AbstractRenderer.Current?.BindFrameBuffer(fboTarget, bindingId);

                public static void SetReadBuffer(EDrawBuffersAttachment attachment)
                    => AbstractRenderer.Current?.SetReadBuffer(attachment);

                public static float GetDepth(float x, float y)
                    => AbstractRenderer.Current?.GetDepth(x, y) ?? 0.0f;

                public static byte GetStencilIndex(float x, float y)
                    => AbstractRenderer.Current?.GetStencilIndex(x, y) ?? 0;

                public static void EnableDepthTest(bool v)
                    => AbstractRenderer.Current?.EnableDepthTest(v);

                public static void StencilMask(uint mask)
                    => AbstractRenderer.Current?.StencilMask(mask);

                public static void AllowDepthWrite(bool v)
                    => AbstractRenderer.Current?.AllowDepthWrite(v);

                public static void DepthFunc(EComparison always)
                    => AbstractRenderer.Current?.DepthFunc(always);

                public static bool CalcDotLuminance(XRTexture2D texture, out float dotLuminance, bool genMipmapsNow)
                {
                    dotLuminance = 1.0f;
                    return AbstractRenderer.Current?.CalcDotLuminance(texture, out dotLuminance, genMipmapsNow) ?? false;
                }
                public static float CalculateDotLuminance(XRTexture2D texture, bool generateMipmapsNow)
                    => CalcDotLuminance(texture, out float dotLum, generateMipmapsNow) ? dotLum : 1.0f;
            }
        }
    }
}