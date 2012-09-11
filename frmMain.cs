using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

namespace lossy
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        ArrayList files = new ArrayList();

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string f in fileNames)
                {
                    files.Add(f);
                    listBox1.Items.Add(f.Substring(f.LastIndexOf('\\') + 1));
                }

            }
        }

        private string getOpt()
        {            
            // FIXME: Get opts from comboboxes.
            return "--preset insane";
        }

        private bool runLame(string opt, string filename)
        {
            //MessageBox.Show("o: " + opt + "f: " + filename);

            Process p = new Process();
            p.StartInfo.FileName = "lame.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = opt + " \"" + filename + "\"";

            try
            {
                p.Start();
                p.WaitForExit();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // FIXME: Actually check if lame fails/succeeds.
            return true;

        }

        private void clearStuff()
        {
            files.Clear();
            listBox1.Items.Clear();
        }
        
        private void btnConvert_Click(object sender, EventArgs e)
        {
            string opt = getOpt();

            int i = 1, total = files.Count;

            if (total > 0)
            {
                progressBar1.Maximum = total;
                progressBar1.Value = 0;

                foreach (var f in files)
                {
                    label3.Text = "Processing " + i + " of " + total;
                    if (runLame(opt, f.ToString()))
                    {
                        progressBar1.Value = i;
                        i++;
                    }
                    else
                    {
                        MessageBox.Show("Error processing: " + f.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                clearStuff();

                progressBar1.Value = 0;
                label3.Text = "Done.";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clearStuff();
        }


    }
}
