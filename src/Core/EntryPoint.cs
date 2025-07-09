using System.Net.Http.Headers;
using BfevLibrary.Common;
using BfevLibrary.Parsers;

namespace BfevLibrary.Core;

public class EntryPoint : IBfevDataBlock
{
    public List<short> SubFlowEventIndices { get; set; }

    public RadixTree<VariableDef>? Variables { get; set; }

    public short EventIndex { get; set; }

    public EntryPoint()
    {
    }

    public EntryPoint(BfevReader reader)
    {
        Read(reader);
    }

    public IBfevDataBlock Read(BfevReader reader)
    {
        long subFlowEventIndicesPtr = reader.ReadInt64();
        long variableDefNamesPtr = reader.ReadInt64();
        long variableDefsPtr = reader.ReadInt64();
        ushort subFlowEventIndicesCount = reader.ReadUInt16();
        ushort variableDefCount = reader.ReadUInt16();
        EventIndex = reader.ReadInt16();
        reader.BaseStream.Position += 2; // padding

        SubFlowEventIndices = reader.ReadObjectsPtr(
            new short[subFlowEventIndicesCount], reader.ReadInt16, subFlowEventIndicesPtr).ToList();

        Variables = reader.ReadObjectPtr(() => new RadixTree<VariableDef>(reader), variableDefNamesPtr);
        Variables?.LinkToArray(
            reader.ReadObjectsPtr(new VariableDef[variableDefCount], () => new VariableDef(reader), variableDefsPtr)
        );

        reader.Align(8);
        return this;
    }

    public void Write(BfevWriter writer)
    {
        Action insertSubFlowEventIndicesPtr = writer.ReservePtrIf(SubFlowEventIndices.Count > 0, register: true);
        Action insertVariableDefDictPtr = writer.ReservePtrIf(Variables is { Count: > 0 }, register: true);
        Action insertVariableDefsPtr = writer.ReservePtrIf(Variables is { Count: > 0 }, register: true);
        writer.Write((ushort)SubFlowEventIndices.Count);
        writer.Write((ushort)(Variables?.Count ?? 0));
        writer.Write(EventIndex);
        writer.Align(8);
        
        writer.ReserveBlockWriter("EntryPointExtraDataBlock", () => {
            if (SubFlowEventIndices.Count > 0) {
                insertSubFlowEventIndicesPtr();
                foreach (short s16 in SubFlowEventIndices) {
                    writer.Write(s16);
                }

                writer.Align(8);
            }

            if (Variables is not { Count: > 0 }) {
                return;
            }

            insertVariableDefDictPtr();
            writer.WriteRadixTree(Variables.Keys.ToArray());
            writer.Align(8);
            
            insertVariableDefsPtr();
            foreach (VariableDef variableDef in Variables.Values) {
                variableDef.Write(writer);
            }
            
            writer.WriteReserved("VariableDefData", alignment: 8);
            
            // The size for each entry point is sizeof(event_idx_array)
            // rounded up to the nearest multiple of 8 + 0x18 bytes.
            writer.Seek(0x18, SeekOrigin.Current);
        });
    }
}