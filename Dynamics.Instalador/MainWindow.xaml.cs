using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.AccessControl;
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
        private string URL_OLEB = "https://download.microsoft.com/download/b/f/b/bfbfa4b8-7f91-4649-8dab-9a6476360365/VFPOLEDBSetup.msi";
        private string TEMP_OLEB = string.Empty;
        private string DYNAMICS_ROUTE = string.Empty;
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
            }
        }

        private void AddingPages()
        {
            grids.Add(Grid1);
            grids.Add(Grid2);
            grids.Add(Grid3);
        }

        // Methods Next Button
        private void NextPage()
        {
            foreach (var grid in grids)
            {
                grid.Visibility = Visibility.Hidden;
            }

            indexPage++;

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
                    break;

                default: break;
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

                webClient.DownloadFileCompleted += (s, e) =>
                {
                    if (!e.Cancelled)
                    {
                        if (Directory.Exists(DYNAMICS_ROUTE))
                            DeleteFolder(DYNAMICS_ROUTE);

                        Directory.CreateDirectory(DYNAMICS_ROUTE);

                        var destFile = Path.Combine(DYNAMICS_ROUTE, "VFPOLEDBSetup.msi");

                        File.Copy(TEMP_OLEB, destFile);

                        pboleb.Value = pboleb.Maximum;
                        
                    }
                };

                return false;
            }

            return true;
        }





        // Function next button 
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


            //

            DYNAMICS_ROUTE = Path.Combine(initialRoute, "IntegracionDynamics");            


        }


        private void DeleteFolder(string ruta)
        {
            foreach (string ar in Directory.GetFiles(ruta)) File.Delete(ar);
            foreach (string ca in Directory.GetDirectories(ruta)) DeleteFolder(ca);

            Directory.Delete(ruta);
        }
    }
}
