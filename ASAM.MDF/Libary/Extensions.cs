using System;
using System.Collections.Generic;

namespace ASAM.MDF.Libary
{
    internal static class Extensions
    {
        /// <summary>
        /// Copy address data to byte massive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="block">Block coping</param>
        /// <param name="pointerData">pointer to change</param>
        /// <param name="bytes">byte massive for copy</param>
        internal static void CopyAddress<T>(this T block, PointerAddress<uint> pointerData, List<byte> bytes, int index, uint count) where T : Block
        {
            var newbytes = BitConverter.GetBytes(pointerData.address);

            for (int i = block.BlockAddress + pointerData.offset, j = 0; j < newbytes.Length; i++, j++)
                bytes[i] = newbytes[j];
            //Array.Copy(newbytes, 0, bytes, block.BlockAddress + pointerData.offset, newbytes.Length);

        }
        internal static void CopyAddress<T>(this T block, PointerAddress<ulong> pointerData, List<byte> bytes, int index, ulong count) where T : Block
        {
            var newBlockAddress = block.BlockAddress;
            if (newBlockAddress > index)
                newBlockAddress -= (int)count;

            var newbytes = BitConverter.GetBytes(pointerData.address);

            for (int i = newBlockAddress + pointerData.offset, j = 0; j < newbytes.Length; i++, j++)
                bytes[i] = newbytes[j];
            //Array.Copy(newbytes, 0, bytes, block.BlockAddress + pointerData.offset, newbytes.Length);
        }
    }
}
