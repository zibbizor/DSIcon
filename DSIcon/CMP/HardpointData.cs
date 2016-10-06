using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UTFEditor
{
    public class HardpointData
    {
        public float PosX, PosY, PosZ;
        public float RotMatXX, RotMatXY, RotMatXZ;
        public float RotMatYX, RotMatYY, RotMatYZ;
        public float RotMatZX, RotMatZY, RotMatZZ;
        public float AxisX, AxisY, AxisZ;
        public float Min, Max;

        public string Name
        {
            get
            {
                return hardpoint.Name;
            }
        }

        public TreeNode Node
        {
            get
            {
                return hardpoint;
            }
        }

        TreeNode hardpoint;

        private TreeNode MakeNode(string name)
        {
            TreeNode n = new TreeNode(name);
            n.Name = n.Text;
            n.Tag = new byte[0];

            return n;
        }

        public HardpointData(string name, bool revolute)
        {
            hardpoint = MakeNode(name);

            hardpoint.Nodes.Add(MakeNode("Position"));
            hardpoint.Nodes.Add(MakeNode("Orientation"));

            if (revolute)
            {
                hardpoint.Nodes.Add(MakeNode("Axis"));
                hardpoint.Nodes.Add(MakeNode("Min"));
                hardpoint.Nodes.Add(MakeNode("Max"));
            }

            RotMatXX = RotMatYY = RotMatZZ = 1;

            Write();
        }

        public HardpointData(TreeNode hardpoint)
        {
            this.hardpoint = hardpoint;

            float[] data;

            data = Read("Position", 3);
            PosX = data[0];
            PosY = data[1];
            PosZ = data[2];

            data = Read("Orientation", 9);
            RotMatXX = data[0];
            RotMatXY = data[1];
            RotMatXZ = data[2];
            RotMatYX = data[3];
            RotMatYY = data[4];
            RotMatYZ = data[5];
            RotMatZX = data[6];
            RotMatZY = data[7];
            RotMatZZ = data[8];

            if (Utilities.StrIEq(hardpoint.Parent.Name, "Revolute"))
            {
                data = Read("Axis", 3);
                AxisX = data[0];
                AxisY = data[1];
                AxisZ = data[2];

                data = Read("Min", 1);
                Min = data[0];

                data = Read("Max", 1);
                Max = data[0];
            }
        }

        public void Write()
        {
            float[] data = new float[9];

            data[0] = PosX;
            data[1] = PosY;
            data[2] = PosZ;
            Write("Position", data, 3);

            data[0] = RotMatXX;
            data[1] = RotMatXY;
            data[2] = RotMatXZ;
            data[3] = RotMatYX;
            data[4] = RotMatYY;
            data[5] = RotMatYZ;
            data[6] = RotMatZX;
            data[7] = RotMatZY;
            data[8] = RotMatZZ;
            Write("Orientation", data, 9);

            if ((hardpoint.Parent == null && hardpoint.Nodes["Axis"] != null) || (hardpoint.Parent != null && Utilities.StrIEq(hardpoint.Parent.Name, "Revolute")))
            {
                data[0] = AxisX;
                data[1] = AxisY;
                data[2] = AxisZ;
                Write("Axis", data, 3);

                data[0] = Min;
                Write("Min", data, 1);

                data[0] = Max;
                Write("Max", data, 1);
            }
            else if (hardpoint.Parent != null && !Utilities.StrIEq(hardpoint.Parent.Name, "Revolute"))
            {
                if (hardpoint.Nodes["Axis"] != null) hardpoint.Nodes["Axis"].Remove();
                if (hardpoint.Nodes["Min"] != null) hardpoint.Nodes["Min"].Remove();
                if (hardpoint.Nodes["Max"] != null) hardpoint.Nodes["Max"].Remove();
            }
        }

        private void Write(string name, float[] val, int count)
        {
            try
            {
                TreeNode node = hardpoint.Nodes[name];
                if (node == null)
                    node = hardpoint.Nodes.Add(name, name);
                List<byte> data = new List<byte>(count * 4);
                for (int i = 0; i < count; ++i)
                    data.AddRange(BitConverter.GetBytes(val[i]));
                node.Tag = data.ToArray();
            }
            catch { }
        }

        private float[] Read(string name, int count)
        {
            float[] val = new float[count];
            try
            {
                TreeNode node = hardpoint.Nodes[name];
                byte[] data = node.Tag as byte[];
                int pos = 0;
                for (int i = 0; i < count; ++i)
                    val[i] = Utilities.GetFloat(data, ref pos);
            }
            catch { }
            return val;
        }
    };
}
