using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ImageMagick;

using UTFFile = UTFEditor.UTFFile;

namespace DSIcon
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rnd = new Random();
            //Parse the image(s?)
            if (args.Length > 0)
            {
                foreach (var item in args)
                {
                    if (File.Exists(item))
                    {
                        try
                        {
                            using (MagickImage image = new MagickImage(item))
                            {
                                var filename = Path.GetFileNameWithoutExtension(item);
                                var path = Path.GetDirectoryName(item);

                                var imgdata = new MemoryStream();
                                int rng = rnd.Next(65535);
                                var name = filename + rng + ".tga";
                                var shortname = filename + rng;
                                //image.Format = MagickFormat.Dds;
                                image.Format = MagickFormat.Tga;
                                //image.Flip();
                                image.Clamp();
                                image.Scale(64, 64);
                                //image.Settings.SetDefine(MagickFormat.Dds, "compression", "dxt1");
                                //image.Settings.SetDefine(MagickFormat.Dds, "mipmaps", false);
                                image.Write(imgdata);

                                //test
                                //image.Write(name);

                                //Make the CMP
                                UTFFile utfFile = new UTFFile();

                                //Initialize the root node
                                TreeNode CMPRoot = new TreeNode("\\");
                                CMPRoot.Tag = AlleyUtils.StringToByte("");
                                CMPRoot.Nodes.Add("Exporter Version", "Exporter Version");
                                CMPRoot.Nodes["Exporter Version"].Tag = AlleyUtils.StringToCMPString("Converted with DSIcon v0.1");

                                //// TEXTURE ////
                                //Set up the texture library
                                TreeNode TextureLibrary = new TreeNode("Texture library");
                                TextureLibrary.Tag = AlleyUtils.StringToByte("");

                                //Set up the texture
                                TreeNode Texture = new TreeNode(name);
                                Texture.Tag = AlleyUtils.StringToByte("");

                                //Set up the MIPS (we don't have texture LODs for icons)
                                TreeNode MIPS = new TreeNode("MIP0");
                                MIPS.Tag = imgdata.ToArray();

                                //Add Nodes to the root
                                Texture.Nodes.Add(MIPS);
                                TextureLibrary.Nodes.Add(Texture);
                                

                                //// MATERIAL ////
                                TreeNode MaterialLibrary = new TreeNode("Material library");

                                MaterialLibrary.Tag = AlleyUtils.StringToByte("");

                                //Material Count
                                TreeNode MaterialCount = new TreeNode("Material count");
                                MaterialCount.Tag = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

                                //The material itself
                                TreeNode Material = new TreeNode(shortname);
                                Material.Tag = AlleyUtils.StringToByte("");
                                //Material flags
                                TreeNode Material_Dt_flags = new TreeNode("Dt_flags");
                                Material_Dt_flags.Tag = new byte[] { 64, 0, 0, 0, 0, 0, 0, 0 };
                                //Material texture name
                                TreeNode Material_Dt_name = new TreeNode("Dt_name");
                                Material_Dt_name.Tag = AlleyUtils.StringToByte(name);
                                //Material type
                                TreeNode Material_Type = new TreeNode("Type");
                                Material_Type.Tag = AlleyUtils.StringToByte("DcDt");

                                //Add Nodes to root
                                Material.Nodes.Add(Material_Dt_flags);
                                Material.Nodes.Add(Material_Dt_name);
                                Material.Nodes.Add(Material_Type);

                                MaterialLibrary.Nodes.Add(Material);
                                MaterialLibrary.Nodes.Add(MaterialCount);                                

                                //Set up the vmesh
                                var MaterialCRC = UTFEditor.Utilities.FLModelCRC(shortname);
                                TreeNode VMeshLibrary = new TreeNode("VMeshLibrary");
                                VMeshLibrary.Tag = AlleyUtils.StringToByte("");

                                //Create a generic icon mesh
                                TreeNode Mesh = new TreeNode(shortname + ".lod0-112.vms");
                                Mesh.Tag = AlleyUtils.StringToByte("");
                                //Create mesh data
                                TreeNode MeshData = new TreeNode("VMeshData");
                                MeshData.Tag = MakeVMeshData(MaterialCRC);

                                //Append the nodes
                                Mesh.Nodes.Add(MeshData);
                                VMeshLibrary.Nodes.Add(Mesh);

                                //Set up the VMeshPart
                                TreeNode VMeshPart = new TreeNode("VMeshPart");
                                VMeshPart.Tag = AlleyUtils.StringToByte("");
                                //Create the VMeshRef
                                var VMeshNameCRC = UTFEditor.Utilities.FLModelCRC(shortname + ".lod0-112.vms");
                                TreeNode VMeshRef = new TreeNode("VMeshRef");
                                VMeshRef.Tag = MakeVMeshRef(VMeshNameCRC);

                                //Append the nodes
                                VMeshPart.Nodes.Add(VMeshRef);

                                //Append all to root
                                CMPRoot.Nodes.Add(VMeshLibrary);
                                CMPRoot.Nodes.Add(VMeshPart);
                                CMPRoot.Nodes.Add(MaterialLibrary);
                                CMPRoot.Nodes.Add(TextureLibrary);

                                utfFile.SaveUTFFile(CMPRoot, path + "\\" + filename + ".3db");
                                Console.WriteLine("parsed file " + item);
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to parse file " + item);
                        }                  
                    }
                }               
            }
            else
            {
                Console.WriteLine("Give me files thanks");
                
            }

            Console.ReadLine();
        }

        //We only have one material here and it's always going to be the same mesh so we can just do it the stupid way
        static byte[] MakeVMeshData(UInt32 MaterialID)
        {
            byte[] MaterialToBytes = BitConverter.GetBytes(MaterialID);
            byte[] header = new byte[] { 1, 0, 0, 0, 4, 0, 0, 0, 1, 0, 90, 0, 18, 1, 24, 0 };
            byte[] meshheader = new byte[] { 0, 0, 23, 0, 90, 0, 204, 0 };
            byte[] meshdata = new byte[] { 0, 0, 1, 0, 2, 0, 0, 0, 2, 0, 3, 0, 2, 0, 4, 0, 5, 0, 2, 0, 5, 0, 3, 0, 5, 0, 4, 0, 6, 0, 5, 0, 6, 0, 7, 0, 8, 0, 9, 0, 2, 0, 8, 0, 2, 0, 1, 0, 2, 0, 9, 0, 10, 0, 2, 0, 10, 0, 4, 0, 10, 0, 11, 0, 6, 0, 10, 0, 6, 0, 4, 0, 8, 0, 12, 0, 13, 0, 8, 0, 13, 0, 9, 0, 13, 0, 14, 0, 10, 0, 13, 0, 10, 0, 9, 0, 10, 0, 14, 0, 15, 0, 10, 0, 15, 0, 11, 0, 16, 0, 17, 0, 13, 0, 16, 0, 13, 0, 12, 0, 13, 0, 17, 0, 18, 0, 13, 0, 18, 0, 14, 0, 18, 0, 19, 0, 15, 0, 18, 0, 15, 0, 14, 0, 16, 0, 20, 0, 21, 0, 16, 0, 21, 0, 17, 0, 21, 0, 22, 0, 18, 0, 21, 0, 18, 0, 17, 0, 18, 0, 22, 0, 23, 0, 18, 0, 23, 0, 19, 0, 204, 204, 76, 191, 205, 204, 76, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 170, 208, 128, 63, 22, 73, 126, 63, 204, 204, 76, 191, 143, 194, 245, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 148, 151, 128, 63, 96, 22, 75, 63, 41, 92, 143, 190, 143, 194, 245, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 200, 252, 45, 63, 234, 207, 75, 63, 41, 92, 143, 190, 205, 204, 76, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 244, 110, 46, 63, 162, 2, 127, 63, 143, 194, 117, 62, 143, 194, 245, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 190, 148, 181, 62, 116, 137, 76, 63, 143, 194, 117, 62, 205, 204, 76, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 28, 121, 182, 62, 44, 188, 127, 63, 204, 204, 76, 63, 143, 194, 245, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 212, 152, 59, 72, 81, 77, 63, 204, 204, 76, 63, 205, 204, 76, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 128, 233, 209, 59, 252, 65, 128, 63, 204, 204, 76, 191, 10, 215, 35, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 124, 94, 128, 63, 172, 227, 23, 63, 41, 92, 143, 190, 10, 215, 35, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 152, 138, 45, 63, 58, 157, 24, 63, 143, 194, 117, 62, 10, 215, 35, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 104, 176, 180, 62, 196, 86, 25, 63, 204, 204, 76, 63, 10, 215, 35, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 125, 63, 59, 144, 30, 26, 63, 204, 204, 76, 191, 10, 215, 35, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 104, 37, 128, 63, 246, 97, 201, 62, 41, 92, 143, 190, 10, 215, 35, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 106, 24, 45, 63, 0, 213, 202, 62, 143, 194, 117, 62, 10, 215, 35, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 12, 204, 179, 62, 22, 72, 204, 62, 204, 204, 76, 63, 10, 215, 35, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 156, 154, 58, 184, 215, 205, 62, 204, 204, 76, 191, 143, 194, 245, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 160, 216, 127, 63, 24, 249, 69, 62, 41, 92, 143, 190, 143, 194, 245, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 60, 166, 44, 63, 64, 223, 72, 62, 143, 194, 117, 62, 143, 194, 245, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 184, 231, 178, 62, 104, 197, 75, 62, 204, 204, 76, 63, 143, 194, 245, 190, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 144, 19, 186, 168, 228, 78, 62, 204, 204, 76, 191, 205, 204, 76, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 114, 102, 127, 63, 64, 54, 218, 187, 41, 92, 143, 190, 205, 204, 76, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 14, 52, 44, 63, 0, 226, 122, 187, 143, 194, 117, 62, 205, 204, 76, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 90, 3, 178, 62, 0, 180, 130, 186, 204, 204, 76, 63, 205, 204, 76, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 13, 23, 187, 128, 117, 6, 59, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            byte[] VData = header.Concat(MaterialToBytes).Concat(meshheader).Concat(meshdata).ToArray();
            return VData;
        }

        static byte[] MakeVMeshRef(UInt32 MeshName)
        {
            byte[] NameToBytes = BitConverter.GetBytes(MeshName);
            byte[] headersize = new byte[] { 60, 0, 0, 0 };
            byte[] headerdata = new byte[] { 0, 0, 24, 0, 0, 0, 90, 0, 0, 0, 1, 0, 204, 204, 76, 63, 204, 204, 76, 191, 205, 204, 76, 63, 205, 204, 76, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 194, 208, 144, 63 };

            byte[] VRef = headersize.Concat(NameToBytes).Concat(headerdata).ToArray();
            return VRef;
        }
    }

    
}
