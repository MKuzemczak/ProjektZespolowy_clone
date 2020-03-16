using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Security.AccessControl;

namespace App5.Views
{
    public sealed partial class BlankPage : Page, INotifyPropertyChanged
    {
        public BlankPage()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        //Klasa do klikania przyciku button1 na blankPage
        private void button1_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            char[] spliter = { '\r' };
            string fileName = "Assets\\PythonApplication3.py"; //lokalizacja skryptu
            string path = "D:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\Python37_64\\python.exe"; // lokalizacja pythona instalowanego nie przez msstore, bo do tego mam dostęp
            //string user = "kamil";    //proba obejscia

            Process python = new Process();
            python.StartInfo.FileName = path;
            python.StartInfo.RedirectStandardOutput = true;
            python.StartInfo.UseShellExecute = false;
            python.StartInfo.RedirectStandardError = true;

            python.StartInfo.Arguments = string.Concat(fileName, " ", 1.ToString());
            python.Start();

            //StreamReader sReader = python.StandardOutput;

            //string[] output = sReader.ReadToEnd().Split(spliter);

            string[] output = python.StandardOutput.ReadToEnd().Split(spliter);

            TextBox1.Text = ""; //Czysszczenie textboxa, zeby nie było milionów hello

            foreach (string s in output)
            {
                TextBox1.Text += s;
            }

            TextBox1.Text += python.StandardError.ReadToEnd();
            python.WaitForExit();

        }
    }
  
}
