﻿using Extensions;
using Silk.NET.OpenGL;
using XREngine.Data;
using XREngine.Data.Core;
using XREngine.Data.Rendering;
using static XREngine.Rendering.OpenGL.OpenGLRenderer;

namespace XREngine.Rendering.OpenGL
{
    public class GLTexture2DArray(OpenGLRenderer renderer, XRTexture2DArray data) : GLTexture<XRTexture2DArray>(renderer, data)
    {
        private bool _storageSet = false;

        public class MipmapInfo : XRBase
        {
            private bool _hasPushedResizedData = false;
            //private bool _hasPushedUpdateData = false;
            private readonly Mipmap2D _mipmap;
            private readonly GLTexture2DArray _textureArray;

            public Mipmap2D Mipmap => _mipmap;

            public MipmapInfo(GLTexture2DArray textureArray, Mipmap2D mipmap)
            {
                _textureArray = textureArray;
                _mipmap = mipmap;
                _mipmap.PropertyChanged += MipmapPropertyChanged;
                _mipmap.Invalidated += MipmapInvalidated;
            }

            ~MipmapInfo()
            {
                _mipmap.PropertyChanged -= MipmapPropertyChanged;
                _mipmap.Invalidated -= MipmapInvalidated;
            }

            private void MipmapInvalidated()
            {
                _textureArray.Invalidate();
            }

            private void MipmapPropertyChanged(object? sender, IXRPropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(Mipmap.Data):
                        //HasPushedUpdateData = false;
                        _textureArray.Invalidate();
                        break;
                    case nameof(Mipmap.Width):
                    case nameof(Mipmap.Height):
                    case nameof(Mipmap.InternalFormat):
                    case nameof(Mipmap.PixelFormat):
                    case nameof(Mipmap.PixelType):
                        HasPushedResizedData = false;
                        //HasPushedUpdateData = false;
                        _textureArray.Invalidate();
                        break;
                }
            }

            public bool HasPushedResizedData
            {
                get => _hasPushedResizedData;
                set => SetField(ref _hasPushedResizedData, value);
            }
            //public bool HasPushedUpdateData
            //{
            //    get => _hasPushedUpdateData;
            //    set => SetField(ref _hasPushedUpdateData, value);
            //}
        }

        public MipmapInfo[] Mipmaps { get; private set; } = [];

        protected override void DataPropertyChanged(object? sender, IXRPropertyChangedEventArgs e)
        {
            base.DataPropertyChanged(sender, e);
            switch (e.PropertyName)
            {
                case nameof(XRTexture2DArray.Mipmaps):
                    {
                        UpdateMipmaps();
                        break;
                    }
            }
        }

        private void UpdateMipmaps()
        {
            if (Data.Mipmaps is null || Data.Mipmaps.Length == 0)
            {
                Mipmaps = [];
                Invalidate();
                return;
            }

            Mipmaps = new MipmapInfo[Data.Mipmaps.Length];
            for (int i = 0; i < Data.Mipmaps.Length; ++i)
                Mipmaps[i] = new MipmapInfo(this, Data.Mipmaps[i]);

            Invalidate();
        }

        public override ETextureTarget TextureTarget 
            => Data.MultiSample 
            ? ETextureTarget.Texture2DMultisampleArray 
            : ETextureTarget.Texture2DArray;

        protected override void UnlinkData()
        {
            base.UnlinkData();

            Data.AttachToFBORequested_OVRMultiView -= AttachToFBO_OVRMultiView;
            Data.DetachFromFBORequested_OVRMultiView -= DetachFromFBO_OVRMultiView;
            Data.PushDataRequested -= PushData;
            Data.BindRequested -= Bind;
            Data.UnbindRequested -= Unbind;
            Data.AttachImageToFBORequested -= AttachImageToFBO;
            Data.DetachImageFromFBORequested -= DetachImageFromFBO;
            Data.Resized -= DataResized;
            Mipmaps = [];
        }
        protected override void LinkData()
        {
            base.LinkData();

            Data.AttachToFBORequested_OVRMultiView += AttachToFBO_OVRMultiView;
            Data.DetachFromFBORequested_OVRMultiView += DetachFromFBO_OVRMultiView;
            Data.PushDataRequested += PushData;
            Data.AttachImageToFBORequested += AttachImageToFBO;
            Data.DetachImageFromFBORequested += DetachImageFromFBO;
            Data.BindRequested += Bind;
            Data.UnbindRequested += Unbind;
            Data.Resized += DataResized;
            UpdateMipmaps();
        }

