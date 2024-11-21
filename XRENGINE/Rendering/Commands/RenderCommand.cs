﻿using XREngine.Data.Core;
using XREngine.Data.Rendering;

namespace XREngine.Rendering.Commands
{
    public abstract class RenderCommand : XRBase, IComparable<RenderCommand>, IComparable
    {
        public RenderCommand() { }
        public RenderCommand(int renderPass) => RenderPass = renderPass;

        public delegate void DelPreRender(RenderCommand command, XRCamera? camera, bool shadowPass);
        public event DelPreRender? OnPreRender;

        public delegate void DelSwapBuffers(RenderCommand command, bool shadowPass);
        public event DelSwapBuffers? OnSwapBuffers;

        private int _renderPass = (int)EDefaultRenderPass.OpaqueForward;
        /// <summary>
        /// Used by the engine for proper order of rendering.
        /// </summary>
        public int RenderPass
        {
            get => _renderPass;
            set => SetField(ref _renderPass, value);
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set => SetField(ref _enabled, value);
        }

        public abstract int CompareTo(RenderCommand? other);
        public int CompareTo(object? obj) => CompareTo(obj as RenderCommand);

        public abstract void Render(bool shadowPass);

        public virtual void PreRender(XRCamera? camera, bool shadowPass)
            => OnPreRender?.Invoke(this, camera, shadowPass);
        public void SwapBuffers(bool shadowPass)
            => OnSwapBuffers?.Invoke(this, shadowPass);
    }
}