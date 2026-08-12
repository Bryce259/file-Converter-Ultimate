using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileConverterUltimateApp
{
    public sealed class MainForm : Form
    {
        private static readonly string DefaultConvertedFilesFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "File Converter Ultimate Output");

        private readonly ListBox _conversionListBox = new ListBox();
        private readonly TextBox _inputTextBox = new TextBox();
        private readonly Button _browseInputButton = new Button();
        private readonly TextBox _outputTextBox = new TextBox();
        private readonly Button _browseOutputButton = new Button();
        private readonly Button _openOutputButton = new Button();
        private readonly ComboBox _backgroundComboBox = new ComboBox();
        private readonly TextBox _backgroundImageTextBox = new TextBox();
        private readonly Button _browseBackgroundButton = new Button();
        private readonly Label _backgroundLabel = new Label();
        private readonly Label _backgroundImageLabel = new Label();
        private readonly Button _convertButton = new Button();
        private readonly TextBox _logTextBox = new TextBox();
        private readonly Label _statusLabel = new Label();
        private readonly ConversionService _conversionService = new ConversionService();
        private readonly List<ConversionOption> _options = ConversionService.GetOptions();

        public MainForm()
        {
            Text = "File Converter Ultimate";
            Width = 920;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new System.Drawing.Size(860, 640);

            Font = new System.Drawing.Font("Segoe UI", 10F);

            Label titleLabel = new Label
            {
                Text = "File Converter Ultimate",
                Left = 20,
                Top = 18,
                Width = 360,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold)
            };

            Label helpLabel = new Label
            {
                Left = 20,
                Top = 52,
                Width = 840,
                Height = 48,
                Text = "Choose a conversion, select a file or ZIP archive, then choose where the results should go. Background options appear automatically when the selected conversion creates a shareable MP4."
            };

            Label conversionLabel = new Label { Left = 20, Top = 118, Width = 220, Text = "Conversion option" };
            _conversionListBox.Left = 20;
            _conversionListBox.Top = 142;
            _conversionListBox.Width = 560;
            _conversionListBox.Height = 104;
            _conversionListBox.AccessibleName = "Conversion options";
            _conversionListBox.AccessibleDescription = "Use the up and down arrow keys to review the available conversion choices.";
            _conversionListBox.DataSource = _options;
            _conversionListBox.SelectedIndexChanged += ConversionComboBox_SelectedIndexChanged;
            _conversionListBox.TabIndex = 0;
            _conversionListBox.IntegralHeight = false;

            Label inputLabel = new Label { Left = 20, Top = 266, Width = 240, Text = "Input file or ZIP archive" };
            _inputTextBox.Left = 20;
            _inputTextBox.Top = 290;
            _inputTextBox.Width = 680;
            _inputTextBox.AccessibleName = "Input path";
            _inputTextBox.TabIndex = 1;
            _browseInputButton.Left = 720;
            _browseInputButton.Top = 288;
            _browseInputButton.Width = 120;
            _browseInputButton.Text = "Browse input";
            _browseInputButton.Click += BrowseInputButton_Click;
            _browseInputButton.TabIndex = 2;

            Label outputLabel = new Label { Left = 20, Top = 336, Width = 220, Text = "Output folder" };
            _outputTextBox.Left = 20;
            _outputTextBox.Top = 360;
            _outputTextBox.Width = 540;
            _outputTextBox.Text = DefaultConvertedFilesFolder;
            _outputTextBox.TabIndex = 3;
            _browseOutputButton.Left = 720;
            _browseOutputButton.Top = 358;
            _browseOutputButton.Width = 120;
            _browseOutputButton.Text = "Browse folder";
            _browseOutputButton.Click += BrowseOutputButton_Click;
            _browseOutputButton.TabIndex = 5;

            _openOutputButton.Left = 580;
            _openOutputButton.Top = 358;
            _openOutputButton.Width = 120;
            _openOutputButton.Text = "Open folder";
            _openOutputButton.Click += OpenOutputButton_Click;
            _openOutputButton.TabIndex = 4;

            _backgroundLabel.Left = 20;
            _backgroundLabel.Top = 410;
            _backgroundLabel.Width = 240;
            _backgroundLabel.Text = "Background for MP4 video";

            _backgroundComboBox.Left = 20;
            _backgroundComboBox.Top = 434;
            _backgroundComboBox.Width = 300;
            _backgroundComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _backgroundComboBox.Items.AddRange(new object[]
            {
                "None",
                "Effects",
                "Image"
            });
            _backgroundComboBox.SelectedIndex = 0;
            _backgroundComboBox.SelectedIndexChanged += BackgroundComboBox_SelectedIndexChanged;
            _backgroundComboBox.TabIndex = 6;

            _backgroundImageLabel.Left = 340;
            _backgroundImageLabel.Top = 410;
            _backgroundImageLabel.Width = 220;
            _backgroundImageLabel.Text = "Background image";

            _backgroundImageTextBox.Left = 340;
            _backgroundImageTextBox.Top = 434;
            _backgroundImageTextBox.Width = 360;
            _backgroundImageTextBox.TabIndex = 7;

            _browseBackgroundButton.Left = 720;
            _browseBackgroundButton.Top = 432;
            _browseBackgroundButton.Width = 120;
            _browseBackgroundButton.Text = "Browse image";
            _browseBackgroundButton.Click += BrowseBackgroundButton_Click;
            _browseBackgroundButton.TabIndex = 8;

            _convertButton.Left = 20;
            _convertButton.Top = 492;
            _convertButton.Width = 180;
            _convertButton.Height = 36;
            _convertButton.Text = "Convert";
            _convertButton.Click += ConvertButton_Click;
            _convertButton.TabIndex = 9;

            _statusLabel.Left = 220;
            _statusLabel.Top = 500;
            _statusLabel.Width = 620;
            _statusLabel.Text = "Ready";

            Label logLabel = new Label { Left = 20, Top = 550, Width = 200, Text = "Status log" };
            _logTextBox.Left = 20;
            _logTextBox.Top = 574;
            _logTextBox.Width = 820;
            _logTextBox.Height = 72;
            _logTextBox.Multiline = true;
            _logTextBox.ScrollBars = ScrollBars.Vertical;
            _logTextBox.ReadOnly = true;
            _logTextBox.TabIndex = 10;

            Controls.Add(titleLabel);
            Controls.Add(helpLabel);
            Controls.Add(conversionLabel);
            Controls.Add(_conversionListBox);
            Controls.Add(inputLabel);
            Controls.Add(_inputTextBox);
            Controls.Add(_browseInputButton);
            Controls.Add(outputLabel);
            Controls.Add(_outputTextBox);
            Controls.Add(_openOutputButton);
            Controls.Add(_browseOutputButton);
            Controls.Add(_backgroundLabel);
            Controls.Add(_backgroundComboBox);
            Controls.Add(_backgroundImageLabel);
            Controls.Add(_backgroundImageTextBox);
            Controls.Add(_browseBackgroundButton);
            Controls.Add(_convertButton);
            Controls.Add(_statusLabel);
            Controls.Add(logLabel);
            Controls.Add(_logTextBox);

            EnsureDefaultOutputFolder();
            ConversionComboBox_SelectedIndexChanged(this, EventArgs.Empty);
            Shown += MainForm_Shown;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            _conversionListBox.Focus();
        }

        private void BrowseInputButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose a file or ZIP archive";
                dialog.Filter = "Supported input files (*.zip;*.*)|*.zip;*.*|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _inputTextBox.Text = dialog.FileName;
                }
            }
        }

        private void BrowseOutputButton_Click(object sender, EventArgs e)
        {
            EnsureDefaultOutputFolder();
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Choose where converted files should be saved.";
                dialog.SelectedPath = _outputTextBox.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _outputTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void OpenOutputButton_Click(object sender, EventArgs e)
        {
            string outputFolder = string.IsNullOrWhiteSpace(_outputTextBox.Text)
                ? DefaultConvertedFilesFolder
                : _outputTextBox.Text.Trim();

            Directory.CreateDirectory(outputFolder);
            System.Diagnostics.Process.Start(outputFolder);
        }

        private void BrowseBackgroundButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose a background image";
                dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _backgroundImageTextBox.Text = dialog.FileName;
                }
            }
        }

        private void ConversionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConversionOption option = _conversionListBox.SelectedItem as ConversionOption;
            bool showBackground = option != null && option.AllowsVideoBackground;

            _backgroundLabel.Visible = showBackground;
            _backgroundComboBox.Visible = showBackground;
            _backgroundImageLabel.Visible = showBackground;
            _backgroundImageTextBox.Visible = showBackground;
            _browseBackgroundButton.Visible = showBackground;

            BackgroundComboBox_SelectedIndexChanged(sender, e);
        }

        private void BackgroundComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool needsImage = string.Equals(Convert.ToString(_backgroundComboBox.SelectedItem), "Image", StringComparison.OrdinalIgnoreCase);
            _backgroundImageLabel.Enabled = needsImage;
            _backgroundImageTextBox.Enabled = needsImage;
            _browseBackgroundButton.Enabled = needsImage;
        }

        private async void ConvertButton_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateForm();
                ToggleUi(false);
                AppendLog("Starting conversion.");
                _statusLabel.Text = "Converting";

                ConversionRequest request = new ConversionRequest
                {
                    Option = (ConversionOption)_conversionListBox.SelectedItem,
                    InputPath = _inputTextBox.Text.Trim(),
                    OutputDirectory = _outputTextBox.Text.Trim(),
                    IsZipBatch = string.Equals(Path.GetExtension(_inputTextBox.Text.Trim()), ".zip", StringComparison.OrdinalIgnoreCase),
                    BackgroundMode = Convert.ToString(_backgroundComboBox.SelectedItem).ToLowerInvariant(),
                    BackgroundImagePath = _backgroundImageTextBox.Text.Trim()
                };

                Directory.CreateDirectory(request.OutputDirectory);
                List<ConversionResult> results = await Task.Run(() => _conversionService.Convert(request, AppendLog));
                int successCount = results.Count(result => result.Success);
                int failCount = results.Count(result => !result.Success && !result.Message.StartsWith("Skipped", StringComparison.OrdinalIgnoreCase));
                int skippedCount = results.Count(result => !result.Success && result.Message.StartsWith("Skipped", StringComparison.OrdinalIgnoreCase));

                foreach (ConversionResult result in results)
                {
                    AppendLog((result.Success ? "Success: " : "Failed: ") + Path.GetFileName(result.InputFile) + " - " + result.Message);
                }

                _statusLabel.Text = "Finished. Success: " + successCount + ". Failed: " + failCount + ".";
                MessageBox.Show(
                    this,
                    "Conversion complete.\r\n\r\nSuccess: " + successCount +
                    "\r\nSkipped: " + skippedCount +
                    "\r\nFailed: " + failCount +
                    "\r\nSaved to:\r\n" + request.OutputDirectory,
                    "File Converter Ultimate",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Conversion failed";
                AppendLog("Error: " + ex.Message);
                MessageBox.Show(this, ex.Message, "File Converter Ultimate", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true);
            }
        }

        private void ValidateForm()
        {
            if (_conversionListBox.SelectedItem == null)
            {
                throw new InvalidOperationException("Choose a conversion option.");
            }

            if (string.IsNullOrWhiteSpace(_inputTextBox.Text) || !File.Exists(_inputTextBox.Text))
            {
                throw new InvalidOperationException("Choose an input file.");
            }

            if (string.IsNullOrWhiteSpace(_outputTextBox.Text))
            {
                throw new InvalidOperationException("Choose an output folder.");
            }

            if (_backgroundComboBox.Visible &&
                string.Equals(Convert.ToString(_backgroundComboBox.SelectedItem), "Image", StringComparison.OrdinalIgnoreCase) &&
                !File.Exists(_backgroundImageTextBox.Text))
            {
                throw new InvalidOperationException("Choose a background image.");
            }
        }

        private void ToggleUi(bool ready)
        {
            _conversionListBox.Enabled = ready;
            _inputTextBox.Enabled = ready;
            _browseInputButton.Enabled = ready;
            _outputTextBox.Enabled = ready;
            _openOutputButton.Enabled = ready;
            _browseOutputButton.Enabled = ready;
            _backgroundComboBox.Enabled = ready;
            _convertButton.Enabled = ready;
        }

        private void EnsureDefaultOutputFolder()
        {
            Directory.CreateDirectory(DefaultConvertedFilesFolder);
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            string line = "[" + DateTime.Now.ToString("T") + "] " + message;
            if (_logTextBox.TextLength == 0)
            {
                _logTextBox.Text = line;
            }
            else
            {
                _logTextBox.AppendText(Environment.NewLine + line);
            }

            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.ScrollToCaret();
        }
    }
}
