using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCompare
{
    public partial class MainForm : Form
    {
        private delegate void DelegateSetFormEnable(bool isEnable);
        private delegate void DelegateSetList();

        private ConcurrentDictionary<string, (long size, string hash)> fileHashes1;
        private ConcurrentDictionary<string, (long size, string hash)> fileHashes2;

        private List<string> fileName1;
        private List<string> fileName2;

        public MainForm()
        {
            InitializeComponent();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            panel1.Visible = false;
            txtPath1.Text = @"H:\";
            txtPath2.Text = @"I:\";
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Process was cancelled", "Process Cancelled");
            }
            else
            {
                SetListInTask();
            }
            SetFormEnableInTask(true);
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SetFormEnableInTask(false);
            string folderPath1 = @txtPath1.Text;
            string folderPath2 = @txtPath2.Text;

            fileHashes1 = GetFileHashes(folderPath1);
            fileHashes2 = GetFileHashes(folderPath2);

            foreach (var file1 in fileHashes1)
            {
                foreach (var file2 in fileHashes2)
                {
                    if (file1.Value == file2.Value)
                    {
                        if (!fileName1.Contains(file1.Key)) fileName1.Add(file1.Key);
                        if (!fileName2.Contains(file2.Key)) fileName2.Add(file2.Key);
                    }
                }
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            fileName1 = new List<string>();
            fileName2 = new List<string>();
            if (bgWorker.IsBusy != true)
            {
                bgWorker.RunWorkerAsync();
            }
        }

        private void SetFormEnableInTask(bool isEnable)
        {
            try
            {
                DelegateSetFormEnable delegateSetFormEnable = new DelegateSetFormEnable(SetFormEnable);
                this.Invoke(delegateSetFormEnable, isEnable);
            }
            catch (Exception)
            {
            }
        }

        private void SetFormEnable(bool isEnable)
        {
            foreach (Control control in Controls)
            {
                if (control != panel1)
                {
                    control.Enabled = isEnable;
                }
            }
            panel1.Visible = !isEnable;
        }

        private void SetListInTask()
        {
            try
            {
                DelegateSetList delegateSetList = new DelegateSetList(SetList);
                this.Invoke(delegateSetList);
            }
            catch (Exception)
            {
            }
        }

        private void SetList()
        {
            listView1.Items.Clear();
            listView1.Items.AddRange(fileName1.ConvertAll(x => new ListViewItem(x)).ToArray());
            listView2.Items.Clear();
            listView2.Items.AddRange(fileName2.ConvertAll(x => new ListViewItem(x)).ToArray());
        }


        private ConcurrentDictionary<string, (long size, string hash)> GetFileHashes(string folderPath)
        {
            var fileHashes = new ConcurrentDictionary<string, (long size, string hash)>();
            var fileList = new List<string>();

            try
            {
                foreach (var file in EnumerateFilesSafe(folderPath))
                {
                    fileList.Add(file);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to folder: {ex.Message}");
            }

            Parallel.ForEach(fileList, filePath =>
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    string hash = ComputeMD5(filePath);
                    fileHashes[filePath] = (fileInfo.Length, hash);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Access denied to file: {filePath}");
                }
            });

            return fileHashes;
        }

        private IEnumerable<string> EnumerateFilesSafe(string path)
        {
            var files = new List<string>();
            try
            {
                files.AddRange(Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly));
                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    files.AddRange(EnumerateFilesSafe(dir));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get folder: {path} Error:{ex.Message}");
            }
            return files;
        }

        private string ComputeMD5(string filePath)
        {
            var md5 = HashLib.HashFactory.Crypto.CreateMD5();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBytes(buffer, 0, bytesRead);
                }
            }
            var hashBytes = md5.TransformFinal().GetBytes();
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                StringBuilder sb = new StringBuilder();
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    sb.AppendLine(item.Text);
                }
                Clipboard.SetDataObject(sb.ToString());
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                StringBuilder sb = new StringBuilder();
                foreach (ListViewItem item in listView2.SelectedItems)
                {
                    sb.AppendLine(item.Text);
                }
                Clipboard.SetDataObject(sb.ToString());
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in listView2.Items)
                {
                    item.Selected = true;
                }
            }
        }
    }
}
