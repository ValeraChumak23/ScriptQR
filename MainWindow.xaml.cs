using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;
using ScriptQR.ServiceReference1;
using QRCoder;
using System.Net;
using System.Net.Mail;
using static MaterialDesignThemes.Wpf.Theme;
using System.ComponentModel;
using System.Windows.Threading;
using System.Xml.Linq;
using static ScriptQR.MainWindow;
using MaterialDesignThemes.Wpf;
using System.Threading;
using static QRCoder.PayloadGenerator;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using Microsoft.Xaml.Behaviors.Layout;
using System.Reflection;
using System.Data;
using ControlzEx.Standard;

namespace ScriptQR
{
    public partial class MainWindow : Window
    {
        public List<PersonData> personDataList;
        public List<PersonData> faild_dounload_personDataList;
        public List<Person> List_people_from_didvision;

        public int count_person = 0;
        public int count_null_date = 0;
        private static readonly Dictionary<string, bool> domainCache = new Dictionary<string, bool>();

        public bool flag_autorization = false;
        public string UserName = "parsec";
        public string Password = "parsec";
        public string Division = "system";

        public SemaphoreSlim Log_semaphore = new SemaphoreSlim(1, 1);

        public string FilePath_Doc = null;
        public string FilePath_log = null;
        public string FilePath_not_uploaded = null;
        public string FilePath_list_delete_people = null;
        public string FilePath_directory = null;
        public string FilePath_Icon = null;
        public string FilePath_QRphoto = null;
        public string FilePath_folder_photos_for_event = null;
        public string FilePath_ListUpload_peoples = null;
        public string Name_file_where_download_is_coming_from = "";

        public string name_file_QR;
        public bool Main_process = false;

        public Guid Session_ID = Guid.Empty;
        public Guid Email_column_ID = Guid.Empty;
        public Guid Phone_column_ID = Guid.Empty;
        public Guid Date_start_end_column_ID = Guid.Empty;
        public Guid Who_inveted_column_ID = Guid.Empty;
        public Guid Method_entry_column_ID = Guid.Empty;

        public string Name_Access;
        public string Key_Access;
        public string Name_Division;
        public string Key_Division;
        public bool flag_send;


        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
            InitializeComponent();
            

        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await PerformAuthorizationAsync();
        }

        private async Task PerformAuthorizationAsync()
        {
            MainRoot.IsEnabled = false; 
            LoadingProgressBar.Visibility = Visibility.Visible;

            await authorization();
            if (flag_autorization)
            {
                PersonDataGrid.ItemsSource = personDataList;

                ReadXmlFile(@"C:\ScriptQR\default_data.xml");

                FilePath_directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                FilePath_directory = FilePath_directory.Substring(0, FilePath_directory.IndexOf(@"\bin"));

                FilePath_Icon = Path.Combine(FilePath_directory, "Imge, Icon");
    
                FilePath_Doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                FilePath_log = Path.Combine(FilePath_Doc, "Generate_Visitor_Qr_Code_logfile.txt");

                FilePath_not_uploaded = Path.Combine(FilePath_Doc, "Not_uploaded_people.txt");

                FilePath_list_delete_people = Path.Combine(FilePath_Doc, "Список удаленных.txt");

                FilePath_QRphoto = Path.Combine(FilePath_Doc, "Qr фотографии и списки людей");

                if (!Directory.Exists(FilePath_QRphoto)) Directory.CreateDirectory(FilePath_QRphoto);


                personDataList = new List<PersonData>();

                LoadingProgressBar.Visibility = Visibility.Collapsed;
                MainRoot.IsEnabled = true;
                this.DataContext = this;

            }
        }


        public async Task authorization()
        {
            var userInputDialog = new UserInputDialog();
            if (userInputDialog.ShowDialog() == true )
            {
                
                var client = new IntegrationServiceSoapClient();
                var sessionResponse = default(OpenSessionResponse);


                sessionResponse = await client.OpenSessionAsync(userInputDialog.organization, userInputDialog.userName, userInputDialog.password);
                var res = sessionResponse.Body.OpenSessionResult;

                if (res.Result == 0)
                {

                    UserName = userInputDialog.userName;
                    Password = userInputDialog.password;
                    Division = userInputDialog.organization;
                    flag_autorization = true;
                    MessageBox.Show($"Добро пожаловать!");
                    await CloseSession();
                   
                }
                else
                {
                    if ( res.ErrorMessage.Contains("Нет оператора") || res.ErrorMessage.Contains("Неправильный пароль") || res.ErrorMessage.Contains("Нет организации"))
                    {
                        MessageBox.Show("Неверно введены данные.\nПожалуйста, попробуйте снова.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        await authorization();
                    }
                  
                    
                }


            }
            else this.Close();
        }

        public void Settings(object sender, RoutedEventArgs e)
        {
            var settings = new Settings(this);
            settings.Show();

        }



        public async Task LogMessage(string message, string type_log)
        {
            string FilePath = null;

            switch (type_log)
            {
                case "system":
                    FilePath = FilePath_log;
                    break;

                case "delete_people":
                    FilePath = FilePath_list_delete_people;
                    break;

                case "upload_people":
                    FilePath = FilePath_ListUpload_peoples;
                    break;
            }


            if (!File.Exists(FilePath))
            {
                // Создаём файл, используя `StreamWriter`, чтобы избежать блокировки
                using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Файл создаётся и сразу закрывается
                }
            }

            string logEntry = $"{DateTime.Now:HH:mm:ss.fff}: {message}{Environment.NewLine}";

            // Используем семафор для синхронизации доступа к файлу
            await Log_semaphore.WaitAsync();
            try
            {
                // Открываем файл в режиме добавления и записываем строку асинхронно
                using (StreamWriter writer = new StreamWriter(FilePath, append: true))
                {
                    await writer.WriteLineAsync(logEntry);
                }
            }
            finally
            {
                Log_semaphore.Release();
            }
        }


