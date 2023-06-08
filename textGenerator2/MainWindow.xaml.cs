using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Words.NET;
using System.Windows.Threading;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace textGenerator2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class FileReader
    {
        public async Task<string[]> ReadFile(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                var fileText = await reader.ReadToEndAsync();
                return fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }
        }
    }

    public class Generator
    {
        private string[] separators = new string[] { ", ", ". ", "! ", " ", "? " };

        private string[] GenerateQuarter(int wordCount, string[] wordDict)
        {
            Random random = new Random();
            string[] str = new string[wordCount / 4];

            for (int i = 0; i < wordCount / 4; i++)
            {
                str[i] = wordDict[random.Next(0, wordDict.Length)] + separators[random.Next(0, separators.Length)];
            }
            return str;
        }

        public async Task<string> GenerateString(int wordCount, string[] wordDict)
        {
            Random random = new Random();
            string resultString;

            string[] str1 = new string[wordCount / 4];
            string[] str2 = new string[wordCount / 4];
            string[] str3 = new string[wordCount / 4];
            string[] str4 = new string[wordCount / 4];

            Task task1 = Task.Factory.StartNew(() => { str1 = GenerateQuarter(wordCount, wordDict); });
            Task task2 = Task.Factory.StartNew(() => { str2 = GenerateQuarter(wordCount, wordDict); });
            Task task3 = Task.Factory.StartNew(() => { str3 = GenerateQuarter(wordCount, wordDict); });
            Task task4 = Task.Factory.StartNew(() => { str4 = GenerateQuarter(wordCount, wordDict); });

            task1.Wait();
            task2.Wait();
            task3.Wait();
            task4.Wait();

            resultString = String.Concat(str1);
            resultString += String.Concat(str2);
            resultString += String.Concat(str3);
            resultString += String.Concat(str4);

            if (wordCount % 4 > 0)
            {
                for (int j = 0; j < wordCount % 4; j++)
                {
                    resultString = String.Concat(resultString, wordDict[random.Next(0, wordDict.Length)] + separators[random.Next(0, separators.Length)]);
                }
            }

            return resultString;
        }
    }

    public class FileCreator
    {
        public async Task CreateFile(string folderPath, int wordCount, int i, string resultString)
        {
            string fileName = $"Document{i}. Word count - {wordCount}.docx";
            var document = DocX.Create(folderPath + "\\" + fileName);
            var paragraph = document.InsertParagraph();
            paragraph.Append(resultString);
            document.Save();
            document.Dispose();
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FilePath.Text = Properties.Settings.Default.prevFilePath;
            FolderPath.Text = Properties.Settings.Default.prevFolderPath;
        }

        private static readonly Regex onlyNumbers = new Regex("[^0-9]+");

        private void InputCheck()
        {
            try
            {
                if (int.Parse(Min.Text) > int.Parse(Max.Text))
                {
                    Warning.Content = "Минимальное количество слов не может быть больше максимального";
                    Generate.IsEnabled = false;
                    return;
                }
                else if (int.Parse(FileCount.Text) == 0)
                {   
                    Warning.Content = "Число генерируемых файлов должно быть больше 0";
                    Generate.IsEnabled = false;
                    return;
                }
                else if (FolderPath.Text == "")
                {
                    Warning.Content = "Вы не выбрали папку для сохранения";
                    Generate.IsEnabled = false;
                    return;
                }
                else if (FilePath.Text == "")
                {
                    Warning.Content = "Вы не выбрали файл со словами";
                    Generate.IsEnabled = false;
                    return;
                }
                else
                {
                    Warning.Content = null;
                    Generate.IsEnabled = true;
                }
            }
            catch
            {
                Warning.Content = "Какое-то из полей пустое или значение в нём больше " + int.MaxValue.ToString();
                Generate.IsEnabled = false;
            }
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            TextBox textbox = e.Source as TextBox;
            if (textbox == null) return;
            textbox.Clear();
        }

        private static bool IsTextAllowed(string text)
        {
            return !onlyNumbers.IsMatch(text);
        }

        private void ContentChanged(object sender, TextChangedEventArgs e)
        {
            InputCheck();
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            Result.Content = null;
            timer.Stop();
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 2);

            Result.Foreground = System.Windows.Media.Brushes.Black;
            Result.Content = "Файлы генерируются. Ожидайте.";

            string filePath = FilePath.Text;
            string folderPath = FolderPath.Text;
            int min = int.Parse(Min.Text);
            int max = int.Parse(Max.Text);
            int count = int.Parse(FileCount.Text);
            

            await Task.Run(() => Generate_ClickTask(filePath, folderPath, min, max, count));
            Result.Content = null;
            Result.Foreground = System.Windows.Media.Brushes.Green;
            Result.Content = "Файлы сгенерированы";
            timer.Start();
        }

        private async Task Generate_ClickTask(string filePath, string folderPath, int min, int max, int count)
        {
            FileReader reader = new FileReader();
            var wordDict = await reader.ReadFile(filePath);

            Generator generator = new Generator();
            FileCreator creator = new FileCreator();

            Random random = new();

            for (int i = 0; i < count; i++)
            {
                int wordCount = random.Next(min, max + 1);
                string resultString = await generator.GenerateString(wordCount, wordDict);
                await creator.CreateFile(folderPath, wordCount, i, resultString);
            }
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath.Text = openFileDlg.SelectedPath;
                Properties.Settings.Default.prevFolderPath = openFileDlg.SelectedPath;
                Properties.Settings.Default.Save();
                InputCheck();
            }
        }

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите исходный файл со словами. Убедитесь, что он сохранён в кодировке UTF-8";
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true) 
            {
                FilePath.Text = openFileDialog.FileName;
                Properties.Settings.Default.prevFilePath = openFileDialog.FileName;
                Properties.Settings.Default.Save();
                InputCheck();
            }
        }
    }
}