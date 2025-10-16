/// Copyright (c)  2024  Xiaomi Corporation (authors: Fangjun Kuang)
using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    public class VoiceActivityDetector : IDisposable
    {
        public VoiceActivityDetector(VadModelConfig config, float bufferSizeInSeconds)
        {
            IntPtr pointer = SherpaOnnxCreateVoiceActivityDetector(ref config, bufferSizeInSeconds);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateVoiceActivityDetector returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyVoiceActivityDetector);
        }

        public void AcceptWaveform(float[] samples)
        {
            SherpaOnnxVoiceActivityDetectorAcceptWaveform(Handle, samples, samples.Length);
        }

        public bool IsEmpty()
        {
            return SherpaOnnxVoiceActivityDetectorEmpty(Handle) == 1;
        }

        public bool IsSpeechDetected()
        {
            return SherpaOnnxVoiceActivityDetectorDetected(Handle) == 1;
        }

        public void Pop()
        {
            SherpaOnnxVoiceActivityDetectorPop(Handle);
        }

        public SpeechSegment Front()
        {
            IntPtr p = SherpaOnnxVoiceActivityDetectorFront(Handle);

            SpeechSegment segment = new SpeechSegment(p);

            SherpaOnnxDestroySpeechSegment(p);

            return segment;
        }

        public void Clear()
        {
            SherpaOnnxVoiceActivityDetectorClear(Handle);
        }

        public void Reset()
        {
            SherpaOnnxVoiceActivityDetectorReset(Handle);
        }

        public void Flush()
        {
            SherpaOnnxVoiceActivityDetectorFlush(Handle);
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~VoiceActivityDetector()
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

        private static IntPtr SherpaOnnxCreateVoiceActivityDetector(ref VadModelConfig config, float bufferSizeInSeconds)
        {
            VadModelConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateVoiceActivityDetector(ref configCopy, bufferSizeInSeconds), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateVoiceActivityDetector(ref config, bufferSizeInSeconds);
        }

        private static void SherpaOnnxDestroyVoiceActivityDetector(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyVoiceActivityDetector(handle),
                () => NativeExternal.SherpaOnnxDestroyVoiceActivityDetector(handle));
        }

        private static void SherpaOnnxVoiceActivityDetectorAcceptWaveform(IntPtr handle, float[] samples, int n)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorAcceptWaveform(handle, samples, n),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorAcceptWaveform(handle, samples, n));
        }

        private static int SherpaOnnxVoiceActivityDetectorEmpty(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorEmpty(handle),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorEmpty(handle));
        }

        private static int SherpaOnnxVoiceActivityDetectorDetected(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorDetected(handle),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorDetected(handle));
        }

        private static void SherpaOnnxVoiceActivityDetectorPop(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorPop(handle),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorPop(handle));
        }

        private static void SherpaOnnxVoiceActivityDetectorClear(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorClear(handle),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorClear(handle));
        }

        private static IntPtr SherpaOnnxVoiceActivityDetectorFront(IntPtr handle)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorFront(handle),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorFront(handle));
        }

        private static void SherpaOnnxDestroySpeechSegment(IntPtr segment)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroySpeechSegment(segment),
                () => NativeExternal.SherpaOnnxDestroySpeechSegment(segment));
        }

        private static void SherpaOnnxVoiceActivityDetectorReset(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorReset(handle),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorReset(handle));
        }

        private static void SherpaOnnxVoiceActivityDetectorFlush(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxVoiceActivityDetectorFlush(handle),
                () => NativeExternal.SherpaOnnxVoiceActivityDetectorFlush(handle));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateVoiceActivityDetector(ref VadModelConfig config, float bufferSizeInSeconds);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyVoiceActivityDetector(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorAcceptWaveform(IntPtr handle, float[] samples, int n);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxVoiceActivityDetectorEmpty(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern int SherpaOnnxVoiceActivityDetectorDetected(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorPop(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorClear(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxVoiceActivityDetectorFront(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroySpeechSegment(IntPtr segment);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorReset(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorFlush(IntPtr handle);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateVoiceActivityDetector(ref VadModelConfig config, float bufferSizeInSeconds);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyVoiceActivityDetector(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorAcceptWaveform(IntPtr handle, float[] samples, int n);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxVoiceActivityDetectorEmpty(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern int SherpaOnnxVoiceActivityDetectorDetected(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorPop(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorClear(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxVoiceActivityDetectorFront(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroySpeechSegment(IntPtr segment);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorReset(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxVoiceActivityDetectorFlush(IntPtr handle);
        }

        #endregion
    }
}
