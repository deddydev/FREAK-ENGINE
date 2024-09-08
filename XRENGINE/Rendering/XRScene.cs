﻿using XREngine.Core.Files;

namespace XREngine.Scene
{
    /// <summary>
    /// Defines a collection of root scene nodes that can be loaded in and out of a world.
    /// </summary>
    public class XRScene : XRAsset
    {
        private bool _isVisible = true;
        internal readonly List<SceneNode> _rootObjects = [];

        /// <summary>
        /// The world this scene is currently loaded into.
        /// </summary>
        public XRWorld? ParentWorld { get; set; }

        /// <summary>
        /// If the scene is currently visible in the world.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            internal set => SetField(ref _isVisible, value);
        }

        /// <summary>
        /// All nodes that are at the root of the scene.
        /// Nodes can have any number of children, recursively.
        /// </summary>
        public IReadOnlyList<SceneNode> RootNodes => _rootObjects;
    }
}
