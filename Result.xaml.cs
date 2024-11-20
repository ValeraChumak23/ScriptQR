using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using static ScriptQR.Window_for_choice;

namespace ScriptQR
{
    /// <summary>
    /// Логика взаимодействия для Result.xaml
    /// </summary>
    public partial class Result : Window
    {
        public ObservableCollection<data> data_person = new ObservableCollection<data>(); // Храним список данных
        public int num = 0;
        public string CountPerson { get; set; }
        public class data
        {
            public int Number { get; set; }
            public string Name { get; set; }

            public string ImageStatusD { get; set; }
            public string ImageStatusQ { get; set; }
            public string ImageStatusSend { get; set; }
        }


        public Result(string countPerson)
        {
            InitializeComponent();
            UnitListView.ItemsSource = data_person;
            CountPerson = countPerson;
            DataContext = this;
        }

        public int AddPerson(string FIO)
        {
            string Status_D = @"C:\ScriptQR\Imge,Icon\Dounload.png";
            string Status_Q = @"C:\ScriptQR\Imge,Icon\Dounload.png";
            string Status_Send = @"C:\ScriptQR\Imge,Icon\Dounload.png";
            num++;
            var person = new data
            {
                Number = num,
                Name = FIO,
                ImageStatusD = Status_D,
                ImageStatusQ = Status_Q,
                ImageStatusSend = Status_Send
            };
            data_person.Add(person);
            UnitListView.Items.Refresh();
            return num;
        }
        public void UpdatePerson(int number, string flag_D, string flag_Q, string flag_Send)
        {
            var personToUpdate = data_person.FirstOrDefault(p => p.Number == number);
            if (personToUpdate != null)
            {
                if (flag_D == "Обновлен успешно") personToUpdate.ImageStatusD = @"C:\ScriptQR\Imge,Icon\update.png";
                //else if (flag_D == "Обновлен не успешно") personToUpdate.ImageStatusD = @"C:\ScriptQR\Imge,Icon\Not update.png";
                // Обновляем статус и путь к изображению
                else personToUpdate.ImageStatusD = flag_D == "Ожидание" ? @"C:\ScriptQR\Imge,Icon\Dounload.png" : Convert.ToBoolean(flag_D) ? @"C:\ScriptQR\Imge,Icon\Yes.png" : @"C:\ScriptQR\Imge,Icon\No.png";
                
                personToUpdate.ImageStatusQ = flag_Q == "Ожидание" ? @"C:\ScriptQR\Imge,Icon\Dounload.png" : Convert.ToBoolean(flag_Q) ? @"C:\ScriptQR\Imge,Icon\Yes.png" : @"C:\ScriptQR\Imge,Icon\No.png";
                personToUpdate.ImageStatusSend = flag_Send == "Ожидание" ? @"C:\ScriptQR\Imge,Icon\Dounload.png" : Convert.ToBoolean(flag_Send) ? @"C:\ScriptQR\Imge,Icon\Yes.png" : @"C:\ScriptQR\Imge,Icon\No.png";

                // Обновляем интерфейс
                UnitListView.Items.Refresh();
            }
        }

    }
}
