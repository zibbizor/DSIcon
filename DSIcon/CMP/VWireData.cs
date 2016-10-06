using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Structures by Mario Brito from FLModelToolby Anton (Xtreme Team Studios).
namespace UTFEditor
{
    public class VWireData
    {
        /// Wire Data Header
        public UInt32 HeaderSize;
        public UInt32 VMeshLibId;
        public UInt16 VertexOffset;
        public UInt16 NoVertices;
        public UInt16 NoRefVertices;
        public UInt16 MaxVertNoPlusOne;
    
        public struct Line
        {
            public UInt16 Point1;
            public UInt16 Point2;
        };

        public List<VWireData.Line> Lines = new List<VWireData.Line>();

        public VWireData(byte[] data)
        {
            int pos = 0;

            // Read the data header.
            HeaderSize = Utilities.GetDWord(data, ref pos);
            VMeshLibId = Utilities.GetDWord(data, ref pos);
            VertexOffset = Utilities.GetWord(data, ref pos);
            NoVertices = Utilities.GetWord(data, ref pos);
            NoRefVertices = Utilities.GetWord(data, ref pos);
            MaxVertNoPlusOne = Utilities.GetWord(data, ref pos);

            pos = (int)HeaderSize;

            // Read Line data
            int no_lines = NoRefVertices / 2;
            for (int count = 0; count < no_lines; count++)
            {
                Line item = new Line();
                item.Point1 = Utilities.GetWord(data, ref pos);
                item.Point2 = Utilities.GetWord(data, ref pos);
                Lines.Add(item);
            }
        }

        public byte[] GetBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(HeaderSize));
            data.AddRange(BitConverter.GetBytes(VMeshLibId));
            data.AddRange(BitConverter.GetBytes(VertexOffset));
            data.AddRange(BitConverter.GetBytes(NoVertices));
            data.AddRange(BitConverter.GetBytes(NoRefVertices));
            data.AddRange(BitConverter.GetBytes(MaxVertNoPlusOne));
            foreach (Line line in Lines)
            {
                data.AddRange(BitConverter.GetBytes(line.Point1));
                data.AddRange(BitConverter.GetBytes(line.Point2));
            }
            return data.ToArray();
        }
    }
}
