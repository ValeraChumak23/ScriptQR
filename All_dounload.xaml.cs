using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace ScriptQR
{
    /// <summary>
    /// Логика взаимодействия для All_dounload.xaml
    /// </summary>
    public partial class All_dounload : Window
    {
        public string CountPerson { get; set; }
        public string Time { get; set; }
        public string FilePath { get; set; }
        public All_dounload(string count_people, string time, string filePath)
        {
            
            InitializeComponent();
            CountPerson = count_people;
            Time = time;
            FilePath = filePath;
            DataContext = this; // обновление данных в окне

        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string path = e.Uri.LocalPath;

                if (File.Exists(path))
                {
                    // Открыть файл
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else if (Directory.Exists(path))
                {
                    // Открыть папку
                    Process.Start(new ProcessStartInfo(path) { FileName = path, UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Путь недействителен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии пути: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true; // Указываем, что событие обработано
        }



    }
}
