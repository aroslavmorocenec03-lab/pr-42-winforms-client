# Відповіді на контрольні запитання

У завданні тричі повторено номер 5. Нижче вісім запитань подано послідовно. У запитанні про CS159 мається на увазі **CS1591** — відсутність XML-коментаря.

## 1 Призначення документації та відмінність коментарів

Документація пояснює призначення типів і методів, правила використання API, параметри, результати, обмеження та можливі помилки. Вона допомагає підтримувати код і створювати клієнти сервісу. Звичайні `//` і `/* ... */` коментарі пояснюють реалізацію всередині коду. Документаційні коментарі C# зазвичай починаються з `///` і містять структуровані XML-теги: `<summary>`, `<remarks>`, `<param>`, `<returns>`, `<exception>`. Їх обробляють компілятор, IntelliSense та генератори документації. Сам компілятор створює XML, а HTML-сторінку формує окремий інструмент.

Джерело: [Microsoft — XML API documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/).

## 2 Коли виникає CS1591

CS1591 виникає, коли ввімкнено генерування XML-документації (`GenerateDocumentationFile` або параметр `DocumentationFile`), а публічно доступний тип або його член не має документаційного коментаря. Діагностика має бути ввімкнена відповідним рівнем попереджень і не приглушена. Це стосується, наприклад, публічних класів DTO та їхніх властивостей. Звичайний `//` коментар її не усуває. Для доступних лише всередині збірки реалізацій вимога CS1591 не така сама, як для публічного API.

Джерело: [Microsoft — CS1591](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591).

## 3 Коли та як приглушувати CS1591

Приглушення доречне для згенерованого коду або ділянок, які свідомо не входять до документованого публічного контракту. Для DTO та ендпойнтів цієї роботи слід додати опис. Найвужчий варіант — `#pragma warning disable CS1591` перед потрібною ділянкою та `#pragma warning restore CS1591` після неї, із поясненням причини. Для всього проєкту в `.csproj` можна задати:

```xml
<NoWarn>$(NoWarn);CS1591</NoWarn>
```

`$(NoWarn)` зберігає раніше задані коди. Вимикати всі попередження через `WarningLevel=0` заради одного CS1591 недоцільно. У виконаній роботі CS1591 не приглушено: він включений до `WarningsAsErrors`, щоб неповна документація контрактів зупиняла збірку.

Джерело: [Microsoft — NoWarn та налаштування попереджень](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#nowarn).

## 4 Як увімкнути генерування XML-файлу

У `.csproj` усередині `PropertyGroup` потрібно встановити:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

Після збірки поруч із DLL створюється однойменний XML-файл. Шлях можна змінити властивістю `DocumentationFile`. Аналогічне налаштування доступне у властивостях проєкту Visual Studio, у параметрах збірки та XML-документації. Якщо DTO винесено в інший проєкт, генерування слід увімкнути і там.

Джерело: [Microsoft — генерування XML-документації](https://learn.microsoft.com/en-us/visualstudio/ide/reference/generate-xml-documentation-comments).

## 5 Що перевірити за відсутності опису ендпойнту у Swagger

У файлі проєкту API слід увімкнути `GenerateDocumentationFile`, а в `Program.cs`, усередині `AddSwaggerGen`, підключити XML-файл збірки API через `IncludeXmlComments`. XML має бути поруч із запущеною DLL, зокрема після публікації. Для Minimal API також потрібен `AddEndpointsApiExplorer`. Коментарі `<summary>`, `<remarks>` і `<param>` мають належати методу обробника. Коментар над довільним викликом `MapGet` не документує лямбду автоматично. Альтернатива для лямбд — `.WithSummary(...)` та `.WithDescription(...)`; параметри й відповіді також потрібно описати метаданими або фільтрами.

У цій роботі використано іменовані обробники `PostingEndpoints`, XML-теги та `.Produces(...)`. Слід перевіряти не лише інтерфейс, а й поля `summary`, `description`, `parameters` у `/swagger/v1/swagger.json`.

Джерела: [Microsoft — Swashbuckle](https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle), [Swashbuckle — XML comments](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v10.3.1/docs/configure-and-customize-swaggergen.md#include-descriptions-from-xml-comments).

## 6 Що перевірити за відсутності опису схеми даних

Слід додати `<summary>` до класу DTO та **кожної** його властивості, ввімкнути XML-генерацію у проєкті DTO і підключити саме XML збірки DTO у конфігурації Swagger. Підключення лише XML сервера недостатньо для контрактів в окремій бібліотеці. У виконаній роботі підключено три файли: `PostService.xml`, `PostService.Dtos.xml`, `PostService.CommonTypes.xml`. Шлях визначається через `AppContext.BaseDirectory`, тому не залежить від робочого каталогу.

```csharp
options.IncludeXmlComments(Path.Combine(
    AppContext.BaseDirectory, "PostService.Dtos.xml"));
options.UseAllOfToExtendReferenceSchemas();
```

Другий виклик дозволяє додавати описи до властивостей із посиланням на спільну схему, наприклад `DeliveryType`. Після змін потрібно перебудувати й перезапустити сервіс та перевірити `components.schemas` у JSON документації.

Джерело: [Swashbuckle — документування моделей та reference schemas](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v10.3.1/docs/configure-and-customize-swaggergen.md).

## 7 Що таке JSDoc і які теги використовуються

JSDoc — формат коментарів і генератор документації для JavaScript. Блок `/** ... */` розміщують перед функцією. Призначення можна подати звичайним текстом або через `@description`; `@function` чи `@method` позначає вид елемента, коли його потрібно уточнити; `@param {тип} назва` описує параметр; `@returns {тип}` — результат. Також корисні `@example`, `@throws`, `@typedef` і `@property`.

```javascript
/**
 * Обчислює навчальний тариф доставки у відділення.
 * @param {number} weight - Вага відправлення в кілограмах.
 * @returns {number} Вартість у гривнях.
 */
function calculatePrice(weight) {
  return 40 + 10 * weight;
}
```

Приклад ілюструє синтаксис JSDoc; у PostService розрахунок ціни залишається на C# сервері.

Джерела: [JSDoc — початок роботи](https://jsdoc.app/about-getting-started), [@param](https://jsdoc.app/tags-param), [@returns](https://jsdoc.app/tags-returns).

## 8 Відмінності XML-документації C# та JSDoc

| Ознака | C# XML | JSDoc |
|---|---|---|
| Синтаксис | `///` із XML-тегами | `/** ... */` із тегами `@...` |
| Типи | Беруться із сигнатур C# | Можуть явно вказуватися в коментарях, наприклад `{number}` |
| Генерація | Компілятор створює XML; HTML формують інші засоби | JSDoc аналізує JavaScript і може створити HTML |
| Контроль | Компілятор перевіряє частину тегів і може видати CS1591 | Перевірка залежить від JSDoc, редактора чи додаткового аналізатора |

Обидва підходи описують призначення функцій, параметри, результат і приклади. Самі документаційні коментарі не виконують перевірку значень під час роботи програми та не замінюють реалізацію валідації.

Джерела: [Microsoft — XML API comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/), [JSDoc](https://jsdoc.app/about-getting-started).
