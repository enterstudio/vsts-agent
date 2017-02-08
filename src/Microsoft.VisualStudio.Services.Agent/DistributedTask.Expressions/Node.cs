using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Microsoft.VisualStudio.Services.DistributedTask.Expressions
{
    public abstract class Node
    {
        internal ContainerNode Container { get; set; }

        internal int Level { get; private set; }

        internal virtual string Name { get; set; }

        protected abstract object EvaluateCore(EvaluationContext context);

        public bool EvaluateBoolean(EvaluationContext context)
        {
            return Evaluate(context).ConvertToBoolean(context);
        }

        public EvaluationResult Evaluate(EvaluationContext context)
        {
            Level = Container == null ? 0 : Container.Level + 1;
            TraceVerbose(context, Level, $"Evaluating {Name}:");
            return new EvaluationResult(context, Level, EvaluateCore(context));
        }

        public decimal EvaluateNumber(EvaluationContext context)
        {
            return Evaluate(context).ConvertToNumber(context);
        }

        public string EvaluateString(EvaluationContext context)
        {
            return Evaluate(context).ConvertToString(context);
        }

        public Version EvaluateVersion(EvaluationContext context)
        {
            return Evaluate(context).ConvertToVersion(context);
        }

        internal static void TraceVerbose(EvaluationContext context, int level, string message)
        {
            context.Trace.Verbose(string.Empty.PadLeft(level * 2, '.') + (message ?? string.Empty));
        }
    }

    public sealed class LeafNode : Node
    {
        public LeafNode(object val)
        {
            Value = val;
        }

        public object Value { get; }

        internal sealed override string Name => "leaf";

        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return Value;
        }
    }

    public abstract class ContainerNode : Node
    {
        private readonly List<Node> _parameters = new List<Node>();

        public IReadOnlyList<Node> Parameters => _parameters.AsReadOnly();

        public void AddParameter(Node node)
        {
            _parameters.Add(node);
            node.Container = this;
        }

        public void ReplaceParameter(int index, Node node)
        {
            _parameters[index] = node;
            node.Container = this;
        }
    }

    internal sealed class IndexerNode : ContainerNode
    {
        internal sealed override string Name => "indexer";

        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            object result = null;
            EvaluationResult item = Parameters[0].Evaluate(context);
            if (item.Kind == ValueKind.Array && item.Value is JArray)
            {
                var jarray = item.Value as JArray;
                EvaluationResult index = Parameters[1].Evaluate(context);
                if (index.Kind == ValueKind.Number)
                {
                    decimal d = (decimal)index.Value;
                    if (d >= 0m && d < (decimal)jarray.Count && d == Math.Floor(d))
                    {
                        result = jarray[(int)d];
                    }
                }
                else if (index.Kind == ValueKind.String && !string.IsNullOrEmpty(index.Value as string))
                {
                    decimal d;
                    if (index.TryConvertToNumber(context, out d))
                    {
                        if (d >= 0m && d < (decimal)jarray.Count && d == Math.Floor(d))
                        {
                            result = jarray[(int)d];
                        }
                    }
                }
            }
            else if (item.Kind == ValueKind.Object && item.Value is JObject)
            {
                var jobject = item.Value as JObject;
                EvaluationResult index = Parameters[1].Evaluate(context);
                string s;
                if (index.TryConvertToString(context, out s))
                {
                    result = jobject[s];
                }
            }

            return result;
        }
    }

    public abstract class FunctionNode : ContainerNode
    {
    }

    internal sealed class AndNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            foreach (Node parameter in Parameters)
            {
                if (!parameter.EvaluateBoolean(context))
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal sealed class ContainsNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            string left = Parameters[0].EvaluateString(context) as string ?? string.Empty;
            string right = Parameters[1].EvaluateString(context) as string ?? string.Empty;
            return left.IndexOf(right, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    internal sealed class EndsWithNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            string left = Parameters[0].EvaluateString(context) ?? string.Empty;
            string right = Parameters[1].EvaluateString(context) ?? string.Empty;
            return left.EndsWith(right, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class EqualNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return Parameters[0].Evaluate(context).Equals(context, Parameters[1].Evaluate(context));
        }
    }

    internal sealed class GreaterThanNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return Parameters[0].Evaluate(context).CompareTo(context, Parameters[1].Evaluate(context)) > 0;
        }
    }

    internal sealed class GreaterThanOrEqualNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return Parameters[0].Evaluate(context).CompareTo(context, Parameters[1].Evaluate(context)) >= 0;
        }
    }

    internal sealed class InNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            EvaluationResult left = Parameters[0].Evaluate(context);
            for (int i = 1; i < Parameters.Count; i++)
            {
                EvaluationResult right = Parameters[i].Evaluate(context);
                if (left.Equals(context, right))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal sealed class LessThanNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return Parameters[0].Evaluate(context).CompareTo(context, Parameters[1].Evaluate(context)) < 0;
        }
    }

    internal sealed class LessThanOrEqualNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return Parameters[0].Evaluate(context).CompareTo(context, Parameters[1].Evaluate(context)) <= 0;
        }
    }

    internal sealed class NotEqualNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return !Parameters[0].Evaluate(context).Equals(context, Parameters[1].Evaluate(context));
        }
    }

    internal sealed class NotNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return !Parameters[0].EvaluateBoolean(context);
        }
    }

    internal sealed class NotInNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            EvaluationResult left = Parameters[0].Evaluate(context);
            for (int i = 1; i < Parameters.Count; i++)
            {
                EvaluationResult right = Parameters[1].Evaluate(context);
                if (left.Equals(context, right))
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal sealed class OrNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            foreach (Node parameter in Parameters)
            {
                if (parameter.EvaluateBoolean(context))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal sealed class StartsWithNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            string left = Parameters[0].EvaluateString(context) ?? string.Empty;
            string right = Parameters[1].EvaluateString(context) ?? string.Empty;
            return left.StartsWith(right, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class XorNode : FunctionNode
    {
        protected sealed override object EvaluateCore(EvaluationContext context)
        {
            return Parameters[0].EvaluateBoolean(context) ^ Parameters[1].EvaluateBoolean(context);
        }
    }

    public sealed class EvaluationResult
    {
        private static readonly NumberStyles s_numberStyles =
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowLeadingWhite |
            NumberStyles.AllowThousands |
            NumberStyles.AllowTrailingWhite;
        private readonly int _level;

        public EvaluationResult(EvaluationContext context, int level, object raw)
        {
            _level = level;
            ValueKind kind;
            Value = ConvertToCanonicalValue(raw, out kind);
            Kind = kind;
            TraceValue(context);
        }

        private EvaluationResult(EvaluationContext context, int level, object val, ValueKind kind)
        {
            _level = level;
            Value = val;
            Kind = kind;
            TraceValue(context);
        }

        public ValueKind Kind { get; }

        public object Value { get; }

        public int CompareTo(EvaluationContext context, EvaluationResult right)
        {
            object leftValue;
            ValueKind leftKind;
            switch (Kind)
            {
                case ValueKind.Boolean:
                case ValueKind.Number:
                case ValueKind.String:
                case ValueKind.Version:
                    leftValue = Value;
                    leftKind = Kind;
                    break;
                default:
                    leftValue = ConvertToNumber(context); // Will throw or succeed
                    leftKind = ValueKind.Number;
                    break;
            }

            if (leftKind == ValueKind.Boolean)
            {
                bool b = right.ConvertToBoolean(context);
                return ((bool)leftValue).CompareTo(b);
            }
            else if (leftKind == ValueKind.Number)
            {
                decimal d = right.ConvertToNumber(context);
                return ((decimal)leftValue).CompareTo(d);
            }
            else if (leftKind == ValueKind.String)
            {
                string s = right.ConvertToString(context);
                return string.Compare(leftValue as string ?? string.Empty, s ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }
            else //if (leftKind == ValueKind.Version)
            {
                Version v = right.ConvertToVersion(context);
                return (leftValue as Version).CompareTo(v);
            }
        }

        public bool ConvertToBoolean(EvaluationContext context)
        {
            bool result;
            switch (Kind)
            {
                case ValueKind.Boolean:
                    return (bool)Value; // Not converted. Don't trace.

                case ValueKind.Number:
                    result = (decimal)Value != 0m; // 0 converts to false, otherwise true.
                    TraceValue(context, result, ValueKind.Boolean);
                    return result;

                case ValueKind.String:
                    result = !string.IsNullOrEmpty(Value as string);
                    TraceValue(context, result, ValueKind.Boolean);
                    return result;

                case ValueKind.Array:
                case ValueKind.Object:
                case ValueKind.Version:
                    result = true;
                    TraceValue(context, result, ValueKind.Boolean);
                    return result;

                case ValueKind.Null:
                    result = false;
                    TraceValue(context, result, ValueKind.Boolean);
                    return result;

                default:
                    throw new NotSupportedException($"Unable to convert value to Boolean. Unexpected value kind '{Kind}'.");
            }
        }

        public object ConvertToNull(EvaluationContext context)
        {
            object result;
            if (TryConvertToNull(context, out result))
            {
                return result;
            }

            throw new ConvertException(Value, fromKind: Kind, toKind: ValueKind.Null);
        }

        public decimal ConvertToNumber(EvaluationContext context)
        {
            decimal result;
            if (TryConvertToNumber(context, out result))
            {
                return result;
            }

            throw new ConvertException(Value, fromKind: Kind, toKind: ValueKind.Number);
        }

        public string ConvertToString(EvaluationContext context)
        {
            string result;
            if (TryConvertToString(context, out result))
            {
                return result;
            }

            throw new ConvertException(Value, fromKind: Kind, toKind: ValueKind.String);
        }

        public Version ConvertToVersion(EvaluationContext context)
        {
            Version result;
            if (TryConvertToVersion(context, out result))
            {
                return result;
            }

            throw new ConvertException(Value, fromKind: Kind, toKind: ValueKind.Version);
        }

        public bool Equals(EvaluationContext context, EvaluationResult right)
        {
            if (Kind == ValueKind.Boolean)
            {
                bool b = right.ConvertToBoolean(context);
                return (bool)Value == b;
            }
            else if (Kind == ValueKind.Number)
            {
                decimal d;
                if (right.TryConvertToNumber(context, out d))
                {
                    return (decimal)Value == d;
                }
            }
            else if (Kind == ValueKind.Version)
            {
                Version v;
                if (right.TryConvertToVersion(context, out v))
                {
                    return (Version)Value == v;
                }
            }
            else if (Kind == ValueKind.String)
            {
                string s;
                if (right.TryConvertToString(context, out s))
                {
                    return string.Equals(
                        Value as string ?? string.Empty,
                        s ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase);
                }
            }
            else if (Kind == ValueKind.Array || Kind == ValueKind.Object)
            {
                return Kind == right.Kind && object.ReferenceEquals(Value, right.Value);
            }
            else if (Kind == ValueKind.Null)
            {
                object n;
                if (right.TryConvertToNull(context, out n))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryConvertToNull(EvaluationContext context, out object result)
        {
            switch (Kind)
            {
                case ValueKind.Null:
                    result = null; // Not converted. Don't trace again.
                    return true;

                case ValueKind.String:
                    if (string.IsNullOrEmpty(Value as string))
                    {
                        result = null;
                        TraceValue(context, result, ValueKind.Null);
                        return true;
                    }

                    break;
            }

            result = null;
            TraceCoercionFailed(context, toKind: ValueKind.Null);
            return false;
        }

        public bool TryConvertToNumber(EvaluationContext context, out decimal result)
        {
            switch (Kind)
            {
                case ValueKind.Boolean:
                    result = (bool)Value ? 1m : 0m;
                    TraceValue(context, result, ValueKind.Number);
                    return true;

                case ValueKind.Number:
                    result = (decimal)Value; // Not converted. Don't trace again.
                    return true;

                case ValueKind.Version:
                    result = default(decimal);
                    TraceCoercionFailed(context, toKind: ValueKind.Number);
                    return false;

                case ValueKind.String:
                    string s = Value as string ?? string.Empty;
                    if (string.IsNullOrEmpty(s))
                    {
                        result = 0m;
                        TraceValue(context, result, ValueKind.Number);
                        return true;
                    }

                    if (decimal.TryParse(s, s_numberStyles, CultureInfo.InvariantCulture, out result))
                    {
                        TraceValue(context, result, ValueKind.Number);
                        return true;
                    }

                    TraceCoercionFailed(context, toKind: ValueKind.Number);
                    return false;

                case ValueKind.Array:
                case ValueKind.Object:
                    result = default(decimal);
                    TraceCoercionFailed(context, toKind: ValueKind.Number);
                    return false;

                case ValueKind.Null:
                    result = 0m;
                    TraceValue(context, result, ValueKind.Number);
                    return true;

                default:
                    throw new NotSupportedException($"Unable to determine whether value can be converted to Number. Unexpected value kind '{Kind}'.");
            }
        }

        public bool TryConvertToString(EvaluationContext context, out string result)
        {
            switch (Kind)
            {
                case ValueKind.Boolean:
                    result = string.Format(CultureInfo.InvariantCulture, "{0}", Value);
                    TraceValue(context, result, ValueKind.String);
                    return true;

                case ValueKind.Number:
                    result = ((decimal)Value).ToString("G", CultureInfo.InvariantCulture);
                    if (result.Contains("."))
                    {
                        result = result.TrimEnd('0').TrimEnd('.'); // Omit trailing zeros after the decimal point.
                    }

                    TraceValue(context, result, ValueKind.String);
                    return true;

                case ValueKind.String:
                    result = Value as string; // Not converted. Don't trace.
                    return true;

                case ValueKind.Version:
                    result = (Value as Version).ToString();
                    TraceValue(context, result, ValueKind.String);
                    return true;

                case ValueKind.Null:
                    result = string.Empty;
                    TraceValue(context, result, ValueKind.Null);
                    return true;

                case ValueKind.Array:
                case ValueKind.Object:
                    result = null;
                    TraceCoercionFailed(context, toKind: ValueKind.String);
                    return false;

                default:
                    throw new NotSupportedException($"Unable to convert to String. Unexpected value kind '{Kind}'.");
            }
        }

        public bool TryConvertToVersion(EvaluationContext context, out Version result)
        {
            switch (Kind)
            {
                case ValueKind.Boolean:
                    result = null;
                    TraceCoercionFailed(context, toKind: ValueKind.Version);
                    return false;

                case ValueKind.Number:
                    if (Version.TryParse(((decimal)Value).ToString("G", CultureInfo.InvariantCulture), out result))
                    {
                        TraceValue(context, result, ValueKind.Version);
                        return true;
                    }

                    TraceCoercionFailed(context, toKind: ValueKind.Version);
                    return false;

                case ValueKind.String:
                    string s = Value as string ?? string.Empty;
                    if (Version.TryParse(s, out result))
                    {
                        TraceValue(context, result, ValueKind.Version);
                        return true;
                    }

                    TraceCoercionFailed(context, toKind: ValueKind.Version);
                    return false;

                case ValueKind.Version:
                    result = Value as Version; // Not converted. Don't trace again.
                    return true;

                case ValueKind.Array:
                case ValueKind.Object:
                case ValueKind.Null:
                    result = null;
                    TraceCoercionFailed(context, toKind: ValueKind.Version);
                    return false;

                default:
                    throw new NotSupportedException($"Unable to convert to Version. Unexpected value kind '{Kind}'.");
            }
        }

        private void TraceCoercionFailed(EvaluationContext context, ValueKind toKind)
        {
            TraceVerbose(context, string.Format(CultureInfo.InvariantCulture, "=> Unable to coerce {0} to {1}.", Kind, toKind));
        }

        private void TraceValue(EvaluationContext context)
        {
            TraceValue(context, Value, Kind);
        }

        private void TraceValue(EvaluationContext context, object val, ValueKind kind)
        {
            switch (kind)
            {
                case ValueKind.Boolean:
                case ValueKind.Number:
                case ValueKind.Version:
                    TraceVerbose(context, String.Format(CultureInfo.InvariantCulture, "=> ({0}) {1}", kind, val));
                    break;
                case ValueKind.String:
                    TraceVerbose(context, String.Format(CultureInfo.InvariantCulture, "=> ({0}) '{1}'", kind, (val as string).Replace("'", "''")));
                    break;
                default:
                    TraceVerbose(context, string.Format(CultureInfo.InvariantCulture, "=> ({0})", kind));
                    break;
            }
        }

        private void TraceVerbose(EvaluationContext context, string message)
        {
            context.Trace.Verbose(string.Empty.PadLeft(_level * 2, '.') + (message ?? string.Empty));
        }

        private static object ConvertToCanonicalValue(object val, out ValueKind kind)
        {
            if (object.ReferenceEquals(val, null))
            {
                kind = ValueKind.Null;
                return null;
            }
            else if (val is JToken)
            {
                var jtoken = val as JToken;
                switch (jtoken.Type)
                {
                    case JTokenType.Array:
                        kind = ValueKind.Array;
                        return jtoken;
                    case JTokenType.Boolean:
                        kind = ValueKind.Boolean;
                        return jtoken.ToObject<bool>();
                    case JTokenType.Float:
                        kind = ValueKind.Number;
                        // todo: test the extents of the conversion
                        return jtoken.ToObject<decimal>();
                    case JTokenType.Integer:
                        kind = ValueKind.Number;
                        // todo: test the extents of the conversion
                        return jtoken.ToObject<decimal>();
                    case JTokenType.Null:
                        kind = ValueKind.Null;
                        return null;
                    case JTokenType.Object:
                        kind = ValueKind.Object;
                        return jtoken;
                    case JTokenType.String:
                        kind = ValueKind.String;
                        return jtoken.ToObject<string>();
                }
            }
            else if (val is string)
            {
                kind = ValueKind.String;
                return val;
            }
            else if (val is Version)
            {
                kind = ValueKind.Version;
                return val;
            }
            else if (!val.GetType().GetTypeInfo().IsClass)
            {
                if (val is bool)
                {
                    kind = ValueKind.Boolean;
                    return val;
                }
                else if (val is decimal || val is byte || val is sbyte || val is short || val is ushort || val is int || val is uint || val is long || val is ulong || val is float || val is double)
                {
                    kind = ValueKind.Number;
                    // todo: test the extents of the conversion
                    return (decimal)val;
                }
            }

            kind = ValueKind.Object;
            return val;
        }
    }

    public enum ValueKind
    {
        Array,
        Boolean,
        Null,
        Number,
        Object,
        String,
        Version,
    }

    internal sealed class ConvertException : Exception
    {
        private readonly string _message;

        public ConvertException(object val, ValueKind fromKind, ValueKind toKind)
        {
            Value = val;
            FromKind = fromKind;
            ToKind = toKind;
            switch (fromKind)
            {
                case ValueKind.Boolean:
                case ValueKind.Number:
                case ValueKind.String:
                case ValueKind.Version:
                    // TODO: loc
                    _message = $"Unable to convert from {FromKind} to {ToKind}. Value: '{val}'";
                    break;
                default:
                    // TODO: loc
                    _message = $"Unable to convert from {FromKind} to {ToKind}. Value: '{val}'";
                    break;
            }
        }

        public object Value { get; private set; }

        public ValueKind FromKind { get; private set; }

        public ValueKind ToKind { get; private set; }

        public sealed override string Message => _message;
    }
}