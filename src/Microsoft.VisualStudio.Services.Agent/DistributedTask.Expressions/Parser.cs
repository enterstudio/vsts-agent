using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.Services.DistributedTask.Expressions
{
    public sealed class Parser
    {
        public Node CreateTree(string expression, ITraceWriter trace, IEnumerable<IExtensionInfo> extensions)
        {
            var context = new ParseContext(expression, trace, extensions);
            context.Trace.Verbose($"Parsing [{expression}]");
            while (TryGetNextToken(context))
            {
                switch (context.Token.Kind)
                {
                    // Punctuation
                    case TokenKind.StartIndex:
                        HandleStartIndex(context);
                        break;
                    case TokenKind.EndIndex:
                        HandleEndIndex(context);
                        break;
                    case TokenKind.EndParameter:
                        HandleEndParameter(context);
                        break;
                    case TokenKind.Separator:
                        HandleSeparator(context);
                        break;
                    case TokenKind.Dereference:
                        HandleDereference(context);
                        break;

                    // Functions
                    case TokenKind.And:
                    case TokenKind.Contains:
                    case TokenKind.EndsWith:
                    case TokenKind.Equal:
                    case TokenKind.GreaterThan:
                    case TokenKind.GreaterThanOrEqual:
                    case TokenKind.LessThan:
                    case TokenKind.LessThanOrEqual:
                    case TokenKind.In:
                    case TokenKind.Not:
                    case TokenKind.NotEqual:
                    case TokenKind.NotIn:
                    case TokenKind.Or:
                    case TokenKind.Xor:
                    case TokenKind.Extension:
                        HandleFunction(context);
                        break;

                    // Leaf values
                    case TokenKind.Boolean:
                    case TokenKind.Number:
                    case TokenKind.Version:
                    case TokenKind.String:
                        HandleValue(context);
                        break;

                    // Malformed
                    case TokenKind.Unrecognized:
                        throw new ParseException(ParseExceptionKind.UnrecognizedValue, context.Token, context.Raw);

                    // Unexpected
                    case TokenKind.PropertyName:    // PropertyName should never reach here.
                    case TokenKind.StartParameter:  // StartParameter is only expected by HandleFunction.
                    default:
                        throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
                }
            }

            // Validate all containers were closed.
            if (context.Containers.Count > 0)
            {
                ContainerInfo container = context.Containers.Peek();
                if (container.Node is FunctionNode)
                {
                    throw new ParseException(ParseExceptionKind.UnclosedFunction, container.Token, context.Raw);
                }
                else
                {
                    throw new ParseException(ParseExceptionKind.UnclosedIndexer, container.Token, context.Raw);
                }
            }

            return context.Root;
        }

        private static bool TryGetNextToken(ParseContext context)
        {
            context.LastToken = context.Token;
            if (context.Lexer.TryGetNextToken(ref context.Token))
            {
                int indentLevel = context.Containers.Count;
                if (indentLevel > 0)
                {
                    switch (context.Token.Kind)
                    {
                        case TokenKind.StartParameter:
                        case TokenKind.EndParameter:
                        case TokenKind.EndIndex:
                            indentLevel--;
                            break;
                    }
                }

                string indent = string.Empty.PadRight(indentLevel * 2, '.');
                switch (context.Token.Kind)
                {
                    // Literal values
                    case TokenKind.Boolean:
                    case TokenKind.Number:
                    case TokenKind.Version:
                    case TokenKind.String:
                        context.Trace.Verbose($"{indent}{context.Token.Kind} '{context.Token.ParsedValue}'");
                        break;
                    // Named or unrecognized
                    case TokenKind.Extension:
                    case TokenKind.PropertyName:
                    case TokenKind.Unrecognized:
                        context.Trace.Verbose($"{indent}{context.Token.Kind} '{context.Raw.Substring(context.Token.Index, context.Token.Length)}'");
                        break;
                    // Punctuation
                    case TokenKind.StartIndex:
                    case TokenKind.StartParameter:
                    case TokenKind.EndIndex:
                    case TokenKind.EndParameter:
                    case TokenKind.Separator:
                    case TokenKind.Dereference:
                        context.Trace.Verbose($"{indent}{context.Raw.Substring(context.Token.Index, 1)}");
                        break;
                    // Functions
                    default:
                        context.Trace.Verbose($"{indent}{context.Token.Kind}");
                        break;
                }

                return true;
            }

            return false;
        }

        private static void HandleStartIndex(ParseContext context)
        {
            // Validate follows ")", "]", or a property name.
            if (context.LastToken == null ||
                (context.LastToken.Kind != TokenKind.EndParameter && context.LastToken.Kind != TokenKind.EndIndex && context.LastToken.Kind != TokenKind.PropertyName))
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }

            // Wrap the object being indexed into.
            var indexer = new IndexerNode();
            Node obj = null;
            if (context.Containers.Count > 0)
            {
                ContainerNode container = context.Containers.Peek().Node;
                int objIndex = container.Parameters.Count - 1;
                obj = container.Parameters[objIndex];
                container.ReplaceParameter(objIndex, indexer);
            }
            else
            {
                obj = context.Root;
                context.Root = indexer;
            }

            indexer.AddParameter(obj);

            // Update the container stack.
            context.Containers.Push(new ContainerInfo() { Node = indexer, Token = context.Token });
        }

        private static void HandleDereference(ParseContext context)
        {
            // Validate follows ")", "]", or a property name.
            if (context.LastToken == null ||
                (context.LastToken.Kind != TokenKind.EndParameter && context.LastToken.Kind != TokenKind.EndIndex && context.LastToken.Kind != TokenKind.PropertyName))
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }

            // Wrap the object being indexed into.
            var indexer = new IndexerNode();
            Node obj = null;
            if (context.Containers.Count > 0)
            {
                ContainerNode container = context.Containers.Peek().Node;
                int objIndex = container.Parameters.Count - 1;
                obj = container.Parameters[objIndex];
                container.ReplaceParameter(objIndex, indexer);
            }
            else
            {
                obj = context.Root;
                context.Root = indexer;
            }

            indexer.AddParameter(obj);

            // Validate a property name follows.
            if (!TryGetNextToken(context))
            {
                throw new ParseException(ParseExceptionKind.ExpectedPropertyName, context.LastToken, context.Raw);
            }

            if (context.Token.Kind != TokenKind.PropertyName)
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }

            // Add the property name to the indexer, as a string.
            string propertyName = context.Raw.Substring(context.Token.Index, context.Token.Length);
            indexer.AddParameter(new LeafNode(propertyName));
        }

        private static void HandleEndParameter(ParseContext context)
        {
            ContainerInfo container = context.Containers.Count > 0 ? context.Containers.Peek() : null;  // Validate:
            if (container == null ||                                                        // 1) Container is not null
                !(container.Node is FunctionNode) ||                                        // 2) Container is a function
                container.Node.Parameters.Count < GetMinParamCount(context, container.Token) || // 3) Not below min param threshold
                context.LastToken.Kind == TokenKind.Separator)                              // 4) Last token is not a separator
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }

            context.Containers.Pop();
        }

        private static void HandleEndIndex(ParseContext context)
        {
            IndexerNode indexer = context.Containers.Count > 0 ? context.Containers.Peek().Node as IndexerNode : null;
            //                                  // Validate:
            if (indexer == null ||              // 1) Container is an indexer
                indexer.Parameters.Count != 2)  // 2) Exactly 2 parameters
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }

            context.Containers.Pop();
        }

        private static void HandleValue(ParseContext context)
        {
            // Validate either A) is the first token OR B) follows "[" "(" or ",".
            if (context.LastToken != null &&
                context.LastToken.Kind != TokenKind.StartIndex &&
                context.LastToken.Kind != TokenKind.StartParameter &&
                context.LastToken.Kind != TokenKind.Separator)
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }

            // Update the tree.
            var node = new LeafNode(context.Token.ParsedValue);
            if (context.Root == null)
            {
                context.Root = node;
            }
            else
            {
                context.Containers.Peek().Node.AddParameter(node);
            }
        }

        private static void HandleSeparator(ParseContext context)
        {
            ContainerInfo container = context.Containers.Count > 0 ? context.Containers.Peek() : null;  // Validate:
            if (container == null ||                                                            // 1) Container is not null
                !(container.Node is FunctionNode) ||                                            // 2) Container is a function
                container.Node.Parameters.Count < 1 ||                                          // 3) At least one parameter
                container.Node.Parameters.Count >= GetMaxParamCount(context, container.Token) ||// 4) Under max parameters threshold
                context.LastToken.Kind == TokenKind.Separator)                                  // 5) Last token is not a separator
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }
        }

        private static void HandleFunction(ParseContext context)
        {
            // Validate either A) is first token OR B) follows "," or "[" or "(".
            if (context.LastToken != null &&
                (context.LastToken.Kind != TokenKind.Separator &&
                context.LastToken.Kind != TokenKind.StartIndex &&
                context.LastToken.Kind != TokenKind.StartParameter))
            {
                throw new ParseException(ParseExceptionKind.UnexpectedSymbol, context.Token, context.Raw);
            }

            // Create the node.
            FunctionNode node;
            string name = context.Raw.Substring(context.Token.Index, context.Token.Length);
            switch (context.Token.Kind)
            {
                case TokenKind.And:
                    node = new AndNode();
                    break;
                case TokenKind.Contains:
                    node = new ContainsNode();
                    break;
                case TokenKind.EndsWith:
                    node = new EndsWithNode();
                    break;
                case TokenKind.Equal:
                    node = new EqualNode();
                    break;
                case TokenKind.GreaterThan:
                    node = new GreaterThanNode();
                    break;
                case TokenKind.GreaterThanOrEqual:
                    node = new GreaterThanOrEqualNode();
                    break;
                case TokenKind.LessThan:
                    node = new LessThanNode();
                    break;
                case TokenKind.LessThanOrEqual:
                    node = new LessThanOrEqualNode();
                    break;
                case TokenKind.In:
                    node = new InNode();
                    break;
                case TokenKind.Not:
                    node = new NotNode();
                    break;
                case TokenKind.NotEqual:
                    node = new NotEqualNode();
                    break;
                case TokenKind.NotIn:
                    node = new NotInNode();
                    break;
                case TokenKind.Or:
                    node = new OrNode();
                    break;
                case TokenKind.Xor:
                    node = new XorNode();
                    break;
                case TokenKind.Extension:
                    node = context.Extensions[name].CreateNode();
                    break;
                default:
                    // Should never reach here.
                    throw new NotSupportedException($"Unexpected function token name: '{context.Token.Kind}'");
            }

            node.Name = name;

            // Update the tree.
            if (context.Root == null)
            {
                context.Root = node;
            }
            else
            {
                context.Containers.Peek().Node.AddParameter(node);
            }

            // Update the container stack.
            context.Containers.Push(new ContainerInfo() { Node = node, Token = context.Token });

            // Validate '(' follows.
            if (!TryGetNextToken(context) || context.Token.Kind != TokenKind.StartParameter)
            {
                throw new ParseException(ParseExceptionKind.ExpectedStartParameter, context.LastToken, context.Raw);
            }
        }

        private static int GetMinParamCount(ParseContext context, Token token)
        {
            switch (token.Kind)
            {
                case TokenKind.Not:
                    return 1;
                case TokenKind.And:
                case TokenKind.Contains:
                case TokenKind.EndsWith:
                case TokenKind.Equal:
                case TokenKind.GreaterThan:
                case TokenKind.GreaterThanOrEqual:
                case TokenKind.LessThan:
                case TokenKind.LessThanOrEqual:
                case TokenKind.In:
                case TokenKind.NotEqual:
                case TokenKind.NotIn:
                case TokenKind.Or:
                case TokenKind.Xor:
                    return 2;
                case TokenKind.Extension:
                    string name = context.Raw.Substring(token.Index, token.Length);
                    return context.Extensions[name].MinParameters;
                default: // Should never reach here.
                    throw new NotSupportedException($"Unexpected token kind '{token.Kind}'. Unable to determine min param count.");
            }
        }

        private static int GetMaxParamCount(ParseContext context, Token token)
        {
            switch (token.Kind)
            {
                case TokenKind.Not:
                    return 1;
                case TokenKind.Contains:
                case TokenKind.EndsWith:
                case TokenKind.Equal:
                case TokenKind.GreaterThan:
                case TokenKind.GreaterThanOrEqual:
                case TokenKind.LessThan:
                case TokenKind.LessThanOrEqual:
                case TokenKind.NotEqual:
                case TokenKind.Xor:
                    return 2;
                case TokenKind.And:
                case TokenKind.In:
                case TokenKind.NotIn:
                case TokenKind.Or:
                    return int.MaxValue;
                case TokenKind.Extension:
                    string name = context.Raw.Substring(token.Index, token.Length);
                    return context.Extensions[name].MaxParameters;
                default: // Should never reach here.
                    throw new NotSupportedException($"Unexpected token kind '{token.Kind}'. Unable to determine max param count.");
            }
        }

        private sealed class ContainerInfo
        {
            public ContainerNode Node { get; set; }

            public Token Token { get; set; }
        }

        private sealed class ParseContext
        {
            public readonly Stack<ContainerInfo> Containers = new Stack<ContainerInfo>();
            public readonly Dictionary<string, IExtensionInfo> Extensions = new Dictionary<string, IExtensionInfo>(StringComparer.OrdinalIgnoreCase);
            public readonly LexicalAnalyzer Lexer;
            public readonly string Raw;
            public readonly ITraceWriter Trace;
            public Token Token;
            public Token LastToken;
            public Node Root;

            public ParseContext(string expression, ITraceWriter trace, IEnumerable<IExtensionInfo> extensions)
            {
                if (trace == null)
                {
                    throw new ArgumentNullException(nameof(trace));
                }

                Raw = expression ?? string.Empty;
                Trace = trace;
                foreach (IExtensionInfo extension in (extensions ?? new IExtensionInfo[0]))
                {
                    Extensions.Add(extension.Name, extension);
                }

                Lexer = new LexicalAnalyzer(Raw, trace, Extensions.Keys);
            }
        }
    }

    public sealed class ParseException : Exception
    {
        private readonly string _message;

        internal ParseException(ParseExceptionKind kind, Token token, string condition)
        {
            Condition = condition;
            Kind = kind;
            TokenIndex = token.Index;
            TokenLength = token.Length;
            string description;
            // TODO: LOC
            switch (kind)
            {
                case ParseExceptionKind.ExpectedPropertyName:
                    description = "Expected property name to follow deference operator";
                    break;
                case ParseExceptionKind.ExpectedStartParameter:
                    description = "Expected '(' to follow function";
                    break;
                case ParseExceptionKind.UnclosedFunction:
                    description = "Unclosed function";
                    break;
                case ParseExceptionKind.UnclosedIndexer:
                    description = "Unclosed indexer";
                    break;
                case ParseExceptionKind.UnexpectedSymbol:
                    description = "Unexpected symbol";
                    break;
                case ParseExceptionKind.UnrecognizedValue:
                    description = "Unrecognized value";
                    break;
                default: // Should never reach here.
                    throw new Exception($"Unexpected parse exception kind '{kind}'.");
            }

            RawToken = condition.Substring(token.Index, token.Length);
            int position = token.Index + 1;
            // TODO: loc
            _message = $"{description}: '{RawToken}'. Located at position {position} within condition expression: {Condition}";
        }

        internal string Condition { get; private set; }

        internal ParseExceptionKind Kind { get; private set; }

        internal string RawToken { get; private set; }

        internal int TokenIndex { get; private set; }

        internal int TokenLength { get; private set; }

        public sealed override string Message => _message;
    }

    internal enum ParseExceptionKind
    {
        ExpectedPropertyName,
        ExpectedStartParameter,
        UnclosedFunction,
        UnclosedIndexer,
        UnexpectedSymbol,
        UnrecognizedValue,
    }
}