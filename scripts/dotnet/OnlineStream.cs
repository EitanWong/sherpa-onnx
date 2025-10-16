/// Copyright (c)  2023  Xiaomi Corporation (authors: Fangjun Kuang)
/// Copyright (c)  2023 by manyeyes
/// Copyright (c)  2024.5 by 东风破
using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{

    public class OnlineStream : IDisposable
    {
        public OnlineStream(IntPtr p)
        {
            if (p == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(p), "OnlineStream handle cannot be null.");
            }

            _handle = NativeResourceHandle.Create(p, SherpaOnnxDestroyOnlineStream);
        }

        public void AcceptWaveform(int sampleRate, float[] samples)
        {
            SherpaOnnxOnlineStreamAcceptWaveform(Handle, sampleRate, samples, samples.Length);
        }

        public void InputFinished()
        {
            SherpaOnnxOnlineStreamInputFinished(Handle);
        }

        ~OnlineStream()
        {
            Cleanup();
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
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
        public IntPtr Handle
        {
            get { return _handle != null ? _handle.DangerousGetHandle() : IntPtr.Zero; }
        }

        #region P/Invoke

        private static void SherpaOnnxDestroyOnlineStream(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOnlineStream(handle),
                () => NativeExternal.SherpaOnnxDestroyOnlineStream(handle));
        }

        private static void SherpaOnnxOnlineStreamAcceptWaveform(IntPtr handle, int sampleRate, float[] samples, int n)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxOnlineStreamAcceptWaveform(handle, sampleRate, samples, n),
                () => NativeExternal.SherpaOnnxOnlineStreamAcceptWaveform(handle, sampleRate, samples, n));
        }

        private static void SherpaOnnxOnlineStreamInputFinished(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxOnlineStreamInputFinished(handle),
                () => NativeExternal.SherpaOnnxOnlineStreamInputFinished(handle));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOnlineStream(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxOnlineStreamAcceptWaveform(IntPtr handle, int sampleRate, float[] samples, int n);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxOnlineStreamInputFinished(IntPtr handle);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOnlineStream(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxOnlineStreamAcceptWaveform(IntPtr handle, int sampleRate, float[] samples, int n);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxOnlineStreamInputFinished(IntPtr handle);
        }

        #endregion
    }

}
