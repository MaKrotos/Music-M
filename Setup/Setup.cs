
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
        public async Task startAsync()
        {

            appUpdater = new AppUpdater("0");
            var a = await appUpdater.CheckForUpdates();

            label10.Text = appUpdater.version;
            label9.Text = appUpdater.Name;
            label8.Text = appUpdater.Tit;
            label7.Text = Math.Round((float)appUpdater.sizeFile / 1024 / 1024, 2).ToString() + " Мб";
            label10.Text = appUpdater.version;
            button1.Enabled = true;

        }
        private void AppUpdater_DownloadProgressChanged(object sender, AppUpdater.DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = (int)Math.Round(e.Percentage);
            label6.Text = Math.Round((float)e.BytesDownloaded / 1024 / 1024, 2).ToString() + " Мб";
        }

        private async void button1_Click(object sender, EventArgs e)
        {


            button1.Enabled = false;

            bool a = appUpdater.IsVersionInstalled(RuntimeInformation.FrameworkDescription);

            if (!a)
            {
                var result = MessageBox.Show(
                 $"Необходимо установить .NET веерсии минимум {RuntimeInformation.FrameworkDescription}",
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
                    // Код, если пользователь выбрал "Нет"
                }
            }

            appUpdater.DownloadProgressChanged += AppUpdater_DownloadProgressChanged;



            await appUpdater.DownloadAndOpenFile();



            //Close();
        }


    }
}
