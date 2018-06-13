using PlatformDynamicMethod;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ConsoleApp3
{
    class Program
    {
        // THIS IS ALL NONSENSE - I fucked up and took a ref we can't use; it is garbage
        static void Main(string[] args)
        {

            ClassLibrary1.Class1.FromLib();

            var type = ClassLibrary1.Class1.CreateType();
            var obj = Activator.CreateInstance(type);
            Console.WriteLine(obj);

            var factory = DynamicMethodFactory.Create<Func<string>>("MyCode");

            
            var il = factory.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "from app");
            il.Emit(OpCodes.Ret);

            var method = factory.CreateDelegate();

            Console.WriteLine(method());
        }
    }
}
