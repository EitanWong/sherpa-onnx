/// Copyright (c)  2024.5 by 东风破

using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{

    public class OfflineStream : IDisposable
    {
        public OfflineStream(IntPtr p)
        {
            if (p == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(p), "OfflineStream handle cannot be null.");
            }

            _handle = NativeResourceHandle.Create(p, SherpaOnnxDestroyOfflineStream);
        }

        public void AcceptWaveform(int sampleRate, float[] samples)
        {
            AcceptWaveform(Handle, sampleRate, samples, samples.Length);
        }

        public OfflineRecognizerResult Result
        {
            get
            {
                IntPtr h = GetResult(Handle);
                OfflineRecognizerResult result = new OfflineRecognizerResult(h);
                DestroyResult(h);
                return result;
            }
        }

        ~OfflineStream()
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

        private static void SherpaOnnxDestroyOfflineStream(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOfflineStream(handle),
                () => NativeExternal.SherpaOnnxDestroyOfflineStream(handle));
        }

        private static void AcceptWaveform(IntPtr handle, int sampleRate, float[] samples, int n)
        {
            Dll.Invoke(
                () => NativeInternal.AcceptWaveform(handle, sampleRate, samples, n),
                () => NativeExternal.AcceptWaveform(handle, sampleRate, samples, n));
        }

        private static IntPtr GetResult(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.GetResult(handle),
                () => NativeExternal.GetResult(handle));
        }

        private static void DestroyResult(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.DestroyResult(handle),
                () => NativeExternal.DestroyResult(handle));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOfflineStream(IntPtr handle);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxAcceptWaveformOffline")]
            internal static extern void AcceptWaveform(IntPtr handle, int sampleRate, float[] samples, int n);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxGetOfflineStreamResult")]
            internal static extern IntPtr GetResult(IntPtr handle);

            [DllImport(Dll.Filename, EntryPoint = "SherpaOnnxDestroyOfflineRecognizerResult")]
            internal static extern void DestroyResult(IntPtr handle);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOfflineStream(IntPtr handle);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxAcceptWaveformOffline")]
            internal static extern void AcceptWaveform(IntPtr handle, int sampleRate, float[] samples, int n);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxGetOfflineStreamResult")]
            internal static extern IntPtr GetResult(IntPtr handle);

            [DllImport(Dll.InternalFilename, EntryPoint = "SherpaOnnxDestroyOfflineRecognizerResult")]
            internal static extern void DestroyResult(IntPtr handle);
        }

        #endregion
    }

}
