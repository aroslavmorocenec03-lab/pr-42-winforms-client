# ПР-51 — Swagger документація PostService

До наявного REST API поштових відправлень додано Swagger UI та OpenAPI специфікацію. Задокументовано **6 операцій**, вхідні параметри й тіла запитів, коди відповідей, **3 класи DTO та всі 35 їхніх властивостей**.

## Матеріали для викладача

- [Звіт Word із виконанням роботи та відповідями](swagger-submission/PR51-Swagger-report.docx)
- [Відповіді на всі 8 контрольних запитань](swagger-submission/THEORY.md)
- [Згенерований OpenAPI JSON](swagger-submission/openapi.json)
- [Журнал 73 HTTP та OpenAPI перевірок](swagger-submission/checks.txt)
- [Ті самі 73 перевірки опублікованого сервера](swagger-submission/published-checks.txt)
- [Збірка та публікація без попереджень](swagger-submission/publish-results.txt)

Готовий комплект Windows x64 з runtime .NET, звітом і вихідним кодом передається окремим ZIP-архівом. На GitHub розміщено вихідний код і матеріали роботи. Попередні матеріали Windows Forms збережено в каталозі `submission/`; матеріали цієї практичної розташовані в `swagger-submission/`.

## Запуск із вихідного коду

Потрібен .NET SDK 10. З кореня репозиторію:

```powershell
dotnet build PostServiceSolution/PostService.slnx -warnaserror
dotnet run --project PostServiceSolution/PostService/PostService.csproj --no-build
```

Відкрийте **http://localhost:5211/swagger/index.html**.

Специфікація доступна за **http://localhost:5211/swagger/v1/swagger.json**. GitHub показує код і файл JSON; живий Swagger UI відкривається після локального запуску сервера.

## Що зроблено

1. Додано `Swashbuckle.AspNetCore` версії `10.3.1`, сумісний із .NET 10.
2. У `Program.cs` зареєстровано `AddEndpointsApiExplorer`, `AddSwaggerGen`, `UseSwagger` і `UseSwaggerUI`.
3. Обробники винесено до `PostingEndpoints.cs`. Їхні XML-коментарі описують призначення, поведінку, параметри та HTTP відповіді. Маршрути й бізнес правила збережені.
4. Для кожного ендпойнту задано унікальне ім’я операції, групу й типи відповідей через `WithName`, `WithTags` та `Produces`.
5. У проєктах сервера, DTO і спільних типів увімкнено `GenerateDocumentationFile`. Описані публічні класи, методи та властивості. CS1591 включено до `WarningsAsErrors`, а не приглушено.
6. `IncludeXmlComments` підключає **PostService.xml**, **PostService.Dtos.xml** та **PostService.CommonTypes.xml** з `AppContext.BaseDirectory`.
7. Для DTO додано описи полів, одиниці вимірювання, правила введення та приклади. Enum відображається як рядкові значення `Department`, `Courier`, `ExpressCourier`, що відповідає фактичному JSON.
8. `UseAllOfToExtendReferenceSchemas` зберігає описи властивостей, які посилаються на спільну схему.

| HTTP запит | Призначення | Коди відповідей |
|---|---|---|
| GET / | Перевірка доступності | 200 |
| GET /postings | Отримання списку | 200 |
| GET /postings/{id} | Отримання запису за номером | 200, 404 |
| POST /postings | Створення | 201, 400 |
| PUT /postings/{id} | Оновлення | 200, 400, 404 |
| DELETE /postings/{id} | Видалення | 204, 404 |

## Демонстрація у Swagger UI

1. Розгорніть операцію, прочитайте назву, опис, параметри й можливі відповіді.
2. Для POST натисніть **Try it out** та виконайте запит із прикладом:

```json
{
  "from": "Київ, відділення № 3",
  "to": "Львів, відділення № 7",
  "content": "Книги",
  "deliveryType": "Department",
  "weight": 2.5,
  "width": 20.5,
  "height": 15.25,
  "depth": 3,
  "value": 100.5,
  "description": "Не згинати"
}
```

3. Сервер поверне **201**, новий номер, дату створення та ціну **65**. Використайте повернений номер у GET, PUT і DELETE.
4. У PUT `id` у JSON має збігатися з `id` у маршруті; додайте `price`, наприклад `72.5`. Дата створення не передається і не змінюється.
5. Для перевірки помилки надішліть вагу `0`: бізнес перевірка поверне **400** із помилкою `Weight`.
6. Відкрийте **Schemas**, розгорніть `PostingGetDto`, `PostingPostDto` та `PostingPutDto` й перегляньте описи полів.

POST містить 10 полів, PUT — 12, GET — 13. Номер, дата та ціна нового запису формуються сервером. Необов’язкові цінність та опис допускають `null`. Числа в JSON записуються з крапкою.

## Перевірка

Скрипт `scripts/check-swagger.py` використовує лише стандартну бібліотеку Python 3. Він перевіряє HTML та локальні ресурси Swagger UI, покриття всіх операцій, описи параметрів і 35 властивостей DTO, сумісність enum, а також реальні POST, GET, PUT, DELETE і відповіді 400/404. Створений скриптом тестовий запис видаляється після перевірки.

Перевірку виконано через HTTP та OpenAPI JSON. Автоматичної візуальної перевірки виконання JavaScript у браузері не проводили.

Запустіть сервер з окремою тестовою базою на вільному порту:

```powershell
New-Item -ItemType Directory -Force .swagger-test-data | Out-Null
dotnet run --project PostServiceSolution/PostService/PostService.csproj --no-build -- --urls http://localhost:5318 --contentRoot "$PWD/.swagger-test-data"
```

В іншому вікні:

```powershell
python scripts/check-swagger.py http://localhost:5318
```

Перевірено також опублікований самодостатній сервер Windows x64: усі 73 перевірки пройшли, XML-файли доступні поруч із DLL, документація генерується після публікації.

## Офіційні матеріали

- [Документаційні коментарі C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [CS1591](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591)
- [Swashbuckle та XML-коментарі](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v10.3.1/docs/configure-and-customize-swaggergen.md)
- [JSDoc](https://jsdoc.app/about-getting-started)