        public override void AttachToFBO(XRFrameBuffer fbo, EFrameBufferAttachment attachment, int mipLevel = 0)
        {
            for (int i = 0; i < Data.Depth; ++i)
                AttachImageToFBO(fbo, attachment, i, mipLevel);
        }
        public override void DetachFromFBO(XRFrameBuffer fbo, EFrameBufferAttachment attachment, int mipLevel = 0)
        {
            for (int i = 0; i < Data.Depth; ++i)
                DetachImageFromFBO(fbo, attachment, i, mipLevel);
        }

        private void DetachImageFromFBO(XRFrameBuffer target, EFrameBufferAttachment attachment, int layer, int mipLevel)
        {
            if (Renderer.GetOrCreateAPIRenderObject(target) is not GLObjectBase apiFBO)
                return;

            Api.NamedFramebufferTextureLayer(apiFBO.BindingId, ToGLEnum(attachment), 0, mipLevel, layer);
            //Api.FramebufferTexture2D(GLEnum.Framebuffer, ToGLEnum(attachment), GLEnum.TextureCubeMapPositiveX + layer, BindingId, mipLevel);
        }

        private void AttachImageToFBO(XRFrameBuffer target, EFrameBufferAttachment attachment, int layer, int mipLevel)
        {
            if (Renderer.GetOrCreateAPIRenderObject(target) is not GLObjectBase apiFBO)
                return;

            Api.NamedFramebufferTextureLayer(apiFBO.BindingId, ToGLEnum(attachment), BindingId, mipLevel, layer);
            //Api.FramebufferTexture2D(GLEnum.Framebuffer, ToGLEnum(attachment), GLEnum.TextureCubeMapPositiveX + layer, BindingId, mipLevel);
        }

        private void DataResized()
        {
            _storageSet = false;
            Mipmaps.ForEach(m =>
            {
                m.HasPushedResizedData = false;
                //m.HasPushedUpdateData = false;
            });
            Invalidate();
        }

        protected internal override void PostGenerated()
        {
            Mipmaps.ForEach(m =>
            {
                m.HasPushedResizedData = false;
                //m.HasPushedUpdateData = false;
            });
            _storageSet = false;
            base.PostGenerated();
        }
        protected internal override void PostDeleted()
        {
            _storageSet = false;
            base.PostDeleted();
        }

        public override void PushData()
        {
            if (IsPushing)
                return;
            try
            {
                IsPushing = true;

                OnPrePushData(out bool shouldPush, out bool allowPostPushCallback);
                if (!shouldPush)
                {
                    if (allowPostPushCallback)
                        OnPostPushData();
                    IsPushing = false;
                    return;
                }

                Bind();

                Api.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                EPixelInternalFormat? internalFormatForce = null;
                if (!Data.Resizable && !_storageSet)
                {
                    Api.TextureStorage3D(BindingId, (uint)Data.SmallestMipmapLevel, ToGLEnum(Data.SizedInternalFormat), Data.Width, Data.Height, Data.Depth);
                    internalFormatForce = ToBaseInternalFormat(Data.SizedInternalFormat);
                    _storageSet = true;
                }

                var glTarget = ToGLEnum(TextureTarget);
                if (Mipmaps is null || Mipmaps.Length == 0)
                    PushMipmap(glTarget, 0, null, internalFormatForce);
                else
                {
                    for (int i = 0; i < Mipmaps.Length; ++i)
                        PushMipmap(glTarget, i, Mipmaps[i], internalFormatForce);
                }

                int baseLevel = 0;
                int maxLevel = 1000;
                int minLOD = -1000;
                int maxLOD = 1000;

                Api.TextureParameterI(BindingId, GLEnum.TextureBaseLevel, in baseLevel);
                Api.TextureParameterI(BindingId, GLEnum.TextureMaxLevel, in maxLevel);
                Api.TextureParameterI(BindingId, GLEnum.TextureMinLod, in minLOD);
                Api.TextureParameterI(BindingId, GLEnum.TextureMaxLod, in maxLOD);

                if (Data.AutoGenerateMipmaps)
                    GenerateMipmaps();

                if (allowPostPushCallback)
                    OnPostPushData();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                IsPushing = false;
                Unbind();
            }
        }

