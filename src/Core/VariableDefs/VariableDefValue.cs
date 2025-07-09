using BfevLibrary.Parsers;

namespace BfevLibrary.Core;

public class VariableDefValue
{
    public int? Int { get; set; }

    public float? Float { get; set; }

    public int[]? IntArray { get; set; }

    public float[]? FloatArray { get; set; }

    protected VariableDefValue()
    {
    }
    
    protected void Read(BfevReader reader, long value, ushort count, ContainerDataType type)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type) {
            case ContainerDataType.Int:
                Int = (int)value;
                break;
            case ContainerDataType.Float:
                Float = (float)value;
                break;
            case ContainerDataType.IntArray: {
                IntArray = reader.ReadObjectsPtr(new int[count], reader.ReadInt32, value);
                break;
            }
            case ContainerDataType.FloatArray: {
                FloatArray = reader.ReadObjectsPtr(new float[count], reader.ReadSingle, value);
                break;
            }
        }
    }

    protected Action? WriteData(BfevWriter writer)
    {
        if (Int != null) {
            writer.Write((ulong)Int);
            return null;
        }
        
        if (Float != null) {
            writer.Write((ulong)Float);
            return null;
        }
        
        if (IntArray != null || FloatArray != null) {
            return writer.ReservePtr();
        }

        throw new InvalidOperationException("Invalid variable def data");
    }

    protected int GetCount(ContainerDataType type)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return type switch {
            ContainerDataType.Int => 1,
            ContainerDataType.Float => 1,
            ContainerDataType.IntArray => IntArray!.Length,
            ContainerDataType.FloatArray => FloatArray!.Length,
            _ => throw new NotSupportedException($"Unsupported variable type: {type}")
        };
    }

    public ContainerDataType GetDataType()
    {
        if (Int != null) {
            return ContainerDataType.Int;
        }

        if (Float != null) {
            return ContainerDataType.Float;
        }

        if (IntArray != null) {
            return ContainerDataType.IntArray;
        }

        if (FloatArray != null) {
            return ContainerDataType.FloatArray;
        }

        throw new NullReferenceException("Missing variable data. All possible union types were null.");
    }
}