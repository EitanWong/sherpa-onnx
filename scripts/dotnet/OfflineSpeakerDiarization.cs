/// Copyright (c)  2024  Xiaomi Corporation
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SherpaOnnx
{
    // IntPtr is actually a `const float*` from C++
    public delegate int OfflineSpeakerDiarizationProgressCallback(int numProcessedChunks, int numTotalChunks, IntPtr arg);

    public class OfflineSpeakerDiarization : IDisposable
    {
        public OfflineSpeakerDiarization(OfflineSpeakerDiarizationConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateOfflineSpeakerDiarization(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateOfflineSpeakerDiarization returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyOfflineSpeakerDiarization);
        }

        public void SetConfig(OfflineSpeakerDiarizationConfig config)
        {
            SherpaOnnxOfflineSpeakerDiarizationSetConfig(Handle, ref config);
        }

        public OfflineSpeakerDiarizationSegment[] Process(float[] samples)
        {
            IntPtr result = SherpaOnnxOfflineSpeakerDiarizationProcess(Handle, samples, samples.Length);
            return ProcessImpl(result);
        }

        public OfflineSpeakerDiarizationSegment[] ProcessWithCallback(float[] samples, OfflineSpeakerDiarizationProgressCallback callback, IntPtr arg)
        {
            IntPtr result = SherpaOnnxOfflineSpeakerDiarizationProcessWithCallback(Handle, samples, samples.Length, callback, arg);
            return ProcessImpl(result);
        }

        private OfflineSpeakerDiarizationSegment[] ProcessImpl(IntPtr result)
        {
            if (result == IntPtr.Zero)
            {
                return new OfflineSpeakerDiarizationSegment[] { };
            }

            int numSegments = SherpaOnnxOfflineSpeakerDiarizationResultGetNumSegments(result);
            IntPtr p = SherpaOnnxOfflineSpeakerDiarizationResultSortByStartTime(result);

            OfflineSpeakerDiarizationSegment[] ans = new OfflineSpeakerDiarizationSegment[numSegments];
            unsafe
            {
                int size = sizeof(float) * 2 + sizeof(int);
                for (int i = 0; i != numSegments; ++i)
                {
                    IntPtr t = new IntPtr((byte*)p + i * size);
                    ans[i] = new OfflineSpeakerDiarizationSegment(t);

                    // The following IntPtr.Add() does not support net20
                    // ans[i] = new OfflineSpeakerDiarizationSegment(IntPtr.Add(p, i));
                }
            }


            SherpaOnnxOfflineSpeakerDiarizationDestroySegment(p);
            SherpaOnnxOfflineSpeakerDiarizationDestroyResult(result);

            return ans;

        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~OfflineSpeakerDiarization()
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
                return SherpaOnnxOfflineSpeakerDiarizationGetSampleRate(Handle);
            }
        }
        #region P/Invoke

        private static IntPtr SherpaOnnxCreateOfflineSpeakerDiarization(ref OfflineSpeakerDiarizationConfig config)
        {
            OfflineSpeakerDiarizationConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateOfflineSpeakerDiarization(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateOfflineSpeakerDiarization(ref config);
        }

        private static void SherpaOnnxDestroyOfflineSpeakerDiarization(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOfflineSpeakerDiarization(handle),
                () => NativeExternal.SherpaOnnxDestroyOfflineSpeakerDiarization(handle));
        }

        private static int SherpaOnnxOfflineSpeakerDiarizationGetSampleRate(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationGetSampleRate(handle),
                () => NativeExternal.SherpaOnnxOfflineSpeakerDiarizationGetSampleRate(handle));
        }

        private static int SherpaOnnxOfflineSpeakerDiarizationResultGetNumSegments(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationResultGetNumSegments(handle),
                () => NativeExternal.SherpaOnnxOfflineSpeakerDiarizationResultGetNumSegments(handle));
        }

        private static IntPtr SherpaOnnxOfflineSpeakerDiarizationProcess(IntPtr handle, float[] samples, int n)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationProcess(handle, samples, n),
                () => NativeExternal.SherpaOnnxOfflineSpeakerDiarizationProcess(handle, samples, n));
        }

        private static IntPtr SherpaOnnxOfflineSpeakerDiarizationProcessWithCallback(IntPtr handle, float[] samples, int n, OfflineSpeakerDiarizationProgressCallback callback, IntPtr arg)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationProcessWithCallback(handle, samples, n, callback, arg),
                () => NativeExternal.SherpaOnnxOfflineSpeakerDiarizationProcessWithCallback(handle, samples, n, callback, arg));
        }

        private static void SherpaOnnxOfflineSpeakerDiarizationDestroyResult(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationDestroyResult(handle),
                () => NativeExternal.SherpaOnnxOfflineSpeakerDiarizationDestroyResult(handle));
        }

        private static IntPtr SherpaOnnxOfflineSpeakerDiarizationResultSortByStartTime(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationResultSortByStartTime(handle),
                () => NativeExternal.SherpaOnnxOfflineSpeakerDiarizationResultSortByStartTime(handle));
        }

        private static void SherpaOnnxOfflineSpeakerDiarizationDestroySegment(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationDestroySegment(handle),
                () => NativeExternal.SherpaOnnxOfflineSpeakerDiarizationDestroySegment(handle));
        }

        private static void SherpaOnnxOfflineSpeakerDiarizationSetConfig(IntPtr handle, ref OfflineSpeakerDiarizationConfig config)
        {
            OfflineSpeakerDiarizationConfig configCopy = config;
            if (Dll.InvokeInternal(() => NativeInternal.SherpaOnnxOfflineSpeakerDiarizationSetConfig(handle, ref configCopy)))
            {
                return;
            }
            NativeExternal.SherpaOnnxOfflineSpeakerDiarizationSetConfig(handle, ref config);
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineSpeakerDiarization(ref OfflineSpeakerDiarizationConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOfflineSpeakerDiarization(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxOfflineSpeakerDiarizationGetSampleRate(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxOfflineSpeakerDiarizationResultGetNumSegments(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxOfflineSpeakerDiarizationProcess(IntPtr handle, float[] samples, int n);

            [DllImport(Dll.Filename, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr SherpaOnnxOfflineSpeakerDiarizationProcessWithCallback(IntPtr handle, float[] samples, int n, OfflineSpeakerDiarizationProgressCallback callback, IntPtr arg);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxOfflineSpeakerDiarizationDestroyResult(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxOfflineSpeakerDiarizationResultSortByStartTime(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxOfflineSpeakerDiarizationDestroySegment(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxOfflineSpeakerDiarizationSetConfig(IntPtr handle, ref OfflineSpeakerDiarizationConfig config);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateOfflineSpeakerDiarization(ref OfflineSpeakerDiarizationConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOfflineSpeakerDiarization(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxOfflineSpeakerDiarizationGetSampleRate(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxOfflineSpeakerDiarizationResultGetNumSegments(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxOfflineSpeakerDiarizationProcess(IntPtr handle, float[] samples, int n);

            [DllImport(Dll.InternalFilename, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr SherpaOnnxOfflineSpeakerDiarizationProcessWithCallback(IntPtr handle, float[] samples, int n, OfflineSpeakerDiarizationProgressCallback callback, IntPtr arg);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxOfflineSpeakerDiarizationDestroyResult(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxOfflineSpeakerDiarizationResultSortByStartTime(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxOfflineSpeakerDiarizationDestroySegment(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxOfflineSpeakerDiarizationSetConfig(IntPtr handle, ref OfflineSpeakerDiarizationConfig config);
        }

        #endregion
    }
}
