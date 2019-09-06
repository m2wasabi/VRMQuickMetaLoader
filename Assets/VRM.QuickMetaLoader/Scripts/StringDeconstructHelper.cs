using System; 
using Unity.Collections; 
using Unity.Collections.LowLevel.Unsafe; 
 
namespace VRM.QuickMetaLoader.Model 
{ 
    internal static class StringDeconstructHelper 
    { 
        public static unsafe void Deconstruct(this string text, out IntPtr charPtr, out int charCount) 
        { 
            if (string.IsNullOrEmpty(text)) 
            { 
                charCount = default; 
                charPtr = default; 
            } 
            var ptr = new IntPtr(UnsafeUtility.Malloc(2 * text.Length, 2, Allocator.Persistent)); 
            fixed (char* src = text) 
            { 
                UnsafeUtility.MemCpy(ptr.ToPointer(), src, 2 * text.Length); 
            } 
            charCount = text.Length; 
            charPtr = ptr; 
        } 
    } 
}