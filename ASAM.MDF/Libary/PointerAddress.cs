namespace ASAM.MDF.Libary
{
    internal class PointerAddress<T> where T : struct
    {
        internal readonly int offset;
        internal T address;

        internal PointerAddress(T ptr, int offset)
        {
            address = ptr;
            this.offset = offset;
        }
    }
}
