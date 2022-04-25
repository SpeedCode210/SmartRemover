
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Text.Json;
using Microsoft.Win32;
using System.Windows.Media;
namespace SmartRemover
{

    public partial class MainWindow : Window
    {
        public string ApplicationName { get; set; }
        public static string TempDir;

        //Brushes that changes with Light or Dark theme
        public Brush BackgroundColorBrush { get; private set; }
        public Brush ButtonBackgroundColorBrush { get; private set; }
        public Brush SecondBackgroundColorBrush { get; private set; }
        public Brush SeparatorColorBrush { get; private set; }
        public Brush ForegroundColorBrush { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            //Getting Temp directory in AppData
            TempDir = Path.GetDirectoryName(AppContext.BaseDirectory);
            //Reading package.json to get the app name
            string js = System.IO.File.ReadAllText(TempDir + "\\package.json");
            ProgramData f = JsonSerializer.Deserialize<ProgramData>(js);
            ApplicationName = f.Name;
            InitTheme();
        }

        //function that gets windows'default theme (Light theme for windows 8.1 and older)
        private void InitTheme()
        {
            bool AppsUseLightTheme = true;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null && key.GetValue("AppsUseLightTheme") != null)
                    {
                        Int64 value = Convert.ToInt64(key.GetValue("AppsUseLightTheme").ToString());
                        if (value == 0)
                            AppsUseLightTheme = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }

            if (AppsUseLightTheme)
            {
                this.BackgroundColorBrush = new SolidColorBrush(Color.FromRgb(249, 249, 249));
                this.ButtonBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(249, 249, 249));
                this.SeparatorColorBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229));
                this.SecondBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                this.ForegroundColorBrush = new SolidColorBrush(Color.FromRgb(16, 16, 16));
            }
            else
            {
                this.BackgroundColorBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                this.ButtonBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(52, 52, 52));
                this.SeparatorColorBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                this.SecondBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(39, 39, 39));
                this.ForegroundColorBrush = new SolidColorBrush(Color.FromRgb(250, 250, 250));
            }

            this.DataContext = this;
        }

        //Click event for close button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Click event for uninstall button
        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            TempDir = Path.GetDirectoryName(AppContext.BaseDirectory);
            string js = System.IO.File.ReadAllText(TempDir + "\\package.json");
            ProgramData f = JsonSerializer.Deserialize<ProgramData>(js);
            DirectoryInfo dir = new DirectoryInfo(TempDir);

            //Delete all application's files
            if (dir.Exists)
            {

                foreach (FileInfo file in dir.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }catch{ }
                }
                foreach (DirectoryInfo dirt in dir.GetDirectories())
                {
                    dirt.Delete(true);
                }
            }

            //Delete all shortcuts and registery keys of the app
            DeleteShortcut(f.Name, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            DeleteShortcut(f.Name, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            DeleteShortcut(f.Name + " Uninstaller", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            var a = Registry.LocalMachine.OpenSubKey("SOFTWARE", true).OpenSubKey("Microsoft", true)
                .OpenSubKey("Windows", true).OpenSubKey("CurrentVersion", true).OpenSubKey("Uninstall", true);
            a.DeleteSubKey(ApplicationName);
            AutoDeleter.AutoDeleterStart();
            Application.Current.Shutdown();
        }

        //Class of package.json
        private class ProgramData
        {
            public string Name { get; set; }
            public string MainExe { get; set; }
            public string VersionName { get; set; }
            public int VersionCode { get; set; }
            public string Date { get; set; }
        }

        //Function that deletes shortcut
        public static void DeleteShortcut(string shortcutName, string shortcutPath)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            try
            {
                File.Delete(shortcutLocation);
            }
            catch { }
        }

        //Events that drags the window
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }

    //Deleting the Uninstaller after closing
    class AutoDeleter
    {
        public static void AutoDeleterStart()
        {
            string batchCommands = string.Empty;
            string exeFileName = AppContext.BaseDirectory;

            batchCommands += "@ECHO OFF\n";                         // Do not show any output
            batchCommands += "ping 127.0.0.1 > nul\n";              // Wait approximately 4 seconds (so that the process is already terminated)
            batchCommands += "echo j | del /F ";                    // Delete the executeable
            batchCommands += exeFileName + "\\* \n";
            batchCommands += "rmdir /s /q \""+ exeFileName + "\"\n";
            batchCommands += "echo j | del deleteMyProgram.bat";    // Delete this bat file

            File.WriteAllText("deleteMyProgram.bat", batchCommands);

            Process.Start("deleteMyProgram.bat");
        }
    }
}
