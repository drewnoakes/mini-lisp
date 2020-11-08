using System;

namespace MiniLisp
{
    public sealed class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message)
        {
        }
    }
}