using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public partial class CE
        {


            public CE()
            {

            }
          
            public bool DecryptTrainer(string filePath)
            {
                MemoryStream ms;
                BinaryReader br;

                string outFileName = "decrypted_trainer.xml";
                string outFilePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + outFileName;

                byte[] raw_data = File.ReadAllBytes(filePath);
                int size = raw_data.Length;
                byte ckey = 0xCE;

                for (int i = 2; i < size; i++)
                {
                    raw_data[i] = (byte)(raw_data[i] ^ raw_data[i - 2]);
                }

                for (int j = size - 2; j >= 0; j--)
                {
                    raw_data[j] = (byte)(raw_data[j] ^ raw_data[j + 1]);
                }

                for (int k = 0; k < size; k++)
                {
                    raw_data[k] = (byte)(raw_data[k] ^ ckey);
                    ++ckey;
                }

                ms = new MemoryStream(raw_data);
                br = new BinaryReader(ms);

                byte[] CE_MAGIC = br.ReadBytes(5);
                if (Encoding.ASCII.GetString(CE_MAGIC) == "CHEAT")
                {
                    //Yes we have valid output
                    ms.Seek(5, SeekOrigin.Begin);
                    byte[] out_data = DeCompress(br.ReadBytes((int)ms.Length - 5));
                    if (out_data.Length == 0)
                    {
                        MessageBox.Show("Failed to decompress!");
                        return false;
                    }
                    byte[] out_dataFinal = new byte[out_data.Length - 4]; //Don't include the length of the file
                    Array.Copy(out_data, 4, out_dataFinal, 0, out_dataFinal.Length);
                    File.WriteAllBytes(outFilePath, out_dataFinal); //Spit that shit out and save
                    return true;
                }
                else
                {
                    MessageBox.Show("Either decryption went wrong or the trainer is using the old compression method which I will not support");
                    return false;
                }
            }


            private static byte[] DeCompress(byte[] data)
            {
                using (var compressStream = new MemoryStream(data))
                using (var outStream = new MemoryStream())
                using (var compressor = new DeflateStream(compressStream, CompressionMode.Decompress))
                {
                    compressor.CopyTo(outStream);
                    compressor.Close();
                    return outStream.ToArray();
                }
            }


        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
			using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
			{
				openFileDialog.Filter = "CETRAINER (*.CETRAINER)|*.CETRAINER";
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
                    textBox1.Text = openFileDialog.FileName;

				}
			}
		}

        private void button2_Click(object sender, EventArgs e)
        {
            CE ce = new CE();
            if (File.Exists(textBox1.Text))
            {
                ce.DecryptTrainer(textBox1.Text);
                MessageBox.Show("Done!");
            }
            else
            {
                MessageBox.Show("File doesnt exists");
            }
        }
    }
}
