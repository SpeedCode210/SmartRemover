
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.Win32;


namespace SmartRemover
{
    
    public partial class MainWindow : Window
    {
        public string ApplicationName { get; set; }
        public static string TempDir;
        public MainWindow()
        {
            InitializeComponent();
            TempDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string js = System.IO.File.ReadAllText(TempDir + "\\package.json");
            Foo f = JsonConvert.DeserializeObject<Foo>(js);
            ApplicationName = f.Name;
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            TempDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string js = System.IO.File.ReadAllText(TempDir + "\\package.json");
            Foo f = JsonConvert.DeserializeObject<Foo>(js);
            DirectoryInfo dir = new DirectoryInfo(TempDir);

                if(dir.Exists)
                {
                    
                    foreach(FileInfo file in dir.GetFiles())
                    {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                    }
                    foreach(DirectoryInfo dirt in dir.GetDirectories())
                    {
                        dirt.Delete(true);
                    }
                }
               
            CreateShortcut(f.Name, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            CreateShortcut(f.Name, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            CreateShortcut(f.Name + " Uninstaller", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            var a = Registry.LocalMachine.OpenSubKey("SOFTWARE", true).OpenSubKey("Microsoft", true)
                .OpenSubKey("Windows", true).OpenSubKey("CurrentVersion", true).OpenSubKey("Uninstall", true);
            a.DeleteSubKey(ApplicationName);
            AutoDeleter.AutoDeleterStart();
            Application.Current.Shutdown();
        }

        
        private class Foo
        {
            public string Name { get;  set; }
            public string MainExe { get; set; }
            public string VersionName { get; set; }
            public int VersionCode { get; set; }
            public string Date { get; set; }
        }


        public static void CreateShortcut(string shortcutName, string shortcutPath)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            try
            { 
            File.Delete(shortcutLocation);
            }
            catch { }
        }

        
    }

    class AutoDeleter
    {
        public static bool WindowsIsClosing { get; protected set; }

        static AutoDeleter()
        {
            WindowsIsClosing = false;
            Microsoft.Win32.SystemEvents.SessionEnding += delegate
            {
                WindowsIsClosing = true;
            };
            Microsoft.Win32.SystemEvents.SessionEnded += delegate
            {
                WindowsIsClosing = true;
            };
        }

        static string GetTmpBatName()
        {
            const string model = "{0}\\DS_Unist{1}.bat";

            string WinTmpDir = Path.GetTempPath();
            int nm = 0;
            while(File.Exists(String.Format(model, WinTmpDir, nm.ToString())))
            {
                nm++;
            }

            return String.Format(model, WinTmpDir, nm.ToString());
        }

        [Flags]
        public enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 0x00000001,
            MOVEFILE_COPY_ALLOWED = 0x00000002,
            MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,
            MOVEFILE_WRITE_THROUGH = 0x00000008,
            MOVEFILE_CREATE_HARDLINK = 0x00000010,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName,
           MoveFileFlags dwFlags);

        public static void AutoDeleterStart()
        {
            string file_name = Assembly.GetEntryAssembly().Location;

            if(WindowsIsClosing)
            {
                /* Si Windows est en train de se fermer, on va se supprimer au prochain redémarrage */
                MoveFileEx(file_name, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
            }
            else
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(MainWindow.TempDir);

                FileStream fi = null;

                int try_count = 20;
                do
                {
                    try
                    { fi = new FileStream(GetTmpBatName(), FileMode.Create); }
                    catch { }
                    try_count--;

                    System.Threading.Thread.Sleep(1000);
                }
                while(try_count >= 0 && fi == null);

                if(try_count < 0)
                {
                   
                    return;
                }

                if(fi == null)
                    fi = new FileStream(GetTmpBatName(), FileMode.Create); /* erreur possible, si le do a échoué ... */

                string name = fi.Name;

                StreamWriter wr = new StreamWriter(fi, Encoding.Default);
                wr.WriteLine("@echo off");
                wr.WriteLine("REM DreamShield.IO.Utils AutoDeleter Bat");
                wr.WriteLine("REM BatGenerator v 1.0");
                wr.WriteLine();

                wr.WriteLine(":del_process");
                wr.WriteLine(String.Format("@if exist \"{0}\" del \"{0}\"", file_name));
                wr.WriteLine(String.Format("@if exist \"{0}\" goto del_process", file_name));
                wr.WriteLine();

                wr.WriteLine(String.Format("del \"{0}\"", name));

                wr.Flush();
                fi.Flush();
                fi.Close();

                ProcessStartInfo info = new ProcessStartInfo();
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.FileName = name;
                Process.Start(info);
            }
        }
    }
}
