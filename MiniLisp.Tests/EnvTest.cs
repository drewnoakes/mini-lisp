using System.IO;
using System.Linq;

using Xunit;

namespace MiniLisp.Tests
{
    public class EnvTest
    {
        [Fact]
        public void Evaluate()
        {
            var eval = new Env
            {
                // arithmetic
                { "add", new VariadicFunction<int, int>(args => args.Sum()) },

                // string
                { "add", new VariadicFunction<string, string>(args => args.Aggregate("", string.Concat)) }, // overload by type

                // inequalities
                { "lt",  new Function<int, int, bool>((a, b) => a <  b) },
                { "lte", new Function<int, int, bool>((a, b) => a <= b) },
                { "gt",  new Function<int, int, bool>((a, b) => a >  b) },
                { "gte", new Function<int, int, bool>((a, b) => a >= b) },

                // equality
                { "eq",  new Function<object, object, bool>(Equals) },
                { "ne",  new Function<object, object, bool>((a, b) => !Equals(a, b)) },

                // logic
                { "and", new VariadicFunction<bool, bool>(args => args.All(arg => arg)) },
                { "or",  new VariadicFunction<bool, bool>(args => args.Any(arg => arg)) },
                { "xor", new Function<bool, bool, bool>((a, b) => a ^  b) },
                { "not", new Function<bool, bool>(a => !a) },

                // data model
                { "eval",         new Function<string, string, object>((page, name) => "Evaluated value") },
                { "uneval",       new Function<string, string, object>((page, name) => "Unevaluated value") },
                { "display-name", new Function<string, string, object>((page, name) => "Display name") }
            };

            Assert.Equal(3, eval.Evaluate("(add 1 2)"));
            Assert.Equal(3, eval.Evaluate("(add 1 (add 1 1))"));
            Assert.Equal("ABC", eval.Evaluate(@"(add ""A"" ""B"" ""C"")"));
            Assert.Equal(true, eval.Evaluate("(and true true true)"));
            Assert.Equal(false, eval.Evaluate("(and true true false)"));
            Assert.Equal(true, eval.Evaluate("(or false false true)"));
            Assert.Equal(false, eval.Evaluate("(or false false false)"));
            Assert.Equal(true, eval.Evaluate("(lt 1 2)"));
            Assert.Equal(true, eval.Evaluate("(lte 1 2)"));
            Assert.Equal(true, eval.Evaluate("(and (lte 1 2) (gt 5 (add 2 2)))"));
            Assert.Equal(true, eval.Evaluate("true"));
            Assert.Equal(false, eval.Evaluate("false"));
            Assert.Equal(1234, eval.Evaluate("1234"));
            Assert.Null(eval.Evaluate("null"));
            Assert.Equal(true, eval.Evaluate(" true"));
            Assert.Equal(123, eval.Evaluate(" 123"));
            Assert.Equal(123, eval.Evaluate(" 123 "));
            Assert.Equal(123, eval.Evaluate(" 123   "));
            Assert.Equal("Foo", eval.Evaluate(@"""Foo"""));
            Assert.Equal("Foo\"Bar", eval.Evaluate(@"""Foo\""Bar"""));
            Assert.Equal(true, eval.Evaluate(@"(eq ""Evaluated value"" (eval ""page"" ""name""))"));
            Assert.Equal(false, eval.Evaluate(@"(eq (uneval ""page"" ""name"") (eval ""page"" ""name""))"));

            Assert.Throws<EndOfStreamException>(() => eval.Evaluate("(and"));
            Assert.Throws<EvaluationException>(() => eval.Evaluate("(and true 1)"));
            Assert.Throws<EvaluationException>(() => eval.Evaluate("unknown"));
            Assert.Throws<EvaluationException>(() => eval.Evaluate("(unknown)"));
        }
    }
}
