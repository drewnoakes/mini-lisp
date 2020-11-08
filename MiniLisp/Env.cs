using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MiniLisp
{
    public sealed class Env : IEnumerable
    {
        private readonly Dictionary<string, List<Function>> _functionsBySymbol = new Dictionary<string, List<Function>>();

        public void Add(string symbol, Function function)
        {
            if (!_functionsBySymbol.TryGetValue(symbol, out var functions))
                _functionsBySymbol[symbol] = functions = new List<Function>(1);
            functions.Add(function);
        }

        public object? Evaluate(string expression)
        {
            var i = 0;

            object root;
            try
            {
                root = Parse();
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new EndOfStreamException("Data ended unexpectedly.", ex);
            }

            return Eval(root);

            object Parse()
            {
                do
                {
                    char c = expression[i];

                    switch (c)
                    {
                        case '"': i++; return ParseString();
                        case '(': i++; return ParseList();
                        case ' ':
                        case '\r':
                        case '\n':
                        case '\t': break;
                        default: return ParseSymbol();
                    }

                    i++;
                } while (true);

                StringRef ParseString()
                {
                    var start = i;
                    var isEscaping = false;

                    while (true)
                    {
                        char c = expression[i++];

                        switch (c, isEscaping)
                        {
                            case (_, true): isEscaping = false; break;
                            case ('\\', false): isEscaping = true; break;
                            case ('"', false): return new StringRef(start, i - start - 1);
                        }
                    }
                }

                object[] ParseList()
                {
                    var items = new List<object>(2);

                    while (true)
                    {
                        char c = expression[i];

                        switch (c)
                        {
                            case ')': i++; return items.ToArray(); // TODO pool these arrays
                            case ' ': i++; continue;
                            case '\t': i++; continue;
                            default: items.Add(Parse()); break;
                        }
                    }
                }

                SymbolRef ParseSymbol()
                {
                    var start = i;

                    while (i < expression.Length)
                    {
                        char c = expression[i];

                        switch (c)
                        {
                            case ' ': return new SymbolRef(start, i++ - start);
                            case ')': return new SymbolRef(start, i - start);
                            case '(': return new SymbolRef(start, i - start);
                            default: i++; continue;
                        }
                    }

                    return new SymbolRef(start, i - start);
                }
            }

            object? Eval(object node)
            {
                if (node is object[] list)
                {
                    if (list.Length == 0) throw new EvaluationException("Cannot evaluate an empty list");
                    if (!(list[0] is SymbolRef symbol)) throw new EvaluationException("First item in list must be a symbol");
                    if (!_functionsBySymbol.TryGetValue(symbol.ToString(expression), out var functions)) throw new EvaluationException($"Unknown function '{symbol}'");

                    object?[] args;

                    if (list.Length > 1)
                    {
                        args = new object?[list.Length - 1];

                        for (int i = 1; i < list.Length; i++)
                        {
                            args[i - 1] = Eval(list[i]);
                        }
                    }
                    else
                    {
                        args = Array.Empty<object>();
                    }

                    foreach (var function in functions)
                    {
                        if (function.Arity >= 0 && function.Arity != list.Length - 1) continue;
                        if (function.TryInvoke(args, out var result)) return result;
                    }

                    throw new EvaluationException("Failed to invoke function");
                }

                if (node is StringRef stringRef)
                {
                    return stringRef.ToString(expression);
                }

                if (node is SymbolRef symbolRef)
                {
                    if (symbolRef.Length == 0) throw new EvaluationException("Cannot evaluate a zero length symbol");

                    ReadOnlySpan<char> span = symbolRef.AsSpan(expression);

                    if (span.Equals("true",  StringComparison.OrdinalIgnoreCase)) return Boxed.True;
                    if (span.Equals("false", StringComparison.OrdinalIgnoreCase)) return Boxed.False;
                    if (span.Equals("null",  StringComparison.OrdinalIgnoreCase)) return null;

                    if (char.IsDigit(span[0]))
                        return int.Parse(span);

                    throw new EvaluationException($"Unexpected symbol '{span.ToString()}'");
                }

                throw new EvaluationException("Should be unreachable");
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

        private sealed class StringRef
        {
            public StringRef(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public int Start { get; }
            public int Length { get; }

            public string ToString(string s) => s.Substring(Start, Length).Replace("\\\"", "\"");
        }

        private sealed class SymbolRef
        {
            public SymbolRef(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public int Start { get; }
            public int Length { get; }

            public string ToString(string s) => s.Substring(Start, Length);

            public ReadOnlySpan<char> AsSpan(string s) => s.AsSpan(Start, Length);
        }
    }
}
