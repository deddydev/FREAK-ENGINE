﻿namespace XREngine.Rendering.Pipelines.Commands
{
    public class VPRC_PushOutputFBORenderArea : ViewportStateRenderCommand<VPRC_PopRenderArea>
    {
        protected override void Execute()
        {
            var fbo = Pipeline.RenderState.OutputFBO;
            if (fbo is null)
            {
                PopCommand.ShouldExecute = false;
                return;
            }

            Pipeline.RenderState.PushRenderArea((int)fbo.Width, (int)fbo.Height);
        }
    }
}
