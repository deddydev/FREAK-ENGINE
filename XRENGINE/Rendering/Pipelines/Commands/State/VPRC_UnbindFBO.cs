﻿namespace XREngine.Rendering.Pipelines.Commands
{
    public class VPRC_UnbindFBO(ViewportRenderCommandContainer pipeline) : ViewportPopStateRenderCommand(pipeline)
    {
        /// <summary>
        /// The framebuffer to unbind. This should be set by bind command, and will be set to null after execution.
        /// </summary>
        public XRFrameBuffer? FrameBuffer { get; set; }

        protected override void Execute()
        {
            FrameBuffer?.UnbindFromWriting();
            FrameBuffer = null;
        }
    }
}
