/// Copyright (c)  2024.5 by 东风破
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SherpaOnnx
{
    public class OfflineTtsGeneratedAudio : IDisposable
    {
        public OfflineTtsGeneratedAudio(IntPtr p)
        {
            if (p == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(p), "Generated audio handle cannot be null.");
            }

            _handle = NativeResourceHandle.Create(p, SherpaOnnxDestroyOfflineTtsGeneratedAudio);
        }

        public bool SaveToWaveFile(String filename)
        {
            Impl impl = (Impl)Marshal.PtrToStructure(Handle, typeof(Impl));
            byte[] utf8Filename = Encoding.UTF8.GetBytes(filename);
            byte[] utf8FilenameWithNull = new byte[utf8Filename.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Filename, utf8FilenameWithNull, utf8Filename.Length);
            utf8FilenameWithNull[utf8Filename.Length] = 0; // Null terminator
            int status = SherpaOnnxWriteWave(impl.Samples, impl.NumSamples, impl.SampleRate, utf8FilenameWithNull);
            return status == 1;
        }

        ~OfflineTtsGeneratedAudio()
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

        [StructLayout(LayoutKind.Sequential)]
        struct Impl
        {
            public IntPtr Samples;
            public int NumSamples;
            public int SampleRate;
        }

        private NativeResourceHandle _handle;
        public IntPtr Handle
        {
            get { return _handle != null ? _handle.DangerousGetHandle() : IntPtr.Zero; }
        }

        public int NumSamples
        {
            get
            {
                Impl impl = (Impl)Marshal.PtrToStructure(Handle, typeof(Impl));
                return impl.NumSamples;
            }
        }

        public int SampleRate
        {
            get
            {
                Impl impl = (Impl)Marshal.PtrToStructure(Handle, typeof(Impl));
                return impl.SampleRate;
            }
        }

        public float[] Samples
        {
            get
            {
                Impl impl = (Impl)Marshal.PtrToStructure(Handle, typeof(Impl));

                float[] samples = new float[impl.NumSamples];
                Marshal.Copy(impl.Samples, samples, 0, impl.NumSamples);
                return samples;
            }
        }
        #region P/Invoke

        private static void SherpaOnnxDestroyOfflineTtsGeneratedAudio(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOfflineTtsGeneratedAudio(handle),
                () => NativeExternal.SherpaOnnxDestroyOfflineTtsGeneratedAudio(handle));
        }

        private static int SherpaOnnxWriteWave(IntPtr samples, int n, int sample_rate, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Filename)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxWriteWave(samples, n, sample_rate, utf8Filename),
                () => NativeExternal.SherpaOnnxWriteWave(samples, n, sample_rate, utf8Filename));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOfflineTtsGeneratedAudio(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxWriteWave(IntPtr samples, int n, int sample_rate, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Filename);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOfflineTtsGeneratedAudio(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxWriteWave(IntPtr samples, int n, int sample_rate, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Filename);
        }

        #endregion
    }
}
