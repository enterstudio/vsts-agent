namespace Microsoft.VisualStudio.Services.DistributedTask.Expressions
{
    internal sealed class Token
    {
        public Token(TokenKind kind, int index, int length = 1, object parsedValue = null)
        {
            Kind = kind;
            Index = index;
            Length = length;
            ParsedValue = parsedValue;
        }

        public TokenKind Kind { get; }

        public int Index { get; }

        public int Length { get; }

        public object ParsedValue { get; }
    }

    internal enum TokenKind
    {
        // Punctuation
        StartIndex,
        StartParameter,
        EndIndex,
        EndParameter,
        Separator,
        Dereference,

        // Values
        Boolean,
        Number,
        Version,
        String,
        PropertyName,

        // Functions
        And,
        Contains,
        EndsWith,
        Equal,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        In,
        Not,
        NotEqual,
        Or,
        Xor,
        Extension,

        Unrecognized,
    }
}
