﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.IO.FileInfo;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.IO.Packaging;

namespace DimsISOTweaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int PID = Global.PID;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MountISO(object sender, RoutedEventArgs e)
        {
            if(Global.PID != 0)
            {
                PS.WriteToProcessById(PID, "echo blaahblaah");
            }

            
            //Process.GetProcessById(PID).StandardInput.WriteLine();
            //var cmd = new PS(" /k color 9e");
            //cmd.Create("echo hello world!");
            //PS.WriteToProcessById(PID, "whoohoow i'm a goorooh");
            
            //new ReadStdOut().CreateProcess("cmd.exe", 
            //    " /c powershell -command \"Mount-DiskImage -ImagePath " + ISO.Text + "\"", 
            //    false);
        }

        private void dismountISO(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                "/c powershell.exe -Command \"Dismount-DiskImage -ImagePath " + ISO.Text + "\"",
                false);
        }

        public void CopyWIM(object sender, RoutedEventArgs e)
        {
            // out with the old dos code
            //new ReadStdOut().CreateProcess("cmd.exe",
            //    " /c md " + MountPoint.Text +
            //    " & md C:\\Mount\\Drivers & md " + MountPoint.Text +
            //    "\\MOUNTDIR & MD " + MountPoint.Text + "\\BootWIM",
            //    false);
            // and in with the new code..
                                        //the boolean means recursive delete = true
            if (Directory.Exists(MountPoint.Text)){ Directory.Delete(MountPoint.Text, true); }
            Directory.CreateDirectory(MountPoint.Text);
            Directory.CreateDirectory(MountPoint.Text + "\\Drivers");
            Directory.CreateDirectory(MountPoint.Text + "\\MOUNTDIR");
            Directory.CreateDirectory(MountPoint.Text + "\\BootWIM");
            DriveInfo[] drives = DriveInfo.GetDrives();
            for (int i = 0; i < drives.Count(); i++)
            {
                if (File.Exists(drives[i].Name + "Sources\\boot.wim"))
                {
                    //var FileHandle = File.OpenHandle(MountPoint.Text + "\\BootWIM\\boot.wim");
                    File.Copy(drives[i].Name + "Sources\\boot.wim", 
                            MountPoint.Text + "\\BootWIM\\boot.wim", true);
                    FileInfo fileInfo = new FileInfo(MountPoint.Text + "\\BootWIM\boot.wim");
                    if (fileInfo.IsReadOnly) { fileInfo.IsReadOnly = false; };
                }
            }
        }
        public void getWIMInfo(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c DISM.exe /Get-WimInfo /WimFile:" + MountPoint.Text + "\\BootWIM\\boot.wim", 
                false);
        }
        public void MountWIM(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c DISM /mount-wim /wimfile:" + MountPoint.Text + 
                "\\BootWIM\\boot.wim /index:" + Index.Text + 
                " /MountDir:" + MountPoint.Text + 
                "\\MOUNTDIR", true);
        }
        public void AddPEDrivers(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c dism /image:"+ MountPoint.Text + 
                "\\MOUNTDIR /Add-Driver /Driver:" + MountPoint.Text + 
                "\\Drivers /recurse", 
                false);
        }
        void SlipstreamKB(object sender, RoutedEventArgs e)
        {
            // this is how this works:
            // first deploy your default install.wim on a computer and let it perform windows update.
            // Just make sure you write down which KB-Nrs.msu's you install. then download the correct
            // update file from https://www.catalog.update.microsoft.com/home.aspx and slipstream it into
            // your installation source like i did here.. 
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c for /f \"usebackq\" %x in (`dir " + 
                MountPoint.Text+"\\*.msu /b`) do wusa "+
                MountPoint.Text+"\\%x /quiet /norestart", 
                false);
            new ReadStdOut().CreateProcess("cmd.exe",
            " /c echo for /f \"usebackq\" %x in (`dir " + 
            MountPoint.Text + 
            "\\*.msu /b`) do dism /Image:" + 
            MountPoint.Text + 
            "\\MOUNTDIR /Add-Package /PackagePath=%x", 
            false);
        }

        private void UnMountWIM(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
             " /c dism /unmount-wim /mountdir:" + 
             MountPoint.Text + 
             "\\MOUNTDIR /commit", 
             false);
        }

        private void about(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("how to update windows 10 ISO/Wim (By Dimi Bertolami)\r\n\r\ntopics:\r\n- extra drivers\r\n- slipstream microsoft kb-updates\r\n- extra executables\r\n- how to write a PE-Network script\r\n\r\nfrom dosbox: \r\n\r\ncreate some temporary directories:\r\n(Here i'll download all the hotfixes)\r\nMD C:\\Mount\r\n\r\n:: extra drivers go into this directory. They will be installed recursively\r\nMD C:\\Mount\\Drivers\r\n\r\n:: boot.wim from the windows ISO goes here\r\nMD C:\\Mount\\BootWIM\r\n\r\n:: this is our Mount-Target Directory (in order to mount a wim file this folder has to be empty)\r\nmd C:\\MOUNT\\MOUNTDIR\r\n\r\nTo Mount an ISO with powershell: \r\npowershell -Command \"Mount-DiskImage -ImagePath C:\\Users\\Admin\\Desktop\\dewSystems\\ISO\\Gandalf10PE.ISO\"\r\n\r\ndetect drive letter where iso is mounted and copy wim to bootWIM Folder\r\nFOR /D %x in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do if EXIST %x:\\sources\\boot.wim (copy %x:\\sources\\boot.wim C:\\Mount\\BootWIM)\r\n\r\ncheck what image index to use\r\ndism /Get-WimInfo /WimFile:C:\\Mount\\BootWIM\\boot.wim\r\n\r\nmount wim file into mount-directory\r\ndism /mount-wim /wimfile:C:\\Mount\\BootWIM\\boot.wim /index:1 /MountDir:C:\\Mount\\MOUNTDIR\r\n\r\nrecursively add drivers to your PE (you must mount wim file first): \r\ndism /image:C:\\Mount\\MOUNTDIR /Add-Driver /Driver:D:\\Drivers /recurse\r\n\r\ndownload necessary updates manually\r\nslipstream downloaded windows updates into your solution\r\nDism /Image:C:\\Mount\\MOUNTDIR /Add-Package /PackagePath=kb4456655.msu /LogPath=C:\\mount\\dism.log\r\n\r\n");
        }

        private void adksetup(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists("c:\\ADK")) { return; }
            new ReadStdOut().CreateProcess("cmd.exe",
             " /c Installers\\adksetup.exe /features optionid.deploymentTools /installpath c:\\ADK /Q", 
             false);
        }

        private void ADKPESetup(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists("C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment")) { return; }
            new ReadStdOut().CreateProcess("cmd.exe",
             " /c Installers\\adkwinpesetup.exe /features + /installpath c:\\ADK /Q", 
             false);
        }

        private void addCabs(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                "/c pushd \"C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment\\amd64\\WinPE_OCs\" & dism /Image:" +
                MountPoint.Text + 
                "\\MOUNTDIR /Add-Package /PackagePath:\"C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment\\amd64\\WinPE_OCs\\",
                false);
            new ReadStdOut().CreateProcess("cmd.exe",
                "/c pushd \"C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment\\amd64\\WinPE_OCs\\en-us\" & dism /Image:" +
                MountPoint.Text +
                "\\MOUNTDIR /Add-Package /PackagePath:\"C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment\\amd64\\WinPE_OCs\\en-us\\",
                false);
        }

        private void UnMountWIMDiscard(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                 " /c dism /unmount-wim /mountdir:" +
                MountPoint.Text +
                "\\MOUNTDIR /discard",
                false);
        }

        private void CleanUpMountPoints(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                 " /c dism /Cleanup-Mountpoints",
                false); 
        }

        private void SpawnAShell(object sender, RoutedEventArgs e)
            // it's Spawn - A - Shell .. not Spawn as Hell !
        {
            int PID = new PS(" /k color 9e").Create("echo hello world!");

            //MessageBox.Show("PID : "+pid);
            //var cmdStartInfo = new ProcessStartInfo() { };
            //cmdStartInfo.RedirectStandardInput= true;
            //cmdStartInfo.CreateNoWindow = false;
            //cmdStartInfo.UseShellExecute= false;
            //cmdStartInfo.FileName = "cmd.exe";
            //cmdStartInfo.Arguments = " /k color 9e";
            //cmdStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            //cmdStartInfo.WorkingDirectory = MountPoint.Text;
            //var cmdProcess = Process.Start(cmdStartInfo);
            //PID = cmdProcess.Id;
            //cmdProcess.StandardInput.WriteLine("echo dit is een test!");
        }

        private void CleanupWim(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                 " /c dism /Cleanup-Wim",
                false);
        }

        private void getMountedWIMInfo(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c dism /Get-MountedWimInfo",
                false);
        }

        private void getPackages(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c dism /image:" + MountPoint.Text + "\\MOUNTDIR /Get-Packages", false);

        }

        private void CleanupIMG(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                "Dism /Image:C:\\Mount\\MOUNTDIR /cleanup-image /StartComponentCleanup /ResetBase", false);
            new ReadStdOut().CreateProcess("cmd.exe",
                "Dism /Unmount-Image /MountDir:C:\\test\\offline /Commit", false);
            new ReadStdOut().CreateProcess("cmd.exe",
                "Dism /Export-Image /SourceImageFile:C:\\Images\\install.wim /SourceIndex:1 /DestinationImageFile:C:\\Images\\install_cleaned.wim", false);
        }

        private void adkMountWIM(object sender, RoutedEventArgs e)
        {
            //dism /mount-wim /wimfile:c:\MOUNT\dimpe\media\sources\boot.wim /index:1 /MountDir:c:\MOUNT\dimpe\mount
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c DISM /mount-wim /wimfile:" + MountPoint.Text + "\\dimpe\\media\\sources\\boot.wim" +
                " /index:" + Index.Text +
                " /MountDir:" + MountPoint.Text +
                "\\MOUNTDIR", true);
        }

        private void adkCopyPE(object sender, RoutedEventArgs e)
        {
            //copype amd64 C:\WinPE_amd64
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c copype %processor_architecture% C:\\mount\\dimPE\\_%processor_architecture%", true);
        }

        private void vOS(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c copy c:\\adk\\ValidationOS.wim C:\\pe\\media\\sources\\boot.wim /y",
                false);
        }

        private void AddCabz(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c for /f \"usebackq\" %x in (`dir d:\\cabs\\*.cab /s /b`) do dism /Image:c:\\pe64\\mount /Add-Package/PackagePath:\"%x\"", true);

        }

        private void getADKPackages(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c dism /Image:c:\\pe64\\mount /get-Packages", false);
        }

        private void cleanResetBase(object sender, ContextMenuEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c Dism /Image:c:\\MOUNT\\dimpe\\mount /cleanup-image /StartComponentCleanup /ResetBase", true);
        }

        private void UnmountWIMAdk(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                             " /c dism /unmount-wim /mountdir:" +
                             MountPoint.Text +
                             "\\MOUNTDIR /commit",
                             false);
        }

        private void ExportWIMAdk(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
            " /c Dism /Export-Image /SourceImageFile:C:\\pe64\\media\\sources\\boot.wim /SourceIndex:1 /DestinationImageFile:C:\\pe64\\media\\sources\\boot2.wim", false);
            File.Delete("C:\\pe64\\media\\sources\\boot.wim");
            File.Copy("C:\\pe64\\media\\sources\\boot2.wim", "C:\\pe64\\media\\sources\\boot.wim");
        }

        private void getADKWIMNFO(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                "DISM /GET-WIMINFO /WIMFILE:C:\\PE64\\MEDIA\\SOURCES\\BOOT.WIM /INDEX:1", false);
        }
    }
}

