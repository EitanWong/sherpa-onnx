/// Copyright (c)  2024.5 by 东风破
using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    public class SpeakerEmbeddingExtractor : IDisposable
    {
        public SpeakerEmbeddingExtractor(SpeakerEmbeddingExtractorConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateSpeakerEmbeddingExtractor(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateSpeakerEmbeddingExtractor returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroySpeakerEmbeddingExtractor);
        }

        public OnlineStream CreateStream()
        {
            IntPtr p = SherpaOnnxSpeakerEmbeddingExtractorCreateStream(Handle);
            return new OnlineStream(p);
        }

        public bool IsReady(OnlineStream stream)
        {
            return SherpaOnnxSpeakerEmbeddingExtractorIsReady(Handle, stream.Handle) != 0;
        }

        public float[] Compute(OnlineStream stream)
        {
            IntPtr p = SherpaOnnxSpeakerEmbeddingExtractorComputeEmbedding(Handle, stream.Handle);

            int dim = Dim;
            float[] ans = new float[dim];
            Marshal.Copy(p, ans, 0, dim);

            SherpaOnnxSpeakerEmbeddingExtractorDestroyEmbedding(p);

            return ans;
        }

        public int Dim
        {
            get
            {
                return SherpaOnnxSpeakerEmbeddingExtractorDim(Handle);
            }
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~SpeakerEmbeddingExtractor()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_handle != null)
            {
                _handle.Dispose();
                _handle = null;
            }
        }

        private IntPtr Handle
        {
            get { return _handle != null ? _handle.DangerousGetHandle() : IntPtr.Zero; }
        }

        private NativeResourceHandle _handle;
        #region P/Invoke

        private static IntPtr SherpaOnnxCreateSpeakerEmbeddingExtractor(ref SpeakerEmbeddingExtractorConfig config)
        {
            SpeakerEmbeddingExtractorConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateSpeakerEmbeddingExtractor(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateSpeakerEmbeddingExtractor(ref config);
        }

        private static void SherpaOnnxDestroySpeakerEmbeddingExtractor(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroySpeakerEmbeddingExtractor(handle),
                () => NativeExternal.SherpaOnnxDestroySpeakerEmbeddingExtractor(handle));
        }

        private static int SherpaOnnxSpeakerEmbeddingExtractorDim(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingExtractorDim(handle),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingExtractorDim(handle));
        }

        private static IntPtr SherpaOnnxSpeakerEmbeddingExtractorCreateStream(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingExtractorCreateStream(handle),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingExtractorCreateStream(handle));
        }

        private static int SherpaOnnxSpeakerEmbeddingExtractorIsReady(IntPtr handle, IntPtr stream)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingExtractorIsReady(handle, stream),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingExtractorIsReady(handle, stream));
        }

        private static IntPtr SherpaOnnxSpeakerEmbeddingExtractorComputeEmbedding(IntPtr handle, IntPtr stream)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingExtractorComputeEmbedding(handle, stream),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingExtractorComputeEmbedding(handle, stream));
        }

        private static void SherpaOnnxSpeakerEmbeddingExtractorDestroyEmbedding(IntPtr p)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpeakerEmbeddingExtractorDestroyEmbedding(p),
                () => NativeExternal.SherpaOnnxSpeakerEmbeddingExtractorDestroyEmbedding(p));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateSpeakerEmbeddingExtractor(ref SpeakerEmbeddingExtractorConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroySpeakerEmbeddingExtractor(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingExtractorDim(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingExtractorCreateStream(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingExtractorIsReady(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingExtractorComputeEmbedding(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxSpeakerEmbeddingExtractorDestroyEmbedding(IntPtr p);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateSpeakerEmbeddingExtractor(ref SpeakerEmbeddingExtractorConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroySpeakerEmbeddingExtractor(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingExtractorDim(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingExtractorCreateStream(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxSpeakerEmbeddingExtractorIsReady(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxSpeakerEmbeddingExtractorComputeEmbedding(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxSpeakerEmbeddingExtractorDestroyEmbedding(IntPtr p);
        }

        #endregion
    }

}
