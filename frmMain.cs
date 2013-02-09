/*
 * lossy - A simple C# GUI for batch conversion of FLAC/OGG files to (lossy) MP3s.
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" (Revision 42):
 * <omerkilic@gmail.com> wrote this file. As long as you retain this notice you
 * can do whatever you want with this stuff. If we meet some day, and you think
 * this stuff is worth it, you can buy me a beer in return. Omer Kilic
 * ----------------------------------------------------------------------------
*/

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
using System.IO;

namespace lossy
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        // FIXME: Replace this with a Queue?
        ArrayList filesQueue = new ArrayList();

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
                string[] dropList = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var dropEntry in dropList)
                {
                    if (Directory.Exists(dropEntry))
                    {
                        foreach (string file in getFiles(dropEntry)) {
                            queueFile(file);
                        }
                    }
                    else
                    {
                        queueFile(dropEntry);
                    }
                }
            }
        }

        // Shamelessly stolen from: 
        // http://stackoverflow.com/questions/929276/how-to-recursively-list-all-the-files-in-a-directory-in-c
        static IEnumerable<string> getFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }

        private void queueFile(string file)
        {
            if ((Path.GetExtension(file).Equals(".flac")) ||
                (Path.GetExtension(file).Equals(".FLAC")) ||
                (Path.GetExtension(file).Equals(".ogg")) ||
                (Path.GetExtension(file).Equals(".OGG")))
            {
                if (!filesQueue.Contains(file))
                {
                    filesQueue.Add(file);
                    listBoxQueue.Items.Add(file.Substring(file.LastIndexOf('\\') + 1));
                }
            }
        }

        private string getOpt()
        {            
            // FIXME: Get opts from comboboxes.
            return "--preset insane";
        }

        private bool runLame(string opt, string inputFile)
        {
            var outputFile = Path.ChangeExtension(inputFile.Insert(inputFile.LastIndexOf('\\') + 1, "_MP3\\"), ".mp3");

            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            }

            Process p = new Process();
            p.StartInfo.FileName = "lame.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = opt + " \"" + inputFile + "\" " + " \"" + outputFile + " \"";

            try
            {
                p.Start();
                p.WaitForExit();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        private void clearStuff()
        {
            filesQueue.Clear();
            listBoxQueue.Items.Clear();
        }
        
        private void btnConvert_Click(object sender, EventArgs e)
        {
            string opt = getOpt();

            int i = 1, total = filesQueue.Count;

            if (total > 0)
            {
                progressBar1.Maximum = total + 1;
                progressBar1.Value = 1;

                foreach (var file in filesQueue)
                {
                    label3.Text = "Processing " + i + " of " + total;
                    if (runLame(opt, file.ToString()))
                    {
                        i++;
                        progressBar1.Value = i;
                    }
                    else
                    {
                        MessageBox.Show("Error processing: " + file.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
