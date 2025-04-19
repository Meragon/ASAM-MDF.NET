namespace ASAM.MDF.Libary
{
    internal interface INext<T> where T : INext<T>
    {
        T Next { get; }
    }
    internal interface IPrevious<T> where T : IPrevious<T>
    {
        T Previous { get; set; }
    }
    internal interface IParent<T>
    {
        T Parent { get; set; }
    }
}