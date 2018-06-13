using PlatformDynamicMethod;
using System;
using System.Reflection;
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

        public static Type CreateType()
        {
            AssemblyBuilder asm = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("yourasm"), AssemblyBuilderAccess.Run);
            var m = asm.DefineDynamicModule("yourasm");
            var t = m.DefineType("DynamicType", TypeAttributes.Class | TypeAttributes.Sealed);
            t.DefineDefaultConstructor(MethodAttributes.Public);
            return t.CreateTypeInfo().AsType();
        }
    }
}
