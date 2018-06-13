using PlatformDynamicMethod;
using System;
using System.Reflection.Emit;

namespace ClassLibrary1
{
    public static class Class1
    {
        public static void FromLib()
        {
            var factory = DynamicMethodFactory.Create<Func<string>>("MyCode");


            var il = factory.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "from library");
            il.Emit(OpCodes.Ret);

            var method = factory.CreateDelegate();

            Console.WriteLine(method());
        }
    }
}
