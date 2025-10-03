
using SetupLib;
using System.Runtime.InteropServices;

namespace Setup
{
    public partial class Setup : Form
    {
        public Setup()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            startAsync();
        }

        AppUpdater appUpdater;
        string installLog = "";
        public async Task startAsync()
        {
            try
            {
                appUpdater = new AppUpdater("0");
                appUpdater.InstallStatusChanged += AppUpdater_InstallStatusChanged;
                var a = await appUpdater.CheckForUpdates();

                label10.Text = appUpdater.version;
                label9.Text = appUpdater.Name;
                whatsNews.Text = appUpdater.Tit;
                label7.Text = Math.Round((float)appUpdater.sizeFile / 1024 / 1024, 2).ToString() + " Мб";
                label10.Text = appUpdater.version;
                button1.Enabled = true;
                progressBar1.Style = ProgressBarStyle.Blocks;
                if (appUpdater._currentReleaseInfo.Assets.ContainsKey(PackageType.MSIX))
                {
                    MSIXRadio.Enabled = true;
                    MSIXRadio.Checked = true;
                }
                if (appUpdater._currentReleaseInfo.Assets.ContainsKey(PackageType.ZIP))
                {
                    if (!MSIXRadio.Checked)
                    {
                        MSIXRadio.Checked = true;

                    }
                    EXERadio.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при проверке обновлений: " + ex.Message);
            }
        }

        private void AppUpdater_InstallStatusChanged(object sender, InstallStatusChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppUpdater_InstallStatusChanged(sender, e)));
                return;
            }
            installLog += e.Status + "\r\n";
            logTextBox.Text = installLog;
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.ScrollToCaret();
        }

        private void AppUpdater_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppUpdater_DownloadProgressChanged(sender, e)));
                return;
            }
            progressBar1.Value = (int)Math.Round(e.Percentage);
            label6.Text = Math.Round((float)e.BytesDownloaded / 1024 / 1024, 2).ToString() + " Мб";
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                MSIXRadio.Enabled = false;
                EXERadio.Enabled = false;

                bool a = appUpdater.IsVersionInstalled(RuntimeInformation.FrameworkDescription);

                if (!a)
                {
                    var result = MessageBox.Show(
                         $"Необходимо установить .NET версии минимум {RuntimeInformation.FrameworkDescription}",
                         "Установить?",
                         MessageBoxButtons.YesNo,
                         MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        bool winget_installed = appUpdater.CheckIfWingetIsInstalled();

                        if (winget_installed)
                        {
                            appUpdater.InstallLatestDotNetAppRuntime();
                        }
                        else
                        {
                            var resultw = MessageBox.Show(
                               $"Отсуствуют некоторые компоненты для автоматической установки .NET После установки приложение, .NET необходимо будет установить вручную."
                            );
                        }
                    }
                    else
                    {

                    }
                }

                appUpdater.DownloadProgressChanged += AppUpdater_DownloadProgressChanged;

                await appUpdater.DownloadAndOpenFile(forceInstall: checkBox1.Checked);



                //Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при установке: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://t.me/VK_M_creator",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Name == MSIXRadio.Name)
            {
                appUpdater.SelectedPackageType = PackageType.MSIX;
            }
            else
            {
                appUpdater.SelectedPackageType = PackageType.ZIP;
            }
            label7.Text = Math.Round((float)appUpdater.sizeFile / 1024 / 1024, 2).ToString() + " Мб";
        }
    }
}
