﻿using System.Numerics;
using XREngine.Components;
using XREngine.Data.Core;
using XREngine.Data.Geometry;
using XREngine.Physics.ShapeTracing;

namespace XREngine.Scene
{
    public abstract class AbstractPhysicsScene : XRBase
    {
        public event Action? OnSimulationStep;

        protected virtual void NotifySimulationStepped()
            => OnSimulationStep?.Invoke();

        public ManualResetEventSlim SimulationRunning { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim PostSimulationWorkRunning { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim DebugRendering { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim SwappingDebug { get; } = new ManualResetEventSlim(false);

        public abstract Vector3 Gravity { get; set; }

        public abstract void Initialize();
        public abstract void Destroy();
        public abstract void StepSimulation();

        public abstract void Raycast(Segment worldSegment, SortedDictionary<float, List<(XRComponent item, object? data)>> items);
        public bool Trace(ShapeTraceClosest closestTrace)
        {
            return false;
        }

        public virtual void DebugRender() { }
        public virtual void SwapDebugBuffers(){ }
        public virtual void DebugRenderCollect() { }

        public abstract void AddActor(IAbstractPhysicsActor actor);
        public abstract void RemoveActor(IAbstractPhysicsActor actor);

        public abstract void NotifyShapeChanged(IAbstractPhysicsActor actor);
    }
    public interface IAbstractPhysicsActor
    {
        void Destroy(bool wakeOnLostTouch = false);
    }
    public interface IAbstractStaticRigidBody : IAbstractRigidPhysicsActor
    {

    }
    public interface IAbstractDynamicRigidBody : IAbstractRigidBody
    {

    }
    public interface IAbstractRigidPhysicsActor : IAbstractPhysicsActor
    {
        (Vector3 position, Quaternion rotation) Transform { get; }
        Vector3 LinearVelocity { get; }
        Vector3 AngularVelocity { get; }
    }
    public interface IAbstractRigidBody : IAbstractRigidPhysicsActor
    {

    }
}