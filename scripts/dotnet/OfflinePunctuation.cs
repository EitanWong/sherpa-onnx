/// Copyright (c)  2024  Xiaomi Corporation (authors: Fangjun Kuang)
using System;
using System.Runtime.InteropServices;
using System.Text;


namespace SherpaOnnx
{
    public class OfflinePunctuation : IDisposable
    {
        public OfflinePunctuation(OfflinePunctuationConfig config)
        {
            IntPtr pointer = SherpaOnnxCreateOfflinePunctuation(ref config);
            if (pointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SherpaOnnxCreateOfflinePunctuation returned a null handle.");
            }

            _handle = NativeResourceHandle.Create(pointer, SherpaOnnxDestroyOfflinePunctuation);
        }

        public String AddPunct(String text)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            byte[] utf8BytesWithNull = new byte[utf8Bytes.Length + 1]; // +1 for null terminator
            Array.Copy(utf8Bytes, utf8BytesWithNull, utf8Bytes.Length);
            utf8BytesWithNull[utf8Bytes.Length] = 0; // Null terminator

            IntPtr p = SherpaOfflinePunctuationAddPunct(Handle, utf8BytesWithNull);

            string s = "";
            int length = 0;

            unsafe
            {
                byte* b = (byte*)p;
                if (b != null)
                {
                    while (*b != 0)
                    {
                        ++b;
                        length += 1;
                    }
                }
            }

            if (length > 0)
            {
                byte[] stringBuffer = new byte[length];
                Marshal.Copy(p, stringBuffer, 0, length);
                s = Encoding.UTF8.GetString(stringBuffer);
            }

            SherpaOfflinePunctuationFreeText(p);

            return s;
        }

        public void Dispose()
        {
            Cleanup();
            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        ~OfflinePunctuation()
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

        private static IntPtr SherpaOnnxCreateOfflinePunctuation(ref OfflinePunctuationConfig config)
        {
            OfflinePunctuationConfig configCopy = config;
            if (Dll.TryInvokeInternal(() => NativeInternal.SherpaOnnxCreateOfflinePunctuation(ref configCopy), out var result))
            {
                return result;
            }
            return NativeExternal.SherpaOnnxCreateOfflinePunctuation(ref config);
        }

        private static void SherpaOnnxDestroyOfflinePunctuation(IntPtr handle)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOnnxDestroyOfflinePunctuation(handle),
                () => NativeExternal.SherpaOnnxDestroyOfflinePunctuation(handle));
        }

        private static IntPtr SherpaOfflinePunctuationAddPunct(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Text)
        {
            return Dll.Invoke(
                () => NativeInternal.SherpaOfflinePunctuationAddPunct(handle, utf8Text),
                () => NativeExternal.SherpaOfflinePunctuationAddPunct(handle, utf8Text));
        }

        private static void SherpaOfflinePunctuationFreeText(IntPtr p)
        {
            Dll.Invoke(
                () => NativeInternal.SherpaOfflinePunctuationFreeText(p),
                () => NativeExternal.SherpaOfflinePunctuationFreeText(p));
        }

        private static class NativeExternal
        {
            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOnnxCreateOfflinePunctuation(ref OfflinePunctuationConfig config);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOnnxDestroyOfflinePunctuation(IntPtr handle);

            [DllImport(Dll.Filename)]
            internal static extern IntPtr SherpaOfflinePunctuationAddPunct(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Text);

            [DllImport(Dll.Filename)]
            internal static extern void SherpaOfflinePunctuationFreeText(IntPtr p);
        }

        private static class NativeInternal
        {
            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOnnxCreateOfflinePunctuation(ref OfflinePunctuationConfig config);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOnnxDestroyOfflinePunctuation(IntPtr handle);

            [DllImport(Dll.InternalFilename)]
            internal static extern IntPtr SherpaOfflinePunctuationAddPunct(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)] byte[] utf8Text);

            [DllImport(Dll.InternalFilename)]
            internal static extern void SherpaOfflinePunctuationFreeText(IntPtr p);
        }

        #endregion
    }
}
