using Interpreter.Parser;

namespace Interpreter.Evaluation;

public enum Objects
{
    IntegerObject,
    BooleanObject,
    NullObject,
    ReturnObject,
}

public interface IObject
{
    ObjectType Type();
    string Inspect();
}

public class IntegerObject : IObject
{
    public int Value { get; set; }

    public IntegerObject(int value)
    {
        Value = value;
    }

    public string Inspect() => Value.ToString();

    public string Type() => Objects.IntegerObject.ToString();

    public override string ToString()
    {
        return $"[Integer: {Inspect()}]";
    }
}

public class BooleanObject : IObject
{
    private static BooleanObject? _true;
    private static BooleanObject? _false;

    public static BooleanObject False
    {
        get
        {
            _false ??= new BooleanObject(false);
            return _false;
        }
    }

    public static BooleanObject True
    {
        get
        {
            _true ??= new BooleanObject(true);
            return _true;
        }
    }

    public bool Value { get; set; }

    private BooleanObject(bool value)
    {
        Value = value;
    }

    public string Inspect() => Value.ToString();

    public string Type() => Objects.BooleanObject.ToString();

    public static BooleanObject NativeBoolToBoolean(bool input) => input ? True : False;

    public static BooleanObject NativeBoolToBoolean(BooleanLiteral input) =>
        NativeBoolToBoolean(input.Value);

    public override string ToString()
    {
        return $"[Boolean: {Inspect()}]";
    }
}

public class NullObject : IObject
{
    private static NullObject? _instance;
    public static NullObject Instance
    {
        get
        {
            _instance ??= new NullObject();
            return _instance;
        }
    }

    private NullObject() { }

    public string Inspect() => "null";

    public string Type() => Objects.NullObject.ToString();
}

public class ReturnObject : IObject
{
    public IObject? Value { get; set; }

    public ReturnObject(IObject? value)
    {
        Value = value;
    }

    public string Inspect() => Value?.ToString() ?? "";

    public string Type() => Objects.ReturnObject.ToString();
}
