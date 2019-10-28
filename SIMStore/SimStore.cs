using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SIMStore
{
    public partial class SimStore : Form
    {
        private SimReader _SimReader = new SimReader();
        private OpenFileDialog _OpenFileDialog;
        private FolderBrowserDialog _SaveFileDialog;

        public SimStore()
        {
            InitializeComponent();
            _OpenFileDialog = new OpenFileDialog();
            _OpenFileDialog.Title = "Select File to Save on SIM";
            _OpenFileDialog.CheckFileExists = true;
            _SaveFileDialog = new FolderBrowserDialog();
        }

        private string DefaultDirectory
        {
            get {
                if (!File.Exists("default_dir.txt"))
                {
                    File.WriteAllText("default_dir.txt", Directory.GetCurrentDirectory());
                    return Directory.GetCurrentDirectory();
                }
                else
                {
                    return File.ReadAllText("default_dir.txt");
                }
            }
            set
            {
                File.WriteAllText("default_dir.txt", value);
            }
        }

        private void SimStore_Load(object sender, EventArgs e)
        {
            List<string> readers = _SimReader.Readers;
            if (readers.Count > 0)
            {
                BindingSource bs = new BindingSource
                {
                    DataSource = readers
                };
                cmbCardReaders.DataSource = bs;
            }
            else
            {
                var result = DialogResult.Retry;
                while (result == DialogResult.Retry)
                {
                    result = MessageBox.Show("No card readers were found", "No card readers found", MessageBoxButtons.RetryCancel);
                    if (result == DialogResult.Retry)
                    {
                        _SimReader = new SimReader();
                        System.Threading.Thread.Sleep(100);
                        if (readers.Count > 0)
                        {
                            BindingSource bs = new BindingSource
                            {
                                DataSource = readers
                            };
                            cmbCardReaders.DataSource = bs;
                            result = DialogResult.OK;
                        }
                    }
                }
            }

            var reader = readers.Count > 0 ? readers[0] : string.Empty; 
            if (reader.Length > 0)
            {
                _SimReader.SelectedReader = reader;
                if (!_SimReader.AnswerToReset() || !_SimReader.ReadADN())
                {
                    MessageBox.Show("Failed to read ADN file from SIM card");
                    Application.Exit();
                }
            }
        }

        private void BtnDownloadFileFromSIM_Click(object sender, EventArgs e)
        {

            string fileName = string.Empty;
            byte[] fileBytes = null;

            Enabled = false;
            if (_SimReader.ReadFile(ref fileName, ref fileBytes))
            {
                
                _SaveFileDialog.SelectedPath = DefaultDirectory;
                if (_SaveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderName = _SaveFileDialog.SelectedPath;
                    string saveFile = Path.Combine(folderName, fileName);
                    if (File.Exists(saveFile))
                    {
                        if (MessageBox.Show("Overwrite existing file?", "Overwrite " + saveFile, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            File.WriteAllBytes(saveFile, fileBytes);
                        } 
                    } else
                    {
                        File.WriteAllBytes(saveFile, fileBytes);
                    }
                    MessageBox.Show("Success!");
                }

            } else
            {
                MessageBox.Show("Failed to read SIM file");
            }
            Enabled = true;
        }

        private void BtnUploadFileToSIM_Click(object sender, EventArgs e)
        {
            _OpenFileDialog.InitialDirectory = DefaultDirectory;
            if (_OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                var file = _OpenFileDialog.FileName;
                DefaultDirectory = Path.GetDirectoryName(file);

                var fn = Path.GetFileName(file);
                if (new FileInfo(file).Length >= _SimReader.GetMaxFileLength(fn))
                {
                    MessageBox.Show("File size must less than " + _SimReader.GetMaxFileLength(fn) + " bytes");
                } else
                {
                    byte[] fileBytes = File.ReadAllBytes(file);
                    if (_SimReader.WriteFile(fn, fileBytes)) {
                        MessageBox.Show("Success!");
                    } else
                    {
                        MessageBox.Show("Failed to write file");
                    }
                }
            }
        }

        private void CmbCardReaders_SelectedIndexChanged(object sender, EventArgs e)
        {
            var reader = cmbCardReaders.SelectedText;
            if (reader.Length > 0)
            {
                _SimReader.SelectedReader = reader;
                if (!_SimReader.AnswerToReset() || !_SimReader.ReadADN())
                {
                    MessageBox.Show("Failed to read ADN file from SIM card");
                    Application.Exit();
                }
            }
        }
    }
}