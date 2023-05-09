using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Words.NET;
using System.Windows.Threading;
using System.IO;
using System.Threading;
using Xceed.Document.NET;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;

namespace textGenerator2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string[] separators = new string[] { ", ", ". ", "! ", " ", "? " };
        string[] wordDict = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "RUS.txt"));

        

        private static readonly Regex onlyNumbers = new Regex("[^0-9]+");

        string ResultString = null;

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
                else if (FolderPath.Text == null)
                {
                    Warning.Content = "Вы не выбрали папку для сохранения";
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


        private void ClearText(object sender, MouseButtonEventArgs e)
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

        private string[] GenerateString(int wordCount, string[] str)
        {

            Random random = new Random();
            
            for(int i = 0; i < wordCount / 4; i++)
            {
                str[i] = wordDict[random.Next(0, wordDict.Length)] + separators[random.Next(0, separators.Length)];
            }
            return str;
        }


        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            
            DispatcherTimer timer = new ();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);

            var folderPath = FolderPath.Text.ToString();
            int min = int.Parse(Min.Text);
            int max = int.Parse(Max.Text);
            int size = wordDict.Length / 4;

            


            Random random = new();

            int count = int.Parse(FileCount.Text);
            
            
            try
            {
                for (int i = 0; i < count; i++)
                {
                    
                    int wordCount = random.Next(min, max + 1);

                    string[] str1 = new string[wordCount / 4];
                    string[] str2 = new string[wordCount / 4];
                    string[] str3 = new string[wordCount / 4];
                    string[] str4 = new string[wordCount / 4];

                    string fileName = $"Document{i}. Word count - {wordCount}.docx";
                    var document = DocX.Create(folderPath + "\\" + fileName);
                    var paragraph = document.InsertParagraph();

                    Task task1 = Task.Factory.StartNew(() => { str1 = GenerateString(wordCount, str1); });
                    Task task2 = Task.Factory.StartNew(() => { str2 = GenerateString(wordCount, str2); });
                    Task task3 = Task.Factory.StartNew(() => { str3 = GenerateString(wordCount, str3); });
                    Task task4 = Task.Factory.StartNew(() => { str4 = GenerateString(wordCount, str4); });
                    
                    task1.Wait();
                    task2.Wait();
                    task3.Wait();
                    task4.Wait();


                    ResultString = String.Concat(str1);
                    ResultString += String.Concat(str2);
                    ResultString += String.Concat(str3);
                    ResultString += String.Concat(str4);


                    if (wordCount % 4 > 0) 
                    {
                        for (int j = 0; j < wordCount % 4; j++)
                        {
                            ResultString = String.Concat(ResultString, wordDict[random.Next(0, wordDict.Length)] + separators[random.Next(0, separators.Length)]);
                        }
                    }

                    

                    paragraph.Append(ResultString);
                    document.Save();
                    document.Dispose();
                    ResultString = null;
                }
                Result.Content = "Файлы сгенерированы";
                timer.Start();
            }
            catch (System.IO.IOException)
            {
                Warning.Content = "Файл с таким именем открыт в другой программе. Закройте этот файл и повторите попытку";
            }
        }


        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath.Text = openFileDlg.SelectedPath;
                InputCheck();
            }
        }
    }
}