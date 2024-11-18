﻿using MagicPhysX;
using XREngine.Data.Core;

namespace XREngine.Rendering.Physics.Physx
{
    public unsafe abstract class PhysxBase : XRBase
    {
        public abstract PxBase* Base { get; }
    }
}