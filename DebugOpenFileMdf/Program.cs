using ASAM.MDF.Libary;
using System.Runtime.InteropServices;
using System.Threading.Channels;

var filename = ShowDialog();
try
{
    var bytes = File.ReadAllBytes(filename);
    var mdf = new Mdf(bytes);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine();
    throw;
}

static string ShowDialog()
{
    var ofn = new OpenFileName();
    ofn.lStructSize = Marshal.SizeOf(ofn);
    ofn.lpstrFile = new string(new char[256]);
    ofn.nMaxFile = ofn.lpstrFile.Length;
    ofn.lpstrFileTitle = new string(new char[64]);
    ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
    ofn.lpstrTitle = "Open File Dialog...";
    if (GetOpenFileName(ref ofn))
        return ofn.lpstrFile;
    return string.Empty;
}

[DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
static extern bool GetOpenFileName(ref OpenFileName ofn);

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct OpenFileName
{
    public int lStructSize;
    public IntPtr hwndOwner;
    public IntPtr hInstance;
    public string lpstrFilter;
    public string lpstrCustomFilter;
    public int nMaxCustFilter;
    public int nFilterIndex;
    public string lpstrFile;
    public int nMaxFile;
    public string lpstrFileTitle;
    public int nMaxFileTitle;
    public string lpstrInitialDir;
    public string lpstrTitle;
    public int Flags;
    public short nFileOffset;
    public short nFileExtension;
    public string lpstrDefExt;
    public IntPtr lCustData;
    public IntPtr lpfnHook;
    public string lpTemplateName;
    public IntPtr pvReserved;
    public int dwReserved;
    public int flagsEx;
}