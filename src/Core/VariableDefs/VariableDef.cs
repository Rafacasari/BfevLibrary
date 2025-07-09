using BfevLibrary.Parsers;

namespace BfevLibrary.Core;

public class VariableDef : VariableDefValue
{
    public VariableDef()
    {   
    }
    
    public VariableDef(BfevReader reader)
    {
        long value = reader.ReadInt64();
        ushort count = reader.ReadUInt16();
        var type = (ContainerDataType)reader.ReadByte();
        reader.Align(8);
        Read(reader, value, count, type);
    }

    public void Write(BfevWriter writer)
    {
        Action? ptr = WriteData(writer);
        ContainerDataType type = GetDataType();
        writer.Write((ushort)GetCount(type));
        writer.Write((byte)type);
        writer.Align(8);

        if (ptr is null) {
            return;
        }
        
        writer.ReserveBlockWriter("VariableDefData", () => {
            ptr();

            if (IntArray is not null) {
                foreach (int s32 in IntArray) {
                    writer.Write((long)s32);
                }
            }
            else if (FloatArray is not null) {
                foreach (int f32 in FloatArray) {
                    writer.Write((long)f32);
                }
            }
        });
    }
}