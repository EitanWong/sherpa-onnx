/// Copyright (c)  2025  Xiaomi Corporation (authors: Fangjun Kuang)

using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    public class OfflineSpeechDenoiser : IDisposable
    {
        public OfflineSpeechDenoiser(OfflineSpeechDenoiserConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateOfflineSpeechDenoiser(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateOfflineSpeechDenoiser returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyOfflineSpeechDenoiser);
        }

        public DenoisedAudio Run(float[] samples, int sampleRate)
        {
            IntPtr p = SherpaOnnxOfflineSpeechDenoiserRun(Handle, samples, samples.Length, sampleRate);
            return new DenoisedAudio(p);
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~OfflineSpeechDenoiser()
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

        public int SampleRate
        {
            get
            {
                return SherpaOnnxOfflineSpeechDenoiserGetSampleRate(Handle);
            }
        }
        #region P/Invoke

        private static IntPtr SherpaOnnxCreateOfflineSpeechDenoiser(ref OfflineSpeechDenoiserConfig config)
        {
            OfflineSpeechDenoiserConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateOfflineSpeechDenoiser(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateOfflineSpeechDenoiser(ref config);
        }

        private static void SherpaOnnxDestroyOfflineSpeechDenoiser(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOfflineSpeechDenoiser(handle),
                () => NativeExternal.SherpaOnnxDestroyOfflineSpeechDenoiser(handle));
        }

        private static int SherpaOnnxOfflineSpeechDenoiserGetSampleRate(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeechDenoiserGetSampleRate(handle),
                () => NativeExternal.SherpaOnnxOfflineSpeechDenoiserGetSampleRate(handle));
        }

        private static IntPtr SherpaOnnxOfflineSpeechDenoiserRun(IntPtr handle, float[] samples, int n, int sampleRate)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeechDenoiserRun(handle, samples, n, sampleRate),
                () => NativeExternal.SherpaOnnxOfflineSpeechDenoiserRun(handle, samples, n, sampleRate));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineSpeechDenoiser(ref OfflineSpeechDenoiserConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOfflineSpeechDenoiser(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxOfflineSpeechDenoiserGetSampleRate(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxOfflineSpeechDenoiserRun(IntPtr handle, float[] samples, int n, int sampleRate);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineSpeechDenoiser(ref OfflineSpeechDenoiserConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOfflineSpeechDenoiser(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxOfflineSpeechDenoiserGetSampleRate(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxOfflineSpeechDenoiserRun(IntPtr handle, float[] samples, int n, int sampleRate);
        }

        #endregion
    }
}
