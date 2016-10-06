using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UTFEditor
{
    /// <summary>
    /// A VMeshData Decoder
    /// </summary>
    public class VMeshData
    {
        // repeated <no_meshes> times in segment - 12 bytes
        public struct TMeshHeader
        {
            public uint MaterialId;         // crc of texture name for mesh
            public int StartVertex;
            public int EndVertex;
            public int NumRefVertices;
            public int Padding;             // 0x00CC

            public int TriangleStart;
        };

        // triangle definition - 6 bytes
        public struct TTriangle
        {
            public int Vertex1;
            public int Vertex2;
            public int Vertex3;
        };

        // vertex definition - 32 bytes
        public struct TVertex
        {
            public uint FVF;
            public float X;
            public float Y;
            public float Z;
            public float NormalX;
            public float NormalY;
            public float NormalZ;
            public uint Diffuse;
            public float S;
            public float T;
            public float U;
            public float V;
            public float TangentX;
            public float TangentY;
            public float TangentZ;
            public float BinormalX;
            public float BinormalY;
            public float BinormalZ;
        };

        // Data header - 16 bytes long
        public UInt32 MeshType;                 // 0x00000001
        public UInt32 SurfaceType;              // 0x00000004
        public UInt16 NumMeshes;
        public UInt16 NumRefVertices;
        public UInt16 FlexibleVertexFormat;     // 0x0112
        public UInt16 NumVertices;

        /// <summary>
        /// A list of meshes in the mesh data
        /// </summary>
        public List<TMeshHeader> Meshes = new List<TMeshHeader>();

        /// <summary>
        /// A list of triangles in the mesh data
        /// </summary>
        public List<TTriangle> Triangles = new List<TTriangle>();

        /// <summary>
        /// A list of Vertices in the mesh data
        /// </summary>
        public List<TVertex> Vertices = new List<TVertex>();

        public const uint D3DFVF_RESERVED0      = 0x001;
        public const uint D3DFVF_XYZ = 0x002;
        public const uint D3DFVF_XYZRHW = 0x004;
        public const uint D3DFVF_XYZB1 = 0x006;
        public const uint D3DFVF_XYZB2 = 0x008;
        public const uint D3DFVF_XYZB3 = 0x00a;
        public const uint D3DFVF_XYZB4 = 0x00c;
        public const uint D3DFVF_XYZB5 = 0x00e;

        public const uint D3DFVF_NORMAL = 0x010;
        public const uint D3DFVF_RESERVED1 = 0x020;
        public const uint D3DFVF_DIFFUSE = 0x040;
        public const uint D3DFVF_SPECULAR = 0x080;

        public const uint D3DFVF_TEXCOUNT_MASK = 0xf00;
        public const uint D3DFVF_TEX0 = 0x000;
        public const uint D3DFVF_TEX1 = 0x100;
        public const uint D3DFVF_TEX2 = 0x200;
        public const uint D3DFVF_TEX3 = 0x300;
        public const uint D3DFVF_TEX4 = 0x400;
        public const uint D3DFVF_TEX5 = 0x500;
        public const uint D3DFVF_TEX6 = 0x600;
        public const uint D3DFVF_TEX7 = 0x700;
        public const uint D3DFVF_TEX8 = 0x800;

        /// <summary>
        /// Decode the VMeshData
        /// </summary>
        /// <param name="data"></param>
        public VMeshData(byte[] data)
        {
            int pos = 0;

            // Read the data header.
            MeshType = Utilities.GetDWord(data, ref pos);
            SurfaceType = Utilities.GetDWord(data, ref pos);
            NumMeshes = Utilities.GetWord(data, ref pos);
            NumRefVertices = Utilities.GetWord(data, ref pos);
            FlexibleVertexFormat = Utilities.GetWord(data, ref pos);
            NumVertices = Utilities.GetWord(data, ref pos);

            // The FVF defines what fields are included for each vertex.
            switch (FlexibleVertexFormat)
            {
                case 0x02:
                case 0x12:
                case 0x102:
                case 0x112:
                case 0x142:
                case 0x152:
                case 0x212:
                case 0x252:
                case 0x412:
                case 0x512:
                    break;
                default:
                    throw new Exception(String.Format("FVF 0x{0:X} not supported.", FlexibleVertexFormat));
            }

            // Read the mesh headers.
            int triangleStartOffset = 0;
            for (int count = 0; count < NumMeshes; count++)
            {
                TMeshHeader item = new TMeshHeader();
                item.MaterialId = Utilities.GetDWord(data, ref pos);
                item.StartVertex = Utilities.GetWord(data, ref pos);
                item.EndVertex = Utilities.GetWord(data, ref pos);
                item.NumRefVertices = Utilities.GetWord(data, ref pos);
                item.Padding = Utilities.GetWord(data, ref pos);
               
                item.TriangleStart = triangleStartOffset;
                triangleStartOffset += item.NumRefVertices;

                Meshes.Add(item);
            }

            // Read the triangle data
            int num_triangles = NumRefVertices / 3;
            for (int count = 0; count < num_triangles; count++)
            {
                TTriangle item = new TTriangle();
                item.Vertex1 = Utilities.GetWord(data, ref pos);
                item.Vertex2 = Utilities.GetWord(data, ref pos);
                item.Vertex3 = Utilities.GetWord(data, ref pos);
                Triangles.Add(item);
            }

            // Read the vertex data.
            try
            {
                for (int count = 0; count < NumVertices; count++)
                {                 
                    TVertex item = new TVertex();
                    item.FVF = FlexibleVertexFormat;
                    item.X = Utilities.GetFloat(data, ref pos);
                    item.Y = Utilities.GetFloat(data, ref pos);
                    item.Z = Utilities.GetFloat(data, ref pos);
                    if ((FlexibleVertexFormat & D3DFVF_NORMAL) == D3DFVF_NORMAL)
                    {
                        item.NormalX = Utilities.GetFloat(data, ref pos);
                        item.NormalY = Utilities.GetFloat(data, ref pos);
                        item.NormalZ = Utilities.GetFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF_DIFFUSE) == D3DFVF_DIFFUSE)
                    {
                        item.Diffuse = Utilities.GetDWord(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF_TEX1) == D3DFVF_TEX1)
                    {
                        item.S = Utilities.GetFloat(data, ref pos);
                        item.T = Utilities.GetFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF_TEX2) == D3DFVF_TEX2)
                    {
                        item.S = Utilities.GetFloat(data, ref pos);
                        item.T = Utilities.GetFloat(data, ref pos);
                        item.U = Utilities.GetFloat(data, ref pos);
                        item.V = Utilities.GetFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF_TEX4) == D3DFVF_TEX4)
                    {
                        item.S = Utilities.GetFloat(data, ref pos);
                        item.T = Utilities.GetFloat(data, ref pos);
                        item.TangentX = Utilities.GetFloat(data, ref pos);
                        item.TangentY = Utilities.GetFloat(data, ref pos);
                        item.TangentZ = Utilities.GetFloat(data, ref pos);
                        item.BinormalX = Utilities.GetFloat(data, ref pos);
                        item.BinormalY = Utilities.GetFloat(data, ref pos);
                        item.BinormalZ = Utilities.GetFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF_TEX5) == D3DFVF_TEX5)
                    {
                        item.S = Utilities.GetFloat(data, ref pos);
                        item.T = Utilities.GetFloat(data, ref pos);
                        item.U = Utilities.GetFloat(data, ref pos);
                        item.V = Utilities.GetFloat(data, ref pos);
                        item.TangentX = Utilities.GetFloat(data, ref pos);
                        item.TangentY = Utilities.GetFloat(data, ref pos);
                        item.TangentZ = Utilities.GetFloat(data, ref pos);
                        item.BinormalX = Utilities.GetFloat(data, ref pos);
                        item.BinormalY = Utilities.GetFloat(data, ref pos);
                        item.BinormalZ = Utilities.GetFloat(data, ref pos);
                    }
                    Vertices.Add(item);
                }
            }
            catch
            {
                MessageBox.Show("Header has more vertices than data", "Error");
            }
        }

        /// <summary>
        /// Output the raw data
        /// </summary>
        public byte[] GetRawData()
        {
            // calc byte array size
            int iByteSize = 0;
            iByteSize += 16; // header
            iByteSize += NumMeshes * 12; // meshes header
            iByteSize += NumRefVertices * 2; // triangles

            iByteSize += NumVertices * 16; // vertices position

            if ((FlexibleVertexFormat & D3DFVF_NORMAL) == D3DFVF_NORMAL)
            {
                iByteSize += NumVertices * 16; // vertices normal
            }
            if ((FlexibleVertexFormat & D3DFVF_DIFFUSE) == D3DFVF_DIFFUSE)
            {
                iByteSize += NumVertices * 4; // vertices diffuse
            }
            if ((FlexibleVertexFormat & D3DFVF_TEX1) == D3DFVF_TEX1)
            {
                iByteSize += NumVertices * 8; // vertices texcoords
            }
            if ((FlexibleVertexFormat & D3DFVF_TEX2) == D3DFVF_TEX2)
            {
                iByteSize += NumVertices * 16; // vertices texcoords
            }
            if ((FlexibleVertexFormat & D3DFVF_TEX4) == D3DFVF_TEX4)
            {
                iByteSize += NumVertices * 8; // vertices texcoords
                iByteSize += NumVertices * 24; // vertices tangents binormals
            }
            if ((FlexibleVertexFormat & D3DFVF_TEX5) == D3DFVF_TEX5)
            {
                iByteSize += NumVertices * 16; // vertices texcoords
                iByteSize += NumVertices * 24; // vertices tangents binormals
            }


            byte[] data = new byte[iByteSize];
            int pos = 0;

            // Write the data header.
            Utilities.WriteDWord(data, MeshType, ref pos);
            Utilities.WriteDWord(data, SurfaceType, ref pos);
            Utilities.WriteWord(data, NumMeshes, ref pos);
            Utilities.WriteWord(data, NumRefVertices, ref pos);
            Utilities.WriteWord(data, FlexibleVertexFormat, ref pos);
            Utilities.WriteWord(data, NumVertices, ref pos);

            // write meshes
            foreach(TMeshHeader mesh in Meshes)
            {
                Utilities.WriteDWord(data, mesh.MaterialId, ref pos);
                Utilities.WriteWord(data, (UInt16)mesh.StartVertex, ref pos);
                Utilities.WriteWord(data, (UInt16)mesh.EndVertex, ref pos);
                Utilities.WriteWord(data, (UInt16)mesh.NumRefVertices, ref pos);
                Utilities.WriteWord(data, (UInt16)mesh.Padding, ref pos);
            }

            // Write the triangle data
            foreach(TTriangle triangle in Triangles)
            {
                Utilities.WriteWord(data, (UInt16)triangle.Vertex1, ref pos);
                Utilities.WriteWord(data, (UInt16)triangle.Vertex2, ref pos);
                Utilities.WriteWord(data, (UInt16)triangle.Vertex3, ref pos);
            }

            // Write the vertex data
            foreach(TVertex vertice in Vertices)
            {
                Utilities.WriteFloat(data, vertice.X, ref pos);
                Utilities.WriteFloat(data, vertice.Y, ref pos);
                Utilities.WriteFloat(data, vertice.Z, ref pos);
                if ((FlexibleVertexFormat & D3DFVF_NORMAL) == D3DFVF_NORMAL)
                {
                    Utilities.WriteFloat(data, vertice.NormalX, ref pos);
                    Utilities.WriteFloat(data, vertice.NormalY, ref pos);
                    Utilities.WriteFloat(data, vertice.NormalZ, ref pos);
                }
                if ((FlexibleVertexFormat & D3DFVF_DIFFUSE) == D3DFVF_DIFFUSE)
                {
                    Utilities.WriteDWord(data, vertice.Diffuse, ref pos);
                }
                if ((FlexibleVertexFormat & D3DFVF_TEX1) == D3DFVF_TEX1)
                {
                    Utilities.WriteFloat(data, vertice.S, ref pos);
                    Utilities.WriteFloat(data, vertice.T, ref pos);
                }
                if ((FlexibleVertexFormat & D3DFVF_TEX2) == D3DFVF_TEX2)
                {
                    Utilities.WriteFloat(data, vertice.S, ref pos);
                    Utilities.WriteFloat(data, vertice.T, ref pos);
                    Utilities.WriteFloat(data, vertice.U, ref pos);
                    Utilities.WriteFloat(data, vertice.V, ref pos);
                }
                if ((FlexibleVertexFormat & D3DFVF_TEX4) == D3DFVF_TEX4)
                {
                    Utilities.WriteFloat(data, vertice.S, ref pos);
                    Utilities.WriteFloat(data, vertice.T, ref pos);
                    Utilities.WriteFloat(data, vertice.TangentX, ref pos);
                    Utilities.WriteFloat(data, vertice.TangentY, ref pos);
                    Utilities.WriteFloat(data, vertice.TangentZ, ref pos);
                    Utilities.WriteFloat(data, vertice.BinormalX, ref pos);
                    Utilities.WriteFloat(data, vertice.BinormalY, ref pos);
                    Utilities.WriteFloat(data, vertice.BinormalZ, ref pos);
                }
                if ((FlexibleVertexFormat & D3DFVF_TEX5) == D3DFVF_TEX5)
                {
                    Utilities.WriteFloat(data, vertice.S, ref pos);
                    Utilities.WriteFloat(data, vertice.T, ref pos);
                    Utilities.WriteFloat(data, vertice.U, ref pos);
                    Utilities.WriteFloat(data, vertice.V, ref pos);
                    Utilities.WriteFloat(data, vertice.TangentX, ref pos);
                    Utilities.WriteFloat(data, vertice.TangentY, ref pos);
                    Utilities.WriteFloat(data, vertice.TangentZ, ref pos);
                    Utilities.WriteFloat(data, vertice.BinormalX, ref pos);
                    Utilities.WriteFloat(data, vertice.BinormalY, ref pos);
                    Utilities.WriteFloat(data, vertice.BinormalZ, ref pos);
                }
            }

            return data;
        }

    }
}