        public void Date_windows(object sender, RoutedEventArgs e)
        {
            // Принудительно завершаем редактирование
            if (PersonDataGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                var datePickerWindow = new DatePickerWindow();
                if (datePickerWindow.ShowDialog() == true)
                {
                    DateTime StartDate = datePickerWindow.StartDate;
                    DateTime EndDate = datePickerWindow.EndDate;
                    for (int i = 0; i < personDataList.Count; i++)
                    {
                        var person = personDataList[i];
                        if (person.IsSelected == true)
                        {
                            person.Date_Start = StartDate;
                            person.Date_End = EndDate;
                        }
                        personDataList[i] = person;
                    }

                    // Обновляем источник данных
                    PersonDataGrid.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Завершите редактирование перед выбором даты.");
            }
        }



        public void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;
            bool isChecked = checkBox.IsChecked == true;

            if (PersonDataGrid.CommitEdit(DataGridEditingUnit.Cell, true))
            {
                // Теперь можно продолжить обновление
                var Person_list = personDataList;

                for (int i = 0; i < Person_list.Count; i++)
                {
                    var visit = Person_list[i];

                    if (isChecked)
                    {
                        visit.IsSelected = true;
                    }
                    else
                    {
                        visit.IsSelected = false;
                    }

                    // Обновляем элемент в коллекции
                    Person_list[i] = visit;
                }

                // Обновляем DataGrid
                PersonDataGrid.Items.Refresh();
            }
        }


        public void IndividualCheckBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;
            var currentPerson = (PersonData)checkBox.DataContext;

            // Инвертируем текущее состояние IsSelected
            currentPerson.IsSelected = !currentPerson.IsSelected;
            checkBox.IsChecked = currentPerson.IsSelected;

            // Обновляем состояние человека в общем списке
            for (int i = 0; i < personDataList.Count; i++)
            {
                var person = personDataList[i];
                if (currentPerson.FirstName == person.FirstName &&
                    currentPerson.MiddleName == person.MiddleName &&
                    currentPerson.LastName == person.LastName &&
                    currentPerson.Email == person.Email)
                {
                    person.IsSelected = currentPerson.IsSelected;
                }
                personDataList[i] = person; // Обновляем элемент в коллекции
            }
          
