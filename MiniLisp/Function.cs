using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniLisp
{
    public abstract class Function
    {
        public abstract int Arity { get; }
        public abstract bool TryInvoke(object?[] list, out object? result);
    }

    public sealed class Function<T1, TResult> : Function
    {
        private readonly Func<T1, TResult> _func;
        public Function(Func<T1, TResult> func) => _func = func;
        public override int Arity => 1;

        public override bool TryInvoke(object?[] args, out object? result)
        {
            if (args[0] is T1 t1)
            {
                result = _func(t1);
                return true;
            }

            result = default;
            return false;
        }
    }

    public sealed class VariadicFunction : Function
    {
        private readonly Func<object?[], object> _func;
        public VariadicFunction(Func<object?[], object> func) => _func = func;
        public override int Arity => -1;

        public override bool TryInvoke(object?[] args, out object? result)
        {
            result = _func(args);
            return true;
        }
    }

    public sealed class VariadicFunction<TArg, TResult> : Function
    {
        private readonly Func<IReadOnlyList<TArg>, TResult> _func;
        public VariadicFunction(Func<IReadOnlyList<TArg>, TResult> func) => _func = func;
        public override int Arity => -1;

        public override bool TryInvoke(object?[] args, out object? result)
        {
            var typedArgs = args.OfType<TArg>().ToList();

            if (typedArgs.Count == args.Length)
            {
                result = _func(typedArgs);
                return true;
            }

            result = default;
            return false;
        }
    }

    public sealed class Function<T1, T2, TResult> : Function
    {
        private readonly Func<T1, T2, TResult> _func;
        public Function(Func<T1, T2, TResult> func) => _func = func;
        public override int Arity => 2;

        public override bool TryInvoke(object?[] args, out object? result)
        {
            if (args[0] is T1 t1 && args[1] is T2 t2)
            {
                result = _func(t1, t2);
                return true;
            }

            result = default;
            return false;
        }
    }
}