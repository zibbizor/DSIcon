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
                                //image.Clamp();
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
                                MaterialCount.Tag = new byte[] { 1, 0, 0, 0};

                                //The material itself
                                TreeNode Material = new TreeNode(shortname);
                                Material.Tag = AlleyUtils.StringToByte("");
                                //Material flags
                                TreeNode Material_Dt_flags = new TreeNode("Dt_flags");
                                Material_Dt_flags.Tag = new byte[] { 64, 0, 0, 0};
                                //Material texture name
                                TreeNode Material_Dt_name = new TreeNode("Dt_name");
                                Material_Dt_name.Tag = AlleyUtils.StringToByte(name);
                                //Material type
                                TreeNode Material_Type = new TreeNode("Type");
                                Material_Type.Tag = AlleyUtils.StringToByte("DcDt");

                                //Add Nodes to root                                                               
                                Material.Nodes.Add(Material_Type);
                                Material.Nodes.Add(Material_Dt_name);
                                Material.Nodes.Add(Material_Dt_flags);

                                MaterialLibrary.Nodes.Add(MaterialCount);
                                MaterialLibrary.Nodes.Add(Material);
                                                             
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
                                CMPRoot.Nodes.Add("Exporter Version", "Exporter Version");
                                CMPRoot.Nodes["Exporter Version"].Tag = AlleyUtils.StringToCMPString("Converted with DSIcon v0.1");

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

            byte[] header = new byte[] { 1, 0, 0, 0, 4, 0, 0, 0, 1, 0, 6, 0, 18, 1, 4, 0 };
            byte[] meshheader = new byte[] { 0, 0, 3, 0, 6, 0, 204, 0 };
            byte[] meshdata = new byte[] { 0, 0, 1, 0, 2, 0, 1, 0, 3, 0, 2, 0, 146, 128, 17, 61, 125, 143, 11, 189, 157, 0, 169, 179, 210, 229, 79, 38, 64, 41, 1, 178, 0, 0, 128, 63, 64, 223, 127, 63, 0, 246, 2, 58, 146, 128, 17, 61, 82, 143, 11, 61, 246, 243, 167, 179, 210, 229, 79, 38, 64, 41, 1, 178, 0, 0, 128, 63, 67, 223, 127, 63, 64, 223, 127, 63, 148, 128, 17, 189, 125, 143, 11, 189, 157, 13, 169, 179, 210, 229, 79, 38, 64, 41, 1, 178, 0, 0, 128, 63, 0, 244, 2, 58, 0, 10, 3, 58, 148, 128, 17, 189, 79, 143, 11, 61, 246, 243, 167, 179, 210, 229, 79, 38, 64, 41, 1, 178, 0, 0, 128, 63, 0, 2, 3, 58, 67, 223, 127, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


            byte[] VData = header.Concat(MaterialToBytes).Concat(meshheader).Concat(meshdata).ToArray();
            return VData;
        }

        static byte[] MakeVMeshRef(UInt32 MeshName)
        {
            byte[] NameToBytes = BitConverter.GetBytes(MeshName);

            byte[] headersize = new byte[] { 60, 0, 0, 0 };
            byte[] headerdata = new byte[] { 0, 0, 4, 0, 0, 0, 6, 0, 0, 0, 1, 0, 146, 128, 17, 61, 148, 128, 17, 189, 82, 143, 11, 61, 125, 143, 11, 189, 246, 243, 167, 179, 157, 13, 169, 179, 140, 168, 46, 58, 228, 95, 61, 187, 188, 140, 168, 179, 79, 225, 83, 61 };

            byte[] VRef = headersize.Concat(NameToBytes).Concat(headerdata).ToArray();
            return VRef;
        }
    }


}


