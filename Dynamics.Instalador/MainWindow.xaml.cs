using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dynamics.Instalador
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Grid> grids = new List<Grid>();
        private int indexPage = 0;
        private string initialRoute = string.Empty;
        private WebClient webClient;
        private WebClient dynamicClient;
        private string URL_OLEB = "https://download.microsoft.com/download/b/f/b/bfbfa4b8-7f91-4649-8dab-9a6476360365/VFPOLEDBSetup.msi";
        private string URL_DYNAMICS = "http://localhost:3000/sdapp-v1.0.0.1.zip";
        private string TEMP_OLEB = string.Empty;
        private string TEMP_DYNAMICS = string.Empty;
        private string DYNAMICS_ROUTE = string.Empty;
        private string DIRECTORY_ROUTE = string.Empty;
        private string LAST_VERSION = string.Empty;
        private string DB_ROUTE = string.Empty;
        private string BRANCH = string.Empty;
        private bool OLEB_CREATED = false;
        public MainWindow()
        {
            InitializeComponent();

            AddingPages();
            Buttons();
        }

        private void Buttons()
        {
            switch (indexPage)
            {
                case 0:
                    btnBack.IsEnabled = false;
                    break;
                case 1:
                    btnBack.IsEnabled = true;
                    break;
                case 2:
                    btnBack.IsEnabled = false;
                    break;
                case 4:
                    btnNext.IsEnabled = false;
                    btnsaveconfig.IsEnabled = false;
                    break;

                case 5:
                    btnNext.Content = "Finalizar";
                    break;
            }
        }

        private void AddingPages()
        {
            grids.Add(Grid1);
            grids.Add(Grid2);
            grids.Add(Grid3);
            grids.Add(Grid4);
            grids.Add(Grid5);
            grids.Add(Grid6);
        }

        // Methods Next Button
        private void NextPage()
        {
            foreach (var grid in grids)
            {
                grid.Visibility = Visibility.Hidden;
            } 
            
            if(btnNext.Content.Equals("Finalizar"))
            {
                Environment.Exit(0);
            }

            indexPage++;

            var newGrid = grids[indexPage];

            newGrid.Visibility = Visibility.Visible;
            Buttons();

            LogicPages();
        }

        private void BackPage()
        {
            foreach (var grid in grids)
            {
                grid.Visibility = Visibility.Hidden;
            }

            indexPage--;

            var newGrid = grids[indexPage];

            newGrid.Visibility = Visibility.Visible;
            Buttons();

            LogicPages();
        }

        private void LogicPages()
        {
            switch (indexPage)
            {
                case 1:
                    initialRoute = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    tbroute.Text = initialRoute;
                    break;
                case 2:
                    if (!VerifyRequirements())
                    {
                        DonwloadOled();
                    }
                    NextPage();
                    break;
                case 3:
                    if(CreatingDynamicDownload())
                        DownloadDynamics();
                    break;                

                default: break;
            }
        }

        private string GetVersion()
        {
            try
            {
                rtmessages.AppendText("Verificando ultima actualización");
                return new WebClient().DownloadString("http://localhost:3000/version.txt").Replace("\n", "");                
            }
            catch (Exception ex)
            {
                throw new Exception();
            }

            
        }

        private void DownloadDynamics()
        {
            // Download version file

            LAST_VERSION = $"sdapp-v{GetVersion()}.zip";


            // Download dynamics file

            TEMP_DYNAMICS = Path.Combine(Path.GetTempPath(), "IntegracionDynamics");

            if (Directory.Exists(TEMP_DYNAMICS)) DeleteFolder(TEMP_DYNAMICS);
            Directory.CreateDirectory(TEMP_DYNAMICS);

            TEMP_DYNAMICS = Path.Combine(TEMP_DYNAMICS, LAST_VERSION);
            _ = Path.Combine(URL_DYNAMICS, $"/{LAST_VERSION}");

            dynamicClient.DownloadFileAsync(new Uri(URL_DYNAMICS), TEMP_DYNAMICS);
            rtmessages.AppendText("\nDescargando Sincronizador de Dynamics");
        }

        private bool CreatingDynamicDownload()
        {
            try
            {
                dynamicClient = new WebClient();

                dynamicClient.DownloadProgressChanged += (s, e) =>
                {
                    pbInstaller.IsIndeterminate = false;
                    pbInstaller.Value = e.ProgressPercentage;
                };

                dynamicClient.DownloadFileCompleted += (s, e) =>
                {
                    if (!e.Cancelled)
                    {
                        if (Directory.Exists(DYNAMICS_ROUTE))
                            DeleteFolder(DYNAMICS_ROUTE);

                        Directory.CreateDirectory(DYNAMICS_ROUTE);


                        rtmessages.AppendText("\nExtranyendo Sincronizador de Dynamics");

                        ZipFile.ExtractToDirectory(TEMP_DYNAMICS, DYNAMICS_ROUTE);

                        rtmessages.AppendText("\nInstalación completa");

                        pbInstaller.Value = pbInstaller.Maximum;

                        if (pbInstaller.Value == pbInstaller.Maximum)
                            NextPage();

                    }
                };
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void DonwloadOled()
        {
            TEMP_OLEB = Path.Combine(Path.GetTempPath(), "Oleb_Installer");

            if (Directory.Exists(TEMP_OLEB)) DeleteFolder(TEMP_OLEB);
            Directory.CreateDirectory(TEMP_OLEB);

            TEMP_OLEB = Path.Combine(TEMP_OLEB, "VFPOLEDBSetup.msi");

            webClient.DownloadFileAsync(new Uri(URL_OLEB), TEMP_OLEB);
        }

        private bool VerifyRequirements()
        {
            string ruta = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if (!Directory.Exists($"{ruta}\\Microsoft Visual FoxPro OLE DB Provider\\"))
            {
                webClient = new WebClient(); 

                webClient.DownloadProgressChanged += (s, e) =>
                {
                    pboleb.IsIndeterminate = false;
                    pboleb.Value = e.ProgressPercentage;
                };

                webClient.DownloadFileCompleted += async (s, e) =>
                {
                    if (!e.Cancelled)
                    {
                        if (File.Exists(Path.Combine(DIRECTORY_ROUTE, "VFPOLEDBSetup.msi")))
                            File.Delete(Path.Combine(DIRECTORY_ROUTE, "VFPOLEDBSetup.msi"));

                        var destFile = Path.Combine(DIRECTORY_ROUTE, "VFPOLEDBSetup.msi");

                        File.Copy(TEMP_OLEB, destFile);

                        Process.Start("VFPOLEDBSetup.msi");

                        await VerifyOlebProvider();

                        pboleb.Value = pboleb.Maximum;
                        
                    }
                };

                return false;
            }

            return true;
        }

        private async Task<bool> VerifyOlebProvider()
        {
            string ruta = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            while (!OLEB_CREATED)
            {
                if (Directory.Exists($"{ruta}\\Microsoft Visual FoxPro OLE DB Provider\\"))
                {
                    OLEB_CREATED = true;
                }
            }

            return OLEB_CREATED;
        }
        // Dynamics file configuration

        private void AddingConfig()
        {
            pbconfig.Visibility = Visibility.Visible;
            BRANCH = tbbranch.Text;
            DIRECTORY_ROUTE = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();

            var sourceFile = Path.Combine(DYNAMICS_ROUTE, "InterfaceMD.Program.exe.config");
            var dirFile = Path.Combine(DIRECTORY_ROUTE, "InterfaceMD.Program.exe.config");
            
            File.Move(sourceFile, dirFile);

            
            ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = "InterfaceMD.Program.exe.config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("DirDBF");
            config.AppSettings.Settings.Add("DirDBF", DB_ROUTE);
            config.AppSettings.Settings.Remove("Sucursal");
            config.AppSettings.Settings.Add("Sucursal", BRANCH);
            config.AppSettings.Settings.Remove("Compras");
            config.AppSettings.Settings.Add("Compras", true.ToString());
            config.AppSettings.Settings.Remove("Ventas");
            config.AppSettings.Settings.Add("Ventas", true.ToString());
            config.AppSettings.Settings.Remove("VariosDias");
            config.AppSettings.Settings.Add("VariosDias", false.ToString());
            config.Save(ConfigurationSaveMode.Minimal);

            ConfigurationManager.RefreshSection("appSettings");

            File.Move(dirFile, sourceFile);



            pbconfig.Visibility = Visibility.Hidden;
            pbconfig.IsIndeterminate = false;
            pbconfig.Value = pbconfig.Maximum;

            btnNext.IsEnabled = true;
            NextPage();
        }

        // Events
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NextPage();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            dialog.ShowDialog();

            tbroute.Text = dialog.FileName;
            initialRoute = dialog.FileName;            

            DYNAMICS_ROUTE = Path.Combine(initialRoute, "IntegracionDynamics");         

        }

        private void DeleteFolder(string ruta)
        {
            foreach (string ar in Directory.GetFiles(ruta)) File.Delete(ar);
            foreach (string ca in Directory.GetDirectories(ruta)) DeleteFolder(ca);

            Directory.Delete(ruta);
        }

        private void tbbranch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbrouteDB.Text) && !string.IsNullOrEmpty(tbbranch.Text))
            {
                
                btnsaveconfig.IsEnabled = true;
            }
            else
            {
                
                btnsaveconfig.IsEnabled = false;
            }

        }

        private void btnroutedb_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            dialog.ShowDialog();

            var route = Path.Combine(dialog.FileName, " ");
            tbrouteDB.Text = route;
            DB_ROUTE = route;
        }

        private void btnsaveconfig_Click(object sender, RoutedEventArgs e)
        {            
            AddingConfig();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackPage();
        }
    }
}
