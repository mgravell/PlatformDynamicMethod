using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace PlatformDynamicMethod
{
    // THIS IS ALL NONSENSE - I fucked up and took a ref we can't use; it is garbage
    public static class DynamicMethodFactory
    {
        private static readonly Type _type;
        internal static readonly Func<object, ILGenerator> GetILGenerator;
        internal static readonly Func<object, Type, Delegate> CreateDelegate;
        internal static readonly Func<object, Type, object, Delegate> CreateDelegateWithTarget;
        static DynamicMethodFactory()
        {
            try
            {
                Init(out _type, out GetILGenerator, out CreateDelegate, out CreateDelegateWithTarget);
            }
            catch { }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Init(out Type type,
            out Func<object, ILGenerator> getILGenerator,
            out Func<object, Type, Delegate> createDelegate,
            out Func<object, Type, object, Delegate> createDelegateWithTarget)
        {
            type = null;
            getILGenerator = null;
            createDelegate = null;
            createDelegateWithTarget = null;
 
            type = typeof(ILGenerator).Assembly.GetType("System.Reflection.Emit.DynamicMethod");

            var untyped = Expression.Parameter(typeof(object));
            var typed = Expression.Convert(untyped, _type);
            getILGenerator = Expression.Lambda<Func<object, ILGenerator>>(Expression.Call(typed, "GetILGenerator", Type.EmptyTypes), untyped).Compile();

            var delegateType = Expression.Parameter(typeof(Type));
            var target = Expression.Parameter(typeof(object));
            createDelegate = Expression.Lambda<Func<object, Type, Delegate>>(Expression.Call(typed, "CreateDelegate", Type.EmptyTypes, delegateType), untyped, delegateType).Compile();

            createDelegateWithTarget = Expression.Lambda<Func<object, Type, object, Delegate>>(Expression.Call(typed, "CreateDelegate", Type.EmptyTypes, delegateType, target), untyped, delegateType, target).Compile();
        }
        private static Type DynamicMethod => _type ?? throw new PlatformNotSupportedException();
        public static bool IsSupported => _type != null;
        public static DynamicMethodFactory<T> Create<T>(string name) where T : class, Delegate
        {
            var returnType = Validate<T>(out var parameterTypes);
            var obj = GetCtor(typeof(string), typeof(Type), typeof(Type[])).Invoke(
                new object[] { name, returnType, parameterTypes });
            return new DynamicMethodFactory<T>(obj);
        }
        public static DynamicMethodFactory<T> Create<T>(string name,
    MethodAttributes attributes,
    CallingConventions callingConvention,
    Module m,
    bool skipVisibility) where T : class, Delegate
        {
            var returnType = Validate<T>(out var parameterTypes);

            var obj = GetCtor(typeof(string), typeof(MethodAttributes), typeof(CallingConventions),
                typeof(Type), typeof(Type[]), typeof(Module), typeof(bool)).Invoke(new object[] {name, attributes, callingConvention,
        returnType, parameterTypes, m, skipVisibility });
            return new DynamicMethodFactory<T>(obj);
        }

        // this is illustrative only and should probably be improved
        static ConstructorInfo GetCtor(params Type[] types) => DynamicMethod.GetConstructor(types);


        static Type Validate<T>(out Type[] args) where T : class, Delegate
        {
            var method = typeof(T).GetMethod("Invoke");
            var parameters = method.GetParameters();
            args = (parameters == null || parameters.Length == 0) ? Type.EmptyTypes
                : Array.ConvertAll(parameters, p => p.ParameterType);
            return method.ReturnType;
        }
    }


    public sealed class DynamicMethodFactory<T> where T : class, Delegate
    {
        private readonly object _dynamicMethod;
        internal DynamicMethodFactory(object dynamicMethod)
            => _dynamicMethod = dynamicMethod;

        public ILGenerator GetILGenerator() => DynamicMethodFactory.GetILGenerator(_dynamicMethod);

        public T CreateDelegate() => (T)DynamicMethodFactory.CreateDelegate(_dynamicMethod, typeof(T));
        public T CreateDelegate(object target) => (T)DynamicMethodFactory.CreateDelegateWithTarget(_dynamicMethod, typeof(T), target);
    }


}
