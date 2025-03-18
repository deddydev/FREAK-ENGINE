﻿using XREngine.Animation;
using XREngine.Components;

namespace XREngine.Scene.Components.Animation
{
    public class AnimationClipComponent : XRComponent
    {
        private AnimationClip? _animation;
        public AnimationClip? Animation
        {
            get => _animation;
            set => SetField(ref _animation, value);
        }

        private bool _startOnActivate = false;
        public bool StartOnActivate
        {
            get => _startOnActivate;
            set => SetField(ref _startOnActivate, value);
        }

        protected override bool OnPropertyChanging<T>(string? propName, T field, T @new)
        {
            bool change = base.OnPropertyChanging(propName, field, @new);
            if (change)
            {
                switch (propName)
                {
                    case nameof(Animation):
                        Stop();
                        break;
                }
            }
            return change;
        }

        protected override void OnPropertyChanged<T>(string? propName, T prev, T field)
        {
            base.OnPropertyChanged(propName, prev, field);
            switch (propName)
            {
                case nameof(Animation):
                case nameof(StartOnActivate):
                    if (IsActiveInHierarchy && StartOnActivate)
                        Start();
                    break;
            }
        }

        private void Start()
        {
            if (Animation is null || Animation.IsPlaying)
                return;

            Animation.Start();
            RegisterAnimationTick(TickAnimation);
        }

        private void Stop()
        {
            Animation?.Stop();
            UnregisterAnimationTick(TickAnimation);
        }

        private void TickAnimation(XRWorldObjectBase @base)
        {
            Animation?.Tick(@base, Engine.Delta);
        }

        protected internal override void OnComponentActivated()
        {
            base.OnComponentActivated();

            if (Animation is not null && StartOnActivate)
                Start();
        }
        protected internal override void OnComponentDeactivated()
        {
            base.OnComponentDeactivated();

            if (Animation is not null)
                Stop();
        }
    }
}
