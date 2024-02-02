
using SetupLib;

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
            label7.Text = Math.Round((float)appUpdater.sizeFile / 1024 / 1024, 2).ToString() + " Ìב";
            label10.Text = appUpdater.version;
            button1.Enabled = true;

        }
        private void AppUpdater_DownloadProgressChanged(object sender, AppUpdater.DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = (int)Math.Round(e.Percentage);
            label6.Text = Math.Round((float)e.BytesDownloaded / 1024 / 1024, 2).ToString() + " Ìב";
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            appUpdater.DownloadProgressChanged += AppUpdater_DownloadProgressChanged;
            button1.Enabled = false;
            await appUpdater.DownloadAndOpenFile();

            Close();
        }

       
    }
}
