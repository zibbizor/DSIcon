using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UTFEditor
{
    // Structures by Mario Brito from FLModelToolby Anton (Xtreme Team Studios).

    public class VMeshRef
    {
        // Header - one per lod for each .3db section of cmp - 60 bytes
        public UInt32 HeaderSize;               // 0x0000003C
        public UInt32 VMeshLibId;               // crc of 3db name
        public UInt16 StartVert;
        public UInt16 NumVert;
        public UInt16 StartIndex;
        public UInt16 NumIndex;
        public UInt16 StartMesh;
        public UInt16 NumMeshes;
        public float BoundingBoxMaxX;
        public float BoundingBoxMinX;
        public float BoundingBoxMaxY;
        public float BoundingBoxMinY;
        public float BoundingBoxMaxZ;
        public float BoundingBoxMinZ;
        public float CenterX;
        public float CenterY;
        public float CenterZ;
        public float Radius;

        public VMeshRef(byte[] data)
        {
            int pos = 0;
            HeaderSize = Utilities.GetDWord(data, ref pos);
            VMeshLibId = Utilities.GetDWord(data, ref pos);
            StartVert = Utilities.GetWord(data, ref pos);
            NumVert = Utilities.GetWord(data, ref pos);
            StartIndex = Utilities.GetWord(data, ref pos);
            NumIndex = Utilities.GetWord(data, ref pos);
            StartMesh = Utilities.GetWord(data, ref pos);
            NumMeshes = Utilities.GetWord(data, ref pos);
            BoundingBoxMaxX = Utilities.GetFloat(data, ref pos);
            BoundingBoxMinX = Utilities.GetFloat(data, ref pos);
            BoundingBoxMaxY = Utilities.GetFloat(data, ref pos);
            BoundingBoxMinY = Utilities.GetFloat(data, ref pos);
            BoundingBoxMaxZ = Utilities.GetFloat(data, ref pos);
            BoundingBoxMinZ = Utilities.GetFloat(data, ref pos);
            CenterX = Utilities.GetFloat(data, ref pos);
            CenterY = Utilities.GetFloat(data, ref pos);
            CenterZ = Utilities.GetFloat(data, ref pos);
            Radius = Utilities.GetFloat(data, ref pos);
        }

        public byte[] GetBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(HeaderSize));
            data.AddRange(BitConverter.GetBytes(VMeshLibId));
            data.AddRange(BitConverter.GetBytes(StartVert));
            data.AddRange(BitConverter.GetBytes(NumVert));
            data.AddRange(BitConverter.GetBytes(StartIndex));
            data.AddRange(BitConverter.GetBytes(NumIndex));
            data.AddRange(BitConverter.GetBytes(StartMesh));
            data.AddRange(BitConverter.GetBytes(NumMeshes));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMaxX));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMinX));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMaxY));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMinY));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMaxZ));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMinZ));
            data.AddRange(BitConverter.GetBytes(CenterX));
            data.AddRange(BitConverter.GetBytes(CenterY));
            data.AddRange(BitConverter.GetBytes(CenterZ));
            data.AddRange(BitConverter.GetBytes(Radius));
            return data.ToArray();
        }
    }
}