        private unsafe void PushMipmap(GLEnum glTarget, int i, MipmapInfo? info, EPixelInternalFormat? internalFormatForce)
        {
            if (!Data.Resizable && !_storageSet)
            {
                Debug.LogWarning("Texture storage not set on non-resizable texture, can't push mipmaps.");
                return;
            }

            GLEnum pixelFormat;
            GLEnum pixelType;
            InternalFormat internalPixelFormat;

            DataSource? data;
            bool hasPushedResized;
            Mipmap2D? mip = info?.Mipmap;
            if (mip is null)
            {
                internalPixelFormat = ToInternalFormat(internalFormatForce ?? EPixelInternalFormat.Rgb);
                pixelFormat = GLEnum.Rgb;
                pixelType = GLEnum.UnsignedByte;
                data = null;
                hasPushedResized = false;
            }
            else
            {
                pixelFormat = ToGLEnum(mip.PixelFormat);
                pixelType = ToGLEnum(mip.PixelType);
                internalPixelFormat = ToInternalFormat(internalFormatForce ?? mip.InternalFormat);
                data = mip.Data;
                hasPushedResized = info!.HasPushedResizedData;
            }

            if (data is null || data.Length == 0)
                Push(glTarget, i, Data.Width >> i, Data.Height >> i, pixelFormat, pixelType, internalPixelFormat, hasPushedResized);
            else
            {
                Push(glTarget, i, mip!.Width, mip.Height, pixelFormat, pixelType, internalPixelFormat, data, hasPushedResized);
                data?.Dispose();
            }

            if (info != null)
            {
                info.HasPushedResizedData = true;
                //info.HasPushedUpdateData = true;
            }
        }

        //TODO: allow pushing specific images in the array instead of the whole array at once

        private unsafe void Push(GLEnum glTarget, int i, uint w, uint h, GLEnum pixelFormat, GLEnum pixelType, InternalFormat internalPixelFormat, bool hasPushedResized)
        {
            if (!hasPushedResized && Data.Resizable)
                Api.TexImage3D(glTarget, i, internalPixelFormat, w, h, Data.Depth, 0, pixelFormat, pixelType, IntPtr.Zero.ToPointer());
        }

        private unsafe void Push(GLEnum glTarget, int i, uint w, uint h, GLEnum pixelFormat, GLEnum pixelType, InternalFormat internalPixelFormat, DataSource bmp, bool hasPushedResized)
        {
            // If a non-zero named buffer object is bound to the GL_PIXEL_UNPACK_BUFFER target (see glBindBuffer) while a texture image is specified,
            // the data ptr is treated as a byte offset into the buffer object's data store.
            void* ptr = bmp.Address.Pointer;
            if (ptr is null)
            {
                Debug.LogWarning("Texture data source is null.");
                return;
            }

            if (hasPushedResized || _storageSet)
                Api.TexSubImage3D(glTarget, i, 0, 0, 0, w, h, Data.Depth, pixelFormat, pixelType, ptr);
            else
                Api.TexImage3D(glTarget, i, internalPixelFormat, w, h, Data.Depth, 0, pixelFormat, pixelType, ptr);
        }
    }
}