            PersonDataGrid.Items.Refresh();
        }

        // МАКСИМАЛЬНО кривой способ обновления и сохранени измененных данных в DataGrid.Column (прям пиздец)
        // Если вы не тот кто это разрабатывал, то не смотрите на это (ну или не сильно ругайтесь, т.к. логики максимально кривая и не продуманная)
        // я опишу наверное как нодо было это все реализовывать, но не факт что оно верно 
        // вмество структуры надо было использовать ObservableCollection: если ваша коллекция данных реализует интерфейс INotifyCollectionChanged
        // если этот кривой косой код, будет испольковаться дальше то это будет *****

        public void PersonDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // е - объект класса DataGridCellEditEndingEventArgs который используется в обработчиках событий для события CellEditEnding
            // Проверяем, что действие редактирования - это завершение
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // создание екземпляра хранящего старые записи о человекк
                var editedPerson = (PersonData)e.Row.Item;

                if (e.Column.Header.ToString() == "Дата начала" || e.Column.Header.ToString() == "Дата окончания")
                {
                    e.Cancel = true; // Отменяем изменение
                    return;
                }


                var textBox = e.EditingElement as System.Windows.Controls.TextBox; 
                // старые данные для поиска человека в списке
                string oldLastName = editedPerson.LastName;
                string oldFirstName = editedPerson.FirstName;
                string oldMiddleName = editedPerson.MiddleName;
                // вносим изменения
                if (textBox != null)
                {
                    string columnName = e.Column.Header.ToString();
                    switch (columnName)
                    {
                        case "Фамилия":
                            editedPerson.LastName = textBox.Text;
                            break;
                        case "Имя":
                            editedPerson.FirstName = textBox.Text;
                            break;
                        case "Отчество":
                            editedPerson.MiddleName = textBox.Text;
                            break;
                        case "Цель визита":
                            editedPerson.Purpose_Visit = textBox.Text;
                            break;
                        case "Пригласивший":
                            editedPerson.Who_invited = textBox.Text;
                            break;
                        case "Почта":
                            editedPerson.Email = textBox.Text;
                            break;
                        case "Номер телефона":
                            editedPerson.Phone_number = textBox.Text;
                            break;
                    }
                }
                

                // Находим индекс по старым значениям
                int index = personDataList.FindIndex(p =>
                    p.LastName == oldLastName &&
                    p.FirstName == oldFirstName && 
                    p.MiddleName == oldMiddleName); 

                if (index >= 0)
                {
                    // добавить проверки на телефон если тот менялся и почту
                    if (editedPerson.LastName != "" &&
                        editedPerson.FirstName != "" &&
                        editedPerson.Purpose_Visit != "" &&
                        editedPerson.Who_invited != "")
                    {
                        editedPerson.Problem_data = false;
                    }

                    personDataList[index] = editedPerson;
                    
                    //MessageBox.Show($"{personDataList[index].LastName} {personDataList[index].FirstName} {personDataList[index].Problem_data}");

                    PersonDataGrid.ItemsSource = null;
                    PersonDataGrid.ItemsSource = personDataList;

                }
            }
            
        }


        // Обработчик для выбора файла
        private async void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно для выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text and CSV files (*.txt;*.csv)|*.txt;*.csv|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Очищаем старые данные, если они уже загружены
                PersonDataGrid.ItemsSource = null;
                SelectedFileText.Text = string.Empty;

                // Обновляем текстовое поле с выбранным файлом
                SelectedFileText.Text = $"Выбран файл: {openFileDialog.FileName}";

                Name_file_where_download_is_coming_from = DateTime.Now.ToString("dd.MMMM.yyyy") + " " + Path.GetFileName(openFileDialog.FileName);
                

                // Чтение данных из файла и сохранение в поле класса
                personDataList = await ReadDataFromFileAsync(openFileDialog.FileName);

                // Отображение данных в DataGrid
                if (personDataList != null)
                {
                    PersonDataGrid.ItemsSource = personDataList;
                    DateButton.IsEnabled = true;
                    Dounload_button.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Не удалось прочитать данные из файла.");
                }
            }
        }


        // Обработчик для загрузки данных
        public async void Dounload_Data(object sender, RoutedEventArgs e)
        {
            await LogMessage($"Нажата клавиша загрузка данных.", "system");



            if (personDataList.All(p => p.Date_Start != null || p.Date_End != null) && personDataList.All(p => p.Problem_data != true) && Dounload_button.IsEnabled == true) {

                Stopwatch clock_Dounload = new Stopwatch();

                System.Windows.Controls.Button loadButton = sender as System.Windows.Controls.Button;
                loadButton.IsEnabled = false;
                count_person = personDataList.Count(); 

                faild_dounload_personDataList = new List<PersonData>();

                Session_ID = await OpenSession();

                if (Session_ID != Guid.Empty)
                {
                    // создание папки,где будут хранится QR для определенного события, и списка загруженных людей
                    FilePath_folder_photos_for_event = Path.Combine(FilePath_QRphoto, Name_file_where_download_is_coming_from);
                    delete_and_create_file();
                    FilePath_ListUpload_peoples = Path.Combine(FilePath_folder_photos_for_event, "Список загруженых людей.txt");
                    MessageBox.Show($"{FilePath_ListUpload_peoples}");


                    clock_Dounload.Start();


                    await LogMessage($"Сессия успешно открыта для пользователя с именем:{UserName}","system");

                    await LogMessage($"Получение ID для дополнительных полей.", "system");
                    await GetVisitorExtraFieldTemplates(Session_ID);

                    var res = new Result(count_person.ToString());
                    res.Show();

                    loadButton.IsEnabled = true;

                    // Получение списка людей из подразделения
                    await Getting_list_people_from_division();
                    MessageBox.Show($"колличество людей в подразделении {List_people_from_didvision.Count}");

                    
                    if (Email_column_ID != Guid.Empty && Phone_column_ID != Guid.Empty)
                    {
                        var semaphore = new SemaphoreSlim(10);

                        var tasks = new List<Task>();


                        foreach (var personData in personDataList)
                        {
                            bool has_in_list_faild_dounload = false;

                            // асинхронная загруска
                            tasks.Add(Task.Run(async () =>
                            {

                                await semaphore.WaitAsync();
                                try
                                {

                                    // Проверка наличия персоны в списке людей из подразделения, с последующими условиями
                                    if (List_people_from_didvision.Count() >= 0) await Checking_availability_deletion(personData.LastName, personData.FirstName, personData.MiddleName);

                                    string FIO = personData.LastName + " " + personData.FirstName + " " + personData.MiddleName;

                                    int num = 0;

                                    // Обновляем UI с добавлением персоны
                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        num = res.AddPerson(FIO);
                                    });

                                    if (String.IsNullOrEmpty(personData.Email) || String.IsNullOrEmpty(personData.Phone_number))
                                    {
                                        faild_dounload_personDataList.Add(personData);
                                        has_in_list_faild_dounload = true;
                                    }

                                    await LogMessage($"Создание гостя ФИО: {personData.LastName} {personData.FirstName} {personData.MiddleName}","system");
                                    bool flag_createVisitor = false;
                                    bool flag_addPersonIdentifier = false;
                                    bool flag_add_Email_and_Phone = false;
                                    bool flag_generatePhotoQR = false;
                                    bool flag_SendPhotoQR = false;
                                    bool flag_dounload = false;



                                    var person = new Person
                                    {
                                        ID = Guid.NewGuid(),
                                        FIRST_NAME = personData.FirstName,
                                        LAST_NAME = personData.LastName,
                                        MIDDLE_NAME = personData.MiddleName,
                                        ORG_ID = Guid.Parse(Key_Division)  // ключ подразделение для Гостей
                                    };

                                    Guid visitorID = await CreateVisitor(Session_ID, person);

                                    if (visitorID != Guid.Empty)
                                    {

                                        flag_createVisitor = true;
                                        string Identificator = await GetUnique4bCardCode(Session_ID, person);
                                        Guid personSession = await OpenPersonEditingSession(Session_ID, visitorID);

                                        var Identifier = new IdentifierTemp
                                        {
                                            CODE = Identificator,
                                            PERSON_ID = person.ID,
                                            ACCGROUP_ID = Guid.Parse(Key_Access), // ключ группы доступа
                                            IDENTIFTYPE = 0,
                                            NAME = personData.Purpose_Visit,
                                            VALID_FROM = (DateTime)personData.Date_Start,
                                            VALID_TO = (DateTime)personData.Date_End,
                                        };

                                        flag_addPersonIdentifier = await AddPersonIdentifier(personSession, Identifier, person);

                                        flag_add_Email_and_Phone = await SetPersonExtraFieldValues(personSession, personData.Email, personData.Phone_number,"С " + personData.Date_Start?.ToString("dd.MMMM.yyyy") + " по " + personData.Date_End?.ToString("dd.MMMM.yyyy"), personData.Who_invited);
                                        await SetIdentifierPrivileges(Session_ID, Identifier.CODE); // гостевая карта
                                        await ClosePersonEditingSession(personSession);


                                        if (flag_createVisitor == true && flag_addPersonIdentifier == true)
                                        {
                                            flag_dounload = true;

                                            await Application.Current.Dispatcher.InvokeAsync(() =>
                                            {
                                                res.UpdatePerson(num, flag_dounload.ToString(), "Ожидание", "Ожидание");
                                            });

                                            var Qr_code_text = await GenerateParsecQRCode(Session_ID, Identifier.CODE, personData.LastName, personData.FirstName, personData.MiddleName);

                                            if (Qr_code_text != "")
                                            {

                                                flag_generatePhotoQR = await GeneratePhotoQR(Qr_code_text, personData.LastName, personData.FirstName, personData.MiddleName, personData.Email, personData.Phone_number);

                                                if (flag_generatePhotoQR && has_in_list_faild_dounload == false)
                                                {
                                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                                    {
                                                        res.UpdatePerson(num, flag_dounload.ToString(), flag_generatePhotoQR.ToString(), "Ожидание");
                                                    });


                                                    if (flag_send)
                                                    {
                                                        flag_SendPhotoQR = await SendEmailsAsync(personData.Email, FilePath_Doc, personData.Purpose_Visit, FIO, (personData.Date_Start).ToString(), (personData.Date_End).ToString());
                                                        if (!flag_SendPhotoQR) faild_dounload_personDataList.Add(personData);
                                                    }

                                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                                    {
                                                        res.UpdatePerson(num, flag_dounload.ToString(), flag_generatePhotoQR.ToString(), flag_SendPhotoQR.ToString());
                                                    });
                                                }
                                                else
                                                {
                                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                                    {
                                                        res.UpdatePerson(num, flag_dounload.ToString(), flag_generatePhotoQR.ToString(), flag_SendPhotoQR.ToString());
                                                    });
                                                }
                                            }
                                        }
                                       
                                    }
                                    
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }));
                        }


                        // Асинхронное ожидание завершения всех задач
                        await Task.WhenAll(tasks);
                    }

                    await CloseSession();
                    await LogMessage($"Сессия успешно закрыта.", "system");
                    clock_Dounload.Stop();

                    if (faild_dounload_personDataList.Count == 0)
                    {
                        //MessageBox.Show($"Загрузка прошла успешно!\nЗагружено {count_person}.\nЗа {clock_Dounload.Elapsed.TotalSeconds}");
                        var all_res_windows = new All_dounload((count_person).ToString(), (Math.Round(clock_Dounload.Elapsed.TotalSeconds, 2)).ToString(), FilePath_ListUpload_peoples);
                        all_res_windows.Show();
                    }
                    else
                    {
                        var Not_all_res_windows = new Not_all_load(faild_dounload_personDataList, (count_person - faild_dounload_personDataList.Count).ToString(), (Math.Round(clock_Dounload.Elapsed.TotalSeconds, 2)).ToString(),FilePath_ListUpload_peoples);
                        Not_all_res_windows.Show();
                        //MessageBox.Show($"Загрузка прошла успешно!\nЗагружено {count_person - faild_dounload_personDataList.Count}.\nЗа {clock_Dounload.Elapsed.TotalSeconds}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Есть гости для, которых есть проблемы с данными!", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async Task<bool> SendEmailsAsync(string email,string documentsPath, string Personevent,string FIO, string Start, string End)
        {
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string fromEmail = "estr65st@gmail.com";
            string password = "yrnw tpwm unqt fyot";
            string toEmail = email;

            // Создаем письмо
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail);
            mail.To.Add(new MailAddress(toEmail));
            mail.Subject = "Отправка QR кода";
            mail.Body = $"{FIO} вы приглашены в СПБГУТ ЛЭТИ для участия в мероприятии \"{Personevent}\", ваш QR код будет действовать с {Start} до {End}";

            // Добавляем вложение (путь к файлу .png)
            string imagePath = @$"{documentsPath}\QR_photo\{name_file_QR}";
            Attachment attachment = new Attachment(imagePath);
            mail.Attachments.Add(attachment);

            // Настройки SMTP-клиента
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(fromEmail, password);
            smtpClient.EnableSsl = true;

            try
            {
                // Отправляем письмо асинхронно
                await smtpClient.SendMailAsync(mail);
                await LogMessage($"Приглашение успешно отправлено для {FIO}, по адресу {email}.","system");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                if (smtpEx.StatusCode == SmtpStatusCode.MailboxUnavailable)
                {
                    MessageBox.Show("Почтовый ящик не существует.");
                    await LogMessage($"Ошибка почтового ящика не существует {email}, для {FIO}.\n{smtpEx.Message}", "system");
                }
                else
                {
                    MessageBox.Show($"Ошибка при отправке письма: {smtpEx.Message}");
                    await LogMessage($"Ошибка при отправке письма для {FIO}, по адресу {email}.\n{smtpEx.Message}", "system");
                }
                return false;
            }
            finally
            {
                mail.Dispose();
                smtpClient.Dispose();
            }
            
        }

        // Считыватель .txt файлов
        public async Task<List<PersonData>> ReadDataFromTXTAsync(string filePath)
        {
            var personDataList = new List<PersonData>();
            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var data = line.Split(';');
                        if (data.Length >= 3)
                        {
                            var fullName = data[0].Split(' ');
                            if (fullName.Length == 3)
                            {
                                DateTime? startDate = null;
                                DateTime? endDate = null;

                                if (DateTime.TryParseExact(data[1], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedStartDate))
                                {
                                    startDate = parsedStartDate;
                                }
                                if (DateTime.TryParseExact(data[2], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedEndDate))
                                {
                                    endDate = parsedEndDate;
                                }
  

                                var personData = new PersonData
                                {
                                    FirstName = fullName[1],
                                    MiddleName = fullName[2],
                                    LastName = fullName[0],
                                    Date_Start = startDate,
                                    Date_End = endDate,
                                    Purpose_Visit = data[3],
                                    Email = data[4],
                                    Phone_number = data[5]
                                };

                                personDataList.Add(personData);

                            }
                        }
                    }
                    await LogMessage($"Данные успешно считаны.", "system");
                }
            }
            catch (Exception ex)
            {
                await LogMessage($"Произошла ошибка при чтении файла.\n{ex.Message}", "system");
            }
            return personDataList;
        }

        // Считыватель .csv файлов
        public async Task<List<PersonData>> ReadDataFromCSVAsync(string filePath)
        {
            var personList = new List<PersonData>();

            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Для поддержки кодировки Windows-1251
                using (var reader = new StreamReader(filePath, Encoding.GetEncoding("windows-1251")))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var values = line.Split(';');

                        if (values.Length >= 3)
                        {
                            DateTime? startDate = null;
                            DateTime? endDate = null;
                            bool problem_data = false;
                            string email = "";
                            string phone_number = "";

                            if (DateTime.TryParseExact(values[3], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedStartDate))
                            {
                                startDate = parsedStartDate;
                            }

                            if (DateTime.TryParseExact(values[4], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedEndDate))
                            {
                                endDate = parsedEndDate;
                            }

                            if (!String.IsNullOrEmpty(values[7]))
                            {
                                if (IsValidEmail(values[7]) && DomainExists(values[7]))
                                {
                                    email = values[7];
                                }

                            }
                            if (!String.IsNullOrEmpty(values[8]))
                            {
                                if (IsValidPhoneNumber(values[8]) && IsLengthValid(values[8])) phone_number = values[8];
                            }

                            if (values[0] == "" || values[1] == "" || values[5] == "" || values[6] == "" ) problem_data = true;

                            var person = new PersonData
                            {
                                FirstName = values[1],
                                MiddleName =  values[2],
                                LastName =  values[0],
                                Date_Start = startDate,
                                Date_End = endDate,
                                Purpose_Visit = values[5],
                                Who_invited = values[6],
                                Email = email,
                                Phone_number = phone_number,
                                Problem_data = problem_data,
                                IsSelected = true

                            };

                            personList.Add(person);
                        }
                    }
                    await LogMessage($"Данные успешно считаны.", "system");
                }
            }
            catch (Exception ex)
            {
                await LogMessage($"Произошла ошибка при чтении файла.\n{ex.Message}", "system");
                //MessageBox.Show($"Произошла ошибка при чтении файла: {ex.Message}");
            }
            return personList;
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        public bool DomainExists(string email)
        {
            string domain = email.Substring(email.IndexOf('@') + 1);
            if (domainCache.TryGetValue(domain, out bool exists))
            {
                return exists;
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(domain);
                domainCache[domain] = true;
                return true;
            }
            catch (SocketException)
            {
                domainCache[domain] = false;
                return false;
            }
        }


        public bool IsValidPhoneNumber(string phoneNumber)
        {            
            string pattern = @"^(\+?[1-9]\d{1,14})$"; // Поддержка E.164 (международный формат)
            return Regex.IsMatch(phoneNumber, pattern);
        }
        public static bool IsLengthValid(string phoneNumber)
        {
            string normalizedNumber = Regex.Replace(phoneNumber, @"[^\d]", "");
            return normalizedNumber.Length <= 11;
        }

        // Определяем, какой тип файла считывать
        public async Task<List<PersonData>> ReadDataFromFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            List<PersonData> personDataList = null;

            if (extension == ".txt")
            {
                personDataList = await ReadDataFromTXTAsync(filePath);
            }
            else if (extension == ".csv")
            {
                personDataList = await ReadDataFromCSVAsync(filePath);
            }



            return personDataList;
        }
        
        public void ReadXmlFile(string filePath)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);
                Name_Access = doc.Root.Element("AccessGroup").Element("Name").Value;
                Key_Access = doc.Root.Element("AccessGroup").Element("Key").Value;
                Name_Division = doc.Root.Element("OrgUnit").Element("Name").Value;
                Key_Division = doc.Root.Element("OrgUnit").Element("Key").Value;
                flag_send = Convert.ToBoolean(doc.Root.Element("Sending").Element("Flag").Value);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void ModifyXmlFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            doc.Root.Element("AccessGroup").Element("Name").Value = Name_Access;
            doc.Root.Element("AccessGroup").Element("Key").Value = Key_Access;
            doc.Root.Element("OrgUnit").Element("Name").Value = Name_Division;
            doc.Root.Element("OrgUnit").Element("Key").Value = Key_Division;
            doc.Root.Element("Sending").Element("Flag").Value = flag_send.ToString();
            doc.Save(filePath);
        }


        public struct PersonData
        {
            public string FirstName { get; set; }
            public string?  MiddleName { get; set; }
            public string LastName { get; set; }
            public DateTime? Date_Start { get; set; }
            public DateTime? Date_End { get; set; }
            public string Purpose_Visit { get; set; }
            public string Who_invited { get; set; }
            public string? Email { get; set; }
            public string? Phone_number { get; set; }
            public bool IsSelected { get; set; } 
            public bool Problem_data {  get; set; }

        }

        public async Task<Guid> OpenSession()
        {
            try
            {

                var client = new IntegrationServiceSoapClient();
                var sessionResponse = default(OpenSessionResponse);
                // Передаем введенные данные в OpenSessionAsync

                sessionResponse = await client.OpenSessionAsync(Division, UserName, Password);
                

                var res = sessionResponse.Body.OpenSessionResult;

                if (res.Result == 0)
                {
                    await LogMessage($"Сессия успешно открыта.", "system");
                    //MessageBox.Show($"Сессия успешно открыта. Session ID: {res.Value.SessionID}");
                    var sessionIDBytes = res.Value.SessionID.ToByteArray();
                    return new Guid(sessionIDBytes);
                }
                else
                {
                    await LogMessage($"Ошибка при открытии сессии.\n{res.ErrorMessage}", "system");
                    MessageBox.Show($"Ошибка при открытии сессии: {res.ErrorMessage}");
                    return Guid.Empty;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                return Guid.Empty;
            }
        }
        public async Task CloseSession()
        {
            var client = new IntegrationServiceSoapClient();
            await client.CloseSessionAsync(Session_ID);
            
        }


        public async Task<string> GetOrgUnitsHierarhy(Guid sessionID)
        {
            try
            {
                var client = new IntegrationServiceSoapClient();
                var result = await client.GetOrgUnitsHierarhyAsync(sessionID);
                var orgUnits = result.Body.GetOrgUnitsHierarhyResult;

                var windows_choice = new Window_for_choice(orgUnits);

                if (windows_choice.ShowDialog() == true && windows_choice.SelectedUnit != null)
                {
                    return windows_choice.SelectedUnit.Key;
                }
                else
                {
                    MessageBox.Show("Пользователь не сделал выбор.");
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при получении организационных единиц с посетителями: {ex.Message}");
                return "Error";
            }
        }


        public async Task<string> GetAccessGroups(Guid sessionID)
        {
            try
            {
                var client = new IntegrationServiceSoapClient();
                var result = await client.GetAccessGroupsAsync(sessionID);
                var accsessGroup = result.Body.GetAccessGroupsResult;
                var windows_choice = new Window_for_choice(accsessGroup);
                if (windows_choice.ShowDialog() == true && windows_choice.SelectedUnit != null)
                {
                    return windows_choice.SelectedUnit.Key;
                }
                else
                {
                    MessageBox.Show("Пользователь не сделал выбор.");
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при получении списка групп-доступа: {ex.Message}");
                return "Error";
            }
        }

        public async Task<Guid> CreateVisitor(Guid sessionID, Person person)
        {
            try
            {
                var client = new IntegrationServiceSoapClient(); 
                var result = await client.CreateVisitorAsync(sessionID, person);
                var createVisitorResult = result.Body.CreateVisitorResult;

                if (createVisitorResult.Result == 0)
                {
                    await LogMessage($"Карточка гостя созадана", "system");
                    //MessageBox.Show($"Новый гость успешно создан! ID: {person.ID}");
                    return person.ID;
                }
                else
                {

                    //MessageBox.Show($"Ошибка при создании гостя: {createVisitorResult.ErrorMessage}");
                    await LogMessage($"Ошибка при создании карточки гостя!\n{createVisitorResult.ErrorMessage}", "system");

                }
            }
            catch (Exception ex)
            {
                await LogMessage($"Ошибка при создании карточки гостя!\n{ex.Message}", "system");

            }

            return Guid.Empty;
        }

        public async Task<string> GetUnique4bCardCode(Guid personSessionID, Person person)
        {
            var client = new IntegrationServiceSoapClient();
            var result = await client.GetUnique4bCardCodeAsync(personSessionID);
            var GetUnique4bCardCode = result.Body.GetUnique4bCardCodeResult;
            return GetUnique4bCardCode.Value.ToString();
        }

        public async Task<Guid> OpenPersonEditingSession(Guid sessionID, Guid personID)
        {
            try
            {
                var client = new IntegrationServiceSoapClient();
                var sessionResponse = await client.OpenPersonEditingSessionAsync(sessionID, personID);
                var res = sessionResponse.Body.OpenPersonEditingSessionResult;

                if (res.Result == 0)
                {
                    string message = $"Персональная сессия успешно открыта. Session ID: {res.Value}";
                    
                    return res.Value;
                }
                else
                {
                    string errorMessage = $"Ошибка при открытии персональной сессии: {res.ErrorMessage}";
                    
                    return Guid.Empty;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при открытии сессии: {ex.Message}";
              
                return Guid.Empty;
            }
        }

        public async Task<bool> AddPersonIdentifier(Guid personSessionID, BaseIdentifier BaseIdentifier, Person person)
        {
            var client = new IntegrationServiceSoapClient();
            var result = await client.AddPersonIdentifierAsync(personSessionID, BaseIdentifier);
            var AddPersonIdentifier = result.Body.AddPersonIdentifierResult;
            
            if (AddPersonIdentifier.Result == 0)
            {
                
                await LogMessage($"Идентификар успешно присвоен гостю.", "system");
                return true;
              
            }
            else
            {
                await LogMessage($"Ошибка при присвоении иднтификатора: {AddPersonIdentifier.ErrorMessage}", "system");
                return false;
            }
        }

        public async Task SetIdentifierPrivileges(Guid sessionID, string CardCode)
        {
            var client = new IntegrationServiceSoapClient();
            var result = await client.SetIdentifierPrivilegesAsync(sessionID, CardCode, 128);
            var SetIdentifierPrivileges = result.Body.SetIdentifierPrivilegesResult;
        }

        public async Task ClosePersonEditingSession(Guid personSessionID)
        {
            var client = new IntegrationServiceSoapClient();
            await client.ClosePersonEditingSessionAsync(personSessionID);
        }

        public async Task<string> GenerateParsecQRCode(Guid sessionID, string cardCode, string LastName, string FirstName, string MidleName)
        {
            try
            {
                var client = new IntegrationServiceSoapClient();
                var result = await client.GenerateParsecQRCodeAsync(sessionID, cardCode);
                var GenerateParsecQRCode = result.Body.GenerateParsecQRCodeResult;
                if (GenerateParsecQRCode.Result == 0)
                {
                    await LogMessage($"Текст QR кода для гостя успешно создан.", "system");
                    return GenerateParsecQRCode.Value.ToString();
                }
                else
                {
                    await LogMessage($"Ошибка при создании QR кода для гостя!\n{GenerateParsecQRCode.ErrorMessage}", "system");
                    return "";
                }
            }
            catch (Exception ex) 
            {
                await LogMessage($"Ошибка при создании текста QR кода для гостя!\n{ex.Message}", "system");
                return "";
            }
        }

        public async Task<bool> GeneratePhotoQR(string Text_QR, string LastName, string FirstName, string MidleName, string Email, string Phone_namber)
        {
            if (!string.IsNullOrEmpty(Text_QR))
            {
                try
                {

                    string problem_data = string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Phone_namber) ? "Отсутсвует почта и номер телефона!" :
                                        string.IsNullOrEmpty(Email) ? "Отсутствует почта!" :
                                        string.IsNullOrEmpty(Phone_namber) ? "Отсутствует номер телефона!" :
                                        "Данные корректны!";

                    // Формируем полный путь до файла с QR-кодом
                    string filepath_QR = Path.Combine(FilePath_folder_photos_for_event, $"QR-фото для ({LastName} {FirstName} {MidleName}).png");

                    // Генерация QR-кода
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(Text_QR, QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode bitmapQRCode = new BitmapByteQRCode(qrCodeData);

                    // Если файл уже существует, удаляем его
                    if (File.Exists(filepath_QR))
                    {
                        File.Delete(filepath_QR);
                    }

                    // Записываем файл с QR-кодом
                    await Task.Run(() => File.WriteAllBytes(filepath_QR, bitmapQRCode.GetGraphic(20)));
                    await LogMessage($"Гость: {LastName} {FirstName} {MidleName}.\nПроблемы с данными(или их отсутствие если написано \"Данные корректны!\"): {problem_data}", "upload_people");
                    await LogMessage($"Фото Qr кода для гостя ({LastName} {FirstName} {MidleName}) успешно создано.\n \nНазвание файла QR-фото \"QR-фото для ({LastName} {FirstName} {MidleName}).png\"", "system");
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании фото Qr кода для гостя: {ex.Message}");
                    await LogMessage($"Ошибка при создании фото Qr кода для гостя.\n{ex.Message}", "system");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Ошибка генерации текста QR: текст не должен быть пустым.");
                return false;
            }
        }

        public async Task Getting_list_people_from_division()
        {
            try 
            {
                var client = new IntegrationServiceSoapClient();
                object value = Name_Division;
                object value1 = null;
                var answer = await client.PersonSearchAsync(Session_ID,Guid.Parse("0de358e0-c91b-4333-b902-000000000004"),0, value, value1);
                var res = answer.Body.PersonSearchResult;
                if (res != null)
                {
                    List_people_from_didvision = new List<Person>();
                    List_people_from_didvision = res.ToList<Person>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Проблема со списком людей из подразделения: {ex.Message}");
                
            }

        }

        public async Task Checking_availability_deletion(string Lastname, string Firstname,string Middlename)
        {
            try
            {
                //await LogMessage($"запуск алгоритма", "delete_people");
                var check_availability = (from p in List_people_from_didvision where p.LAST_NAME == Lastname && p.FIRST_NAME == Firstname && p.MIDDLE_NAME == Middlename select new Person {ID = p.ID, LAST_NAME = p.LAST_NAME, FIRST_NAME = p.FIRST_NAME, MIDDLE_NAME = p.MIDDLE_NAME}).FirstOrDefault();
                if (check_availability != null)
                {
                    string FIO = $"{check_availability.LAST_NAME} {check_availability.FIRST_NAME} {check_availability.MIDDLE_NAME}";
          
                    var client = new IntegrationServiceSoapClient();
                    var answer = await client.GetPersonExtraFieldValuesAsync(Session_ID, check_availability.ID);
                    var res = answer.Body.GetPersonExtraFieldValuesResult;
                    // res.All(p => p.TEMPLATE_ID == Method_entry_column_ID && p.VALUE.ToString() == "Программно" && p.TEMPLATE_ID == Date_start_end_column_ID && DateTime.TryParse(((p.VALUE.ToString()).Split(' '))[3], out DateTime parsedDate) && parsedDate <= now)
                    /*
                    bool check_metod = false, check_date = false;

                    foreach (var i in res)
                    {
                        if (i.TEMPLATE_ID == Method_entry_column_ID && i.VALUE.ToString() == "Программно") check_metod = true;
                        if (i.TEMPLATE_ID == Date_start_end_column_ID  && Convert.ToDateTime(((i.VALUE.ToString()).Split(' '))[3]).Date <= DateTime.Now.Date) check_date = true;
                    }
                    */
                    bool check_metod = res.Any(i => i.TEMPLATE_ID == Method_entry_column_ID && i.VALUE.ToString() == "Программно");

                    // Проверяем, что дата не превышает текущую дату для Date_start_end_column_ID
                    bool check_date = res.Any(i =>
                        i.TEMPLATE_ID == Date_start_end_column_ID &&
                        DateTime.TryParse(((i.VALUE.ToString()).Split(' '))[3], out DateTime parsedDate) &&
                        parsedDate.Date <= DateTime.Now.Date);
                    if (check_metod || check_date)
                    {
                        //MessageBox.Show($"Можно удалять");
                        var delete_person = await client.DeletePersonAsync(Session_ID, check_availability.ID);
                        var res_delete = delete_person.Body.DeletePersonResult;
                        // запись данных в основной лог файл и лог с Фио удаленных
                        await LogMessage($"{FIO} найден и удален", "delete_people");
                        await LogMessage($"{FIO} удален из базы ParsecNet3!", "system");
                        
                    }


                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Произошла ошибка при получении организационных единиц с посетителями: {ex.Message}");
                await LogMessage($"Ошибка!\n{ex.Message}", "system");

            }

        }

        public async Task GetVisitorExtraFieldTemplates(Guid sessionID)
        {
            try
            {
                var client = new IntegrationServiceSoapClient();
                var result = await client.GetVisitorExtraFieldTemplatesAsync(sessionID);
                var res = result.Body.GetVisitorExtraFieldTemplatesResult;
                if (res != null)
                {
                    foreach (var i in res)
                    {
                        switch (i.NAME)
                        {
                            case "Почта":
                                Email_column_ID = i.ID;
                                break;
                            case "Телефон":
                                Phone_column_ID = i.ID;
                                break;
                            case "Пригласивший":
                                Who_inveted_column_ID = i.ID;
                                break;
                            case "Даты начала и конца":
                                Date_start_end_column_ID = i.ID;
                                break;
                            case "Способ занесения":
                                Method_entry_column_ID = i.ID;
                                break;
                        }
                        //MessageBox.Show($"{i.NAME}:{i.ID}");
                    }
                }
                else
                {
                    MessageBox.Show("Null");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при получении организационных единиц с посетителями: {ex.Message}");
            }
        }

        public async Task<bool> SetPersonExtraFieldValues(Guid person_session, string email,string phone,string date_start_end, string who_invited)
        {
            try
            {
                var client = new IntegrationServiceSoapClient();

                ExtraFieldValue[] extra_field = new ExtraFieldValue[5];
                extra_field[0] = new ExtraFieldValue(); 
                extra_field[0].TEMPLATE_ID = Email_column_ID;
                extra_field[0].VALUE = email;

                extra_field[1] = new ExtraFieldValue();
                extra_field[1].TEMPLATE_ID = Phone_column_ID;
                extra_field[1].VALUE = phone;

                extra_field[2] = new ExtraFieldValue();
                extra_field[2].TEMPLATE_ID = Date_start_end_column_ID;
                extra_field[2].VALUE = date_start_end;

                extra_field[3] = new ExtraFieldValue();
                extra_field[3].TEMPLATE_ID = Who_inveted_column_ID;
                extra_field[3].VALUE = who_invited;

                extra_field[4] = new ExtraFieldValue();
                extra_field[4].TEMPLATE_ID = Method_entry_column_ID;
                extra_field[4].VALUE = "Программно";

                var result = await client.SetPersonExtraFieldValuesAsync(person_session, extra_field);
                var res = result.Body.SetPersonExtraFieldValuesResult;
                if (res.Result == 0)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show($"{res.ErrorMessage}");
                    return false;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при получении организационных единиц с посетителями: {ex.Message}");
                return false;
            }
        }

        public void delete_and_create_file()
        {
            try
            {
                if (Directory.Exists(FilePath_folder_photos_for_event))
                {
                    foreach (string file in Directory.GetFiles(FilePath_folder_photos_for_event))
                    {
                        File.SetAttributes(file, FileAttributes.Normal); // Снять защитные атрибуты
                        File.Delete(file); // Удалить файл
                    }
                }
                else Directory.CreateDirectory(FilePath_folder_photos_for_event); // Создать новую папку
                
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Недостаточно прав для удаления папки: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка ввода-вывода: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
            }

        }
    }
}