using System;
using System.Runtime.InteropServices;

namespace SherpaOnnx
{
    internal sealed class NativeResourceHandle : SafeHandle
    {
        internal delegate void ReleaseHandleCallback(IntPtr pointer);

        private readonly ReleaseHandleCallback _release;

        private NativeResourceHandle(ReleaseHandleCallback release)
            : base(IntPtr.Zero, ownsHandle: true)
        {
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        internal static NativeResourceHandle Create(IntPtr pointer, ReleaseHandleCallback release)
        {
            var handle = new NativeResourceHandle(release);
            if (pointer == IntPtr.Zero)
            {
                return handle;
            }

            handle.SetHandle(pointer);
            return handle;
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                try
                {
                    _release(handle);
                }
                catch
                {
                    // Suppress cleanup exceptions to honor SafeHandle contract.
                }
            }

            return true;
        }
    }
}
