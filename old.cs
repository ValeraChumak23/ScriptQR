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
      flag_add_Email_and_Phone = await SetPersonExtraFieldValues(personSession, personData.Email, personData.Phone_number);
      await SetIdentifierPrivileges(Session_ID, Identifier.CODE);
      await ClosePersonEditingSession(personSession);

      if (flag_createVisitor && flag_addPersonIdentifier) flag_dounload = true;

      // Обновляем данные в UI
      await Application.Current.Dispatcher.InvokeAsync(() =>
      {
          res.UpdatePerson(num, flag_dounload.ToString(), "Ожидание", "Ожидание");
      });

      var Qr_code_text = await GenerateParsecQRCode(Session_ID, Identifier.CODE, personData.LastName, personData.FirstName, personData.MiddleName);
      if (Qr_code_text != "")
      {
          flag_generatePhotoQR = await GeneratePhotoQR(Qr_code_text, personData.LastName, personData.FirstName, personData.MiddleName, documentsPath);
          if (flag_generatePhotoQR)
          {
              await Application.Current.Dispatcher.InvokeAsync(() =>
              {
                  res.UpdatePerson(num, flag_dounload.ToString(), flag_generatePhotoQR.ToString(), "Ожидание");
              });
              flag_SendPhotoQR = await SendEmailsAsync(personData.Email, documentsPath, personData.Purpose_Visit, FIO, (personData.Date_Start).ToString(), (personData.Date_End).ToString());
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