/// Copyright (c)  2024.5 by 东风破
using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    public class SpokenLanguageIdentification : IDisposable
    {
        public SpokenLanguageIdentification(SpokenLanguageIdentificationConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateSpokenLanguageIdentification(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateSpokenLanguageIdentification returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroySpokenLanguageIdentification);
        }

        public OfflineStream CreateStream()
        {
            IntPtr p = SherpaOnnxSpokenLanguageIdentificationCreateOfflineStream(Handle);
            return new OfflineStream(p);
        }

        public SpokenLanguageIdentificationResult Compute(OfflineStream stream)
        {
            IntPtr h = SherpaOnnxSpokenLanguageIdentificationCompute(Handle, stream.Handle);
            SpokenLanguageIdentificationResult result = new SpokenLanguageIdentificationResult(h);
            SherpaOnnxDestroySpokenLanguageIdentificationResult(h);
            return result;
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~SpokenLanguageIdentification()
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

        private NativeResourceHandle _handle;

        private IntPtr Handle
        {
            get { return _handle != null ? _handle.DangerousGetHandle() : IntPtr.Zero; }
        }
        #region P/Invoke

        private static IntPtr SherpaOnnxCreateSpokenLanguageIdentification(ref SpokenLanguageIdentificationConfig config)
        {
            SpokenLanguageIdentificationConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateSpokenLanguageIdentification(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateSpokenLanguageIdentification(ref config);
        }

        private static void SherpaOnnxDestroySpokenLanguageIdentification(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroySpokenLanguageIdentification(handle),
                () => NativeExternal.SherpaOnnxDestroySpokenLanguageIdentification(handle));
        }

        private static IntPtr SherpaOnnxSpokenLanguageIdentificationCreateOfflineStream(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpokenLanguageIdentificationCreateOfflineStream(handle),
                () => NativeExternal.SherpaOnnxSpokenLanguageIdentificationCreateOfflineStream(handle));
        }

        private static IntPtr SherpaOnnxSpokenLanguageIdentificationCompute(IntPtr handle, IntPtr stream)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxSpokenLanguageIdentificationCompute(handle, stream),
                () => NativeExternal.SherpaOnnxSpokenLanguageIdentificationCompute(handle, stream));
        }

        private static void SherpaOnnxDestroySpokenLanguageIdentificationResult(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroySpokenLanguageIdentificationResult(handle),
                () => NativeExternal.SherpaOnnxDestroySpokenLanguageIdentificationResult(handle));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateSpokenLanguageIdentification(ref SpokenLanguageIdentificationConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroySpokenLanguageIdentification(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxSpokenLanguageIdentificationCreateOfflineStream(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxSpokenLanguageIdentificationCompute(IntPtr handle, IntPtr stream);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroySpokenLanguageIdentificationResult(IntPtr handle);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateSpokenLanguageIdentification(ref SpokenLanguageIdentificationConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroySpokenLanguageIdentification(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxSpokenLanguageIdentificationCreateOfflineStream(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxSpokenLanguageIdentificationCompute(IntPtr handle, IntPtr stream);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroySpokenLanguageIdentificationResult(IntPtr handle);
        }

        #endregion
    }
}
