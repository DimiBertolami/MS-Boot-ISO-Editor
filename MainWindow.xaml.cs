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
namespace DimsISOTweaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MountISO(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe", 
                " /c powershell -command \"Mount-DiskImage -ImagePath " + ISO.Text + "\"", 
                false);
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
                    var FileHandle = File.OpenHandle(MountPoint.Text + "\\BootWIM\\boot.wim");
                         File.Copy(drives[i].Name + "Sources\\boot.wim", 
                        MountPoint.Text + "\\BootWIM\\boot.wim", 
                        true);
                    FileInfo fileInfo = new FileInfo(MountPoint.Text + "\\BootWIM\boot.wim");
                    if (fileInfo.IsReadOnly) { fileInfo.IsReadOnly= false; }
                }
            }
        }
        public void getWIMInfo(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe", 
                " /c dism /Get-MountedWimInfo", 
                false);
        }
        public void MountWIM(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c DISM /mount-wim /wimfile:" + 
                MountPoint.Text + 
                "\\BootWIM\\boot.wim /index:1 /MountDir:" + 
                MountPoint.Text + 
                "\\MOUNTDIR", 
                true);
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
            //new ReadStdOut().CreateProcess("cmd.exe",
            //    " /c dism /image:c:\\Mount\\MOUNTDIR /Get-Packages");
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
            // & echo to unmount wim file just execute & echo dism /unmount-wim /mountdir:c:\\Mount\\MOUNTDIR /discard (or /commit if you want to save changes)
            new ReadStdOut().CreateProcess("cmd.exe",
             " /c dism /unmount-wim /mountdir:" + 
             MountPoint.Text + 
             "\\MOUNTDIR /commit", 
             false);

            // dism /Cleanup-Mountpoints 
            // dism / get - mountedwiminfo


        }

        private void about(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("how to update windows 10 ISO/Wim (By Dimi Bertolami)\r\n\r\ntopics:\r\n- extra drivers\r\n- slipstream microsoft kb-updates\r\n- extra executables\r\n- how to write a PE-Network script\r\n\r\nfrom dosbox: \r\n\r\ncreate some temporary directories:\r\n(Here i'll download all the hotfixes)\r\nMD C:\\Mount\r\n\r\n:: extra drivers go into this directory. They will be installed recursively\r\nMD C:\\Mount\\Drivers\r\n\r\n:: boot.wim from the windows ISO goes here\r\nMD C:\\Mount\\BootWIM\r\n\r\n:: this is our Mount-Target Directory (in order to mount a wim file this folder has to be empty)\r\nmd C:\\MOUNT\\MOUNTDIR\r\n\r\nTo Mount an ISO with powershell: \r\npowershell -Command \"Mount-DiskImage -ImagePath C:\\Users\\Admin\\Desktop\\dewSystems\\ISO\\Gandalf10PE.ISO\"\r\n\r\ndetect drive letter where iso is mounted and copy wim to bootWIM Folder\r\nFOR /D %x in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do if EXIST %x:\\sources\\boot.wim (copy %x:\\sources\\boot.wim C:\\Mount\\BootWIM)\r\n\r\ncheck what image index to use\r\ndism /Get-WimInfo /WimFile:C:\\Mount\\BootWIM\\boot.wim\r\n\r\nmount wim file into mount-directory\r\ndism /mount-wim /wimfile:C:\\Mount\\BootWIM\\boot.wim /index:1 /MountDir:C:\\Mount\\MOUNTDIR\r\n\r\nrecursively add drivers to your PE (you must mount wim file first): \r\ndism /image:C:\\Mount\\MOUNTDIR /Add-Driver /Driver:D:\\Drivers /recurse\r\n\r\ndownload necessary updates manually\r\nslipstream downloaded windows updates into your solution\r\nDism /Image:C:\\Mount\\MOUNTDIR /Add-Package /PackagePath=kb4456655.msu /LogPath=C:\\mount\\dism.log\r\n\r\n");
        }

        private void adksetup(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.ProcessStartInfo myProcessInfo = new System.Diagnostics.ProcessStartInfo();
            //myProcessInfo.Verb = "runas";
            new ReadStdOut().CreateProcess("cmd.exe",
             " /c C:\\Users\\Admin\\source\\repos\\DimsISOTweaker\\Installers\\adksetup.exe /features optionid.deploymentTools /installpath c:\\ADK /Q", 
             false);
        }

        private void ADKPESetup(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
             " /c C:\\Users\\Admin\\source\\repos\\DimsISOTweaker\\Installers\\adkwinpesetup.exe /features + /installpath c:\\ADK /Q", 
             false);
        }

        private void addCabs(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                "/c pushd \"C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment\\amd64\\WinPE_OCs\" & dism /Image:c:\\mount\\MOUNTDIR /Add-Package /PackagePath:\"C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment\\amd64\\WinPE_OCs\\ & cd en-us & dism /Image:c:\\mount\\MOUNTDIR /Add-Package /PackagePath:\"C:\\ADK\\Assessment and Deployment Kit\\Windows Preinstallation Environment\\amd64\\WinPE_OCs\\", 
                false);
        }

        private void Testme(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                " /c echo baaye baaye koeievlaai!", 
                false);
        }

        private void UnMountWIMDiscard(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                 " /c dism /unmount-wim /mountdir:" +
                MountPoint.Text +
                "\\MOUNTDIR /discard",
                false);

            // dism /Cleanup-Mountpoints 


        }

        private void CleanUpMountPoints(object sender, RoutedEventArgs e)
        {
            new ReadStdOut().CreateProcess("cmd.exe",
                 " /c dism /Cleanup-Mountpoints",
                false); 
        }
    }
}

