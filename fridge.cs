using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;

class Program
{
    // Создаем ОДИН РАЗ на уровне класса. 
    // static — чтобы его видели статичные функции (как Main).
    // readonly — защита, чтобы случайно не стереть его в коде.
    private static readonly Random _rnd = new Random();

    //нужны для перерисовки окна, если его уменьшат    
    private static readonly int _windowWidth = 100; //вместо 120
    private static readonly int _windowHeight = 30;

    //чтобы заблокировать изменение размера окна
    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();


    static void Main()
    {
        Console.WindowWidth = _windowWidth;
        Console.BufferWidth = _windowWidth;
        Console.WindowHeight = _windowHeight;
        Console.BufferHeight = _windowHeight;

        // --- БЛОКИРОВКА РАЗМЕРА ОКНА ---
        IntPtr handle = GetConsoleWindow(); // Получаем идентификатор окна нашей консоли
        IntPtr sysMenu = GetSystemMenu(handle, false); // Получаем системное меню этого окна

        if (handle != IntPtr.Zero)
        {
            DeleteMenu(sysMenu, 0xF000, 0x00000000); // 0xF000 — это команда SC_SIZE (изменение размера)
            DeleteMenu(sysMenu, 0xF030, 0x00000000); // 0xF030 — это команда SC_MAXIMIZE (развернуть на весь экран)
        }
        // ---------------------------------


        // Создаём словарь storage
        Dictionary<string, int> storage = new Dictionary<string, int>();
        string[] things = new string[] { "Хлеба кусок", "Пакет молока" ,"Сыра 100 г", "пиццы кусок",
            "Латяо", "Конжак" , "Конжак зел",
        "Колбаски", "Палка сырокопчёной", "Сигара",
            "Мороженка", "Конфета","Печенинка розовая",
            "Каша овсяная", "Каша 5 злаков", "Макароны", "Лапша б/п", "Фасоль", "Гречка 100 г", 
            "Водка 100 г", "Пиво" };

        //заполняем Словарь        
        for (int i = 0; i < things.Length; i++)
        {
            storage.Add(things[i], 1);            
        }

        // Локальная функция для показа меню и содержимого
        void ShowStorage()
        {
            //WindowRestore();
            Console.Clear(); // Очищаем экран
            Console.WriteLine("         продукты:          ");

            // Массив строк меню — компактно и удобно
            string[] menuLines = new string[]
            {
        "            +    Увеличить",
        "            -    Уменьшить",
        "            P    добавить позицию",
        "            N    удалить позицию",
        "            O    открыть файл",
        "            Esc  Выйти из программы"
            };

            // СИТУАЦИЯ 1: Продуктов меньше, чем пунктов меню, либо вообще продуктов нет — выводим меню фиксированно сверху справа
            if (storage.Count < menuLines.Length)
            {
                // Превращаем словарь в массив пар pairs[], чтобы обращаться по индексу (ведь foreach для пустого/короткого списка неудобен)
                KeyValuePair<string, int>[] pairs = storage.ToArray();

                // Цикл всегда идёт столько раз, сколько в меню строк
                for (int i = 0; i < menuLines.Length; i++)
                {
                    // Если продукт под таким индексом есть — пишем его, иначе — просто пустой отступ
                    if (i < pairs.Length)
                        Console.Write($"{pairs[i].Key + ":",-20}{pairs[i].Value,5} шт.");
                    else if (i == 0 && pairs.Length == 0)
                        Console.Write($"{"продуктов нет :(",-29}");
                    else
                        Console.Write($"{"",-29}");

                    // Справа всегда пристыковываем строчку меню
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(menuLines[i]);
                    Console.ResetColor();
                }
            }
            // СИТУАЦИЯ 2: Продуктов >= пунктов меню — меню плавно встраивается сбоку
            else
            {
                int i = 1;
                int iMenu = (int)((storage.Count - (menuLines.Length - 2)) * 0.7);                

                foreach (KeyValuePair<string, int> pair in storage)
                {
                    Console.Write($"{pair.Key + ":",-20}{pair.Value,5} шт.");

                    // Проверяем, попадает ли текущая строка в диапазон отрисовки меню
                    if (i >= iMenu && i < iMenu + menuLines.Length)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(menuLines[i - iMenu]);
                        Console.ResetColor();
                    }

                    Console.WriteLine();
                    i++;
                }
            }

            Console.WriteLine("--------------------------");
        }



        while (true) /////////////////// пользователь работает ///////////////////////////////////////////////////////////
        {
            ShowStorage();
            // Получаем объект ConsoleKeyInfo
            ConsoleKey key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Add || key == ConsoleKey.OemPlus) //******************увеличить **********************************************
            {
                //WindowRestore();
                //добавляем
                var result = ParseProductAndCount("Увеличим что и сколько? (либо Стрелка вниз): ", things);

                string str = result.product;
                int count = result.count;

                //string str = InputStringWithHints("увеличим что и сколько? (либо Стрелка вниз): ", things, 20);
                //int count = InputNumberInt("введи количество: ", 0, 10000);

                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);

                if (count == 0 || !storage.ContainsKey(str))
                {
                    Console.Write("    добавил 0... " );
                }
                else if (storage.ContainsKey(str))
                {
                    storage[str] += count; //приплюсуем к элементу Словаря
                    Console.Write($"    добавлены {count} \"{str}\" " );
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                System.Threading.Thread.Sleep(1700);                
            }


            else if (key == ConsoleKey.Subtract || key == ConsoleKey.OemMinus) //******************вычесть ******************************************
            {
                //WindowRestore();
                if (storage.Count == 0)  //хол-к пуст           
                    continue;                

                string str = InputStringWithHints("уменьшим что и сколько? (либо Стрелка вниз): ", storage.Keys.ToArray(), 20);               
                int count = InputNumberInt("введи количество товара: ", 0);

                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                Console.Write(new string(' ', Console.WindowWidth * 2));
                Console.SetCursorPosition(0, Console.CursorTop - 2);

                //если человек написал 0, либо нет такого продукта, либо продукт закончился, то не вычитаем
                if (count == 0 || !storage.ContainsKey(str) || storage[str] == 0)
                    Console.Write("    вычли 0...");
                else if (storage.ContainsKey(str)) //иначе вычитаем
                {
                    if(count > storage[str])
                    {
                        Console.Write($"    съедены {storage[str]} \"{str}\"");
                        storage[str] = 0;                        
                    }                        
                    else
                    {
                        Console.Write($"    съедены {count} \"{str}\"");
                        storage[str] -= count; //вычтем у элемента Словаря 
                    }
                }
                Console.SetCursorPosition(0, Console.CursorTop);
                System.Threading.Thread.Sleep(1700);                
            }


            if (key == ConsoleKey.P) //******************добавить позицию**********************************************
            {


                //добавляем
                string str = InputStringWithHints("введи наименование для добавления: ", things, 19);
                int count = InputNumberInt("введи количество товара: ", 0);

                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                Console.Write(new string(' ', Console.WindowWidth * 2));
                Console.SetCursorPosition(0, Console.CursorTop - 2);


                if (!storage.ContainsKey(str))
                {
                    storage[str] = count;  //добавим в Словарь
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(0);
                }
                else
                {                    
                    Console.Write($"    \"{str}\" уже есть на складе!");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1100);
                }
            }


            else if (key == ConsoleKey.N) //******************удалить позицию******************************************
            {
                if (storage.Count == 0)
                {                    
                    if (storage.Count == 0)
                        Console.Write("    нечего удалять, лучше добавь");

                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(2700);
                    ClearUserErrors(0, "", "", 0);
                    ShowStorage();
                    continue;
                }

                string str = InputStringWithHints("что удалить? (либо Стрелка вниз): ", storage.Keys.ToArray(), 20);

                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);

                //товара нет в словаре storage?
                if (!storage.ContainsKey(str))
                {
                    Console.Write($"    \"{str}\" на складе нет..");
                    
                }
                else
                {
                    storage.Remove(str);
                    Console.Write($"    \"{str}\" удалены...");
                }


                Console.SetCursorPosition(0, Console.CursorTop);
                System.Threading.Thread.Sleep(1000);                
            }


            else if (key == ConsoleKey.Escape) //******************выход**************************************************
            {
                WindowRestore();
                Console.Write("      до скорого! ");
                System.Threading.Thread.Sleep(500);
                break;
            }
        }
    }

    static int InputNumberInt(string inputMessage, int min = -2147483648, int max = 2147483646)
    {
        int number;
        Console.Write(inputMessage);

        while (true)
        {
            WindowRestore();
            int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
            string str = Console.ReadLine();

            // проверка на null (Ctrl+Z)
            if (str == null)
            {
                ClearUserErrors(startingCursorTop, inputMessage, "   чё за х...? ", 1700);
                continue; // Возврат в начало цикла для нового ввода
            }

            //если чистый enter, то Рандом
            if (str.Length == 0)
            {
                if (max == int.MaxValue)
                    max = int.MaxValue - 1;  //чтобы не переполнить рандом
                number = _rnd.Next(min, max + 1);
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                Console.Write(number + "\n");
                break;
            }

            // Если одни пробелы       
            if (string.IsNullOrWhiteSpace(str))
            {
                ClearUserErrors(startingCursorTop, inputMessage, "   -введи чё-нибудь, или нажми Enter-");
                continue; // Возврат в начало цикла для нового ввода
            }

            //Чистим строку            
            str = str.Replace(" ", "");

            // Узнаём, какой разделитель используется в текущей Windows (точка или запятая)
            string currentSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (currentSeparator == ",")
                str = str.Replace(".", ",");
            else if (currentSeparator == ".")
                str = str.Replace(",", ".");

            // Проверка на то, что введено вообще число
            if (!double.TryParse(str, out double parsedDouble))
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"     -Это не число!-");
                continue; // Возврат в начало цикла для нового ввода
            }

            if (parsedDouble > int.MaxValue || parsedDouble < int.MinValue)
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"    Сдурел? Чё такое длинное? Введи нормальное");
                continue; // Возврат в начало цикла для нового ввода
            }

            // Проверка на то, что введено именно целое число
            if (parsedDouble % 1 != 0)
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"    -Неeт! Введи ЦЕЛОЕ число-");
                continue; // Возврат в начало цикла для нового ввода
            }
            number = (int)parsedDouble;

            // Проверка на попадание в диапазон
            if (number < min || number > max)
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"   Число должно быть в пределах: ({min}...{max})", 2300);
                continue; // Возврат в начало цикла для нового ввода
            }

            ClearUserErrors(startingCursorTop, inputMessage, "", 0);
            Console.Write(number + "\n");
            break;
        }

        return number;
    }
    static string InputString(string inputMessage, string[] random, int max = 60)
    {
        string str;
        Console.Write(inputMessage);

        while (true)
        {
            WindowRestore();
            int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
            str = Console.ReadLine();

            // проверка на null (Ctrl+Z)
            if (str == null)
            {
                ClearUserErrors(startingCursorTop, inputMessage, "   чё за х...? ", 1700);
                continue; // Возврат в начало цикла для нового ввода
            }

            //если чистый enter, то Рандом
            if (str.Length == 0)
            {
                // если массив забыли заполнить или он пустой
                if (random == null || random.Length == 0)
                {
                    str = $"Объект {_rnd.Next(1, 1000)}";
                }
                else
                {
                    int number = _rnd.Next(0, random.Length);
                    str = random[number];
                }
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                Console.Write(str + "\n");
                break;
            }

            // Если одни пробелы       
            if (string.IsNullOrWhiteSpace(str))
            {
                ClearUserErrors(startingCursorTop, inputMessage, "   -введи чё-нибудь, или нажми Enter-");
                continue; // Возврат в начало цикла для нового ввода
            }

            //Чистим строку
            str = str.Trim();
            //только первая буква заглавная
            str = str.ToLower();
            str = str.Substring(0, 1).ToUpper() + str.Substring(1);
            //разбиваем строку по пробелам, игнорируя пустые элементы
            string[] words = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //сшиваем слова обратно, разделяя их ОДНИМ пробелом
            str = string.Join(" ", words);

            if (str.Length > max)
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"   -название не больше {max} символов!-");
                continue; // Возврат в начало цикла для нового ввода
            }
            if (!str.Any(char.IsLetter))
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"   -буквы тоже должны быть в названии!-");
                continue; // Возврат в начало цикла для нового ввода
            }

            ClearUserErrors(startingCursorTop, inputMessage, "", 0);
            Console.Write(str + "\n");
            break;
        }
        return str;
    }
    static string InputStringWithHints(string inputMessage, string[] hints, int max = 60)
    {
        string str = "";
        string userStr = ""; // хранит ТОЛЬКО то, что вбито руками
        int hintsIndex = -1; // Индекс подсказки
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
        Console.Write(inputMessage);

        // ЦИКЛ 2: Посимвольный ввод и сборка строки
        while (true)
        {
            // ЦИКЛ 1: Сбор букв до нажатия Enter
            while (true)
            {
                WindowRestore();
                var keyInfo = Console.ReadKey(true);

                // нажат ENTER — завершаем ввод
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                    break;
                }

                // нажата СТРЕЛКА ВНИЗ — листаем подсказки
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (hints == null || hints.Length == 0) continue;

                    // Листаем индекс по кругу
                    //hintsIndex++;
                    //if (hintsIndex >= hints.Length) hintsIndex = 0;

                    hintsIndex = FindBestHints(userStr, hints, hintsIndex);

                    // Стираем старый ввод с экрана
                    ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                    // Подставляем значение из массива
                    str = hints[hintsIndex];

                    // Печатаем новую подсказку
                    Console.Write(str);
                }

                // нажат BACKSPACE — удаляем символ
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (str.Length > 0)
                    {
                        // Если на экране была длинная подсказка, а пользователь нажал Backspace,
                        // логично стереть подсказку и вернуться к тому, что он вводил руками
                        if (str != userStr)
                        {
                            str = userStr;
                        }

                        // Стираем один настоящий символ
                        if (str.Length > 0)
                        {
                            str = str.Substring(0, str.Length - 1);
                            userStr = str; // Синхронизируем запрос
                        }

                        // Перерисовываем экран через ClearUserErrors, чтобы убрать хвост подсказки
                        ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                        Console.Write(str);
                    }
                }

                // нажата обычная буква или знак — печатаем
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    // Если до этого стояла автоподсказка, и пользователь нажал букву,
                    // он продолжает вводить СВОЙ текст, а не дописывает подсказку
                    if (str != userStr)
                    {
                        str = userStr;
                    }

                    str += keyInfo.KeyChar;
                    userStr = str; // Запоминаем, что это ввёл именно пользователь

                    Console.Write(keyInfo.KeyChar);
                }
            }

            //ЦИКЛ 2: Валидация строки

            //если чистый enter, то Рандом
            if (str.Length == 0)
            {
                // если массив забыли заполнить или он пустой
                if (hints == null || hints.Length == 0)
                {
                    str = $"Объект {_rnd.Next(1, 1000)}";
                }
                else
                {
                    int number = _rnd.Next(0, hints.Length);
                    str = hints[number];
                }
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                Console.Write(str + "\n");
                break;
            }

            // Если одни пробелы       
            if (string.IsNullOrWhiteSpace(str))
            {
                ClearUserErrors(startingCursorTop, inputMessage, "   введи чё-нибудь, или нажми Enter или Стрелку вниз");
                str = "";
                continue; // Возврат в начало цикла для нового ввода
            }

            //Чистим строку
            str = str.Trim();
            //только первая буква заглавная
            //str = str.ToLower();
            //str = str.Substring(0, 1).ToUpper() + str.Substring(1);
            //разбиваем строку по пробелам, игнорируя пустые элементы
            string[] words = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //сшиваем слова обратно, разделяя их ОДНИМ пробелом
            str = string.Join(" ", words);

            if (str.Length > max)
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"   -название не больше {max} символов!-");
                str = "";
                continue; // Возврат в начало цикла для нового ввода
            }
            if (!str.Any(char.IsLetter))
            {
                ClearUserErrors(startingCursorTop, inputMessage, $"   -буквы тоже должны быть в названии!-");
                str = "";
                continue; // Возврат в начало цикла для нового ввода
            }

            ClearUserErrors(startingCursorTop, inputMessage, "", 0);
            Console.Write(str + "\n");
            break;
        }

        return str;
    }

    static (string product, int count) ParseProductAndCount(string inputMessage, string[] hints)
    {
        Console.Write(inputMessage);

        string str = ""; // То, что реально написано на экране
        string userStr = ""; // хранит ТОЛЬКО то, что вбито руками
        int hintsIndex = -1; // Индекс подсказки
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода

        // ЦИКЛ 1: Сбор букв до нажатия Enter
        while (true)
        {            
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // 1. нажат ENTER — завершаем ввод
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                break;
            }

            // 2. нажата СТРЕЛКА ВНИЗ — листаем подсказки (работает, только если в конце ЕЩЁ НЕТ цифр)
            else if (keyInfo.Key == ConsoleKey.DownArrow && !str.Any(char.IsDigit))
            {
                if (hints == null || hints.Length == 0) continue;

                hintsIndex = FindBestHints(userStr, hints, hintsIndex);

                // Стираем старый ввод с экрана
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);                

                // Подставляем значение из массива
                str = hints[hintsIndex];

                // Печатаем новую подсказку
                Console.Write(str);
            }

            // 3. нажат BACKSPACE — удаляем символ
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (str.Length > 0)
                {
                    // Если на экране была длинная подсказка, а пользователь нажал Backspace,
                    // логично стереть подсказку и вернуться к тому, что он вводил руками
                    if (str != userStr)
                    {
                        str = userStr;
                    }

                    // Стираем один настоящий символ с конца строки
                    if (str.Length > 0)
                    {
                        str = str.Substring(0, str.Length - 1);

                        // Если мы стёрли цифры и вернулись к буквам, синхронизируем userStr
                        if (!str.Any(char.IsDigit))
                        {
                            userStr = str;
                        }
                    }
                    
                    // Перерисовываем экран через ClearUserErrors, чтобы убрать хвост подсказки
                    ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                    Console.Write(str);
                }
            }

            // 4. НАЖАТА ЦИФРА (0-9)
            else if (char.IsDigit(keyInfo.KeyChar))
            {
                // Разрешаем вводить цифры, только если буквы УЖЕ выбраны (строка не пустая)
                if (str.Length > 0)
                {
                    str += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
            }

            // 5. нажата обычная буква или знак (разрешаем, только если цифры ЕЩЁ НЕ начались)
            else if (!char.IsControl(keyInfo.KeyChar) && !str.Any(char.IsDigit))
            {
                str += keyInfo.KeyChar;
                userStr = str; // Запоминаем, что это ввёл именно пользователь
                Console.Write(keyInfo.KeyChar);
            }
        }

        //--- ФИНАЛЬНЫЙ РАЗБОР СТРОКИ ПОСЛЕ ENTER ---

        // Разделяем строку на буквы и цифры
        string productPart = new string(str.Where(c => !char.IsDigit(c)).ToArray()).Trim();
        string digitsPart = new string(str.Where(c => char.IsDigit(c)).ToArray());

        // Валидация: проверяем, что продукт из получившейся строки РЕАЛЬНО есть в нашем массиве
        // (на случай, если пользователь просто написал буквы руками и нажал Enter, не используя стрелку)
        string finalProduct = hints.FirstOrDefault(h => CleanForSearch(h) == CleanForSearch(productPart));

        // Если продукт не найден в списке, возвращаем пустую строку
        if (finalProduct == null)
        {
            return ("", 0);
        }

        // Переводим цифры в число int (если цифр не было, запишем 0)
        int finalCount = 0;
        if (!string.IsNullOrEmpty(digitsPart))
        {
            int.TryParse(digitsPart, out finalCount);
        }

        return (finalProduct, finalCount);
    }

    // функция для очистки ВСЕХ неверных данных введённых пользователем и вывода временного сообщения об ошибке
    static void ClearUserErrors(int startingCursorTop, string inputMessage, string errorMessage = "   -Неверно! Читай внимательнее!-", int pauseTime = 1900)
    {
        WindowRestore();

        // Проходим снизу вверх от текущей позиции до начальной
        for (int i = Console.CursorTop; i >= startingCursorTop; i--)
        {
            Console.SetCursorPosition(0, i);
            Console.Write(new string(' ', Console.WindowWidth)); // Очищаем всю строку целиком
        }
        Console.SetCursorPosition(0, startingCursorTop); // Возвращаем курсор в начало для новой попытки        

        Console.Write(inputMessage);
        int lengt1 = inputMessage.Length;

        if (pauseTime > 0)
        {
            Console.Write(errorMessage);
            int lengt2 = errorMessage.Length;
            Console.SetCursorPosition(lengt1, Console.CursorTop);
            System.Threading.Thread.Sleep(pauseTime);
            Console.Write(new string(' ', lengt2));
            Console.SetCursorPosition(lengt1, Console.CursorTop);
        }
    }
    static void WindowRestore() //нужно для перерисовки окна, если его уменьшат
    {
        try
        {
            // Если размеры окна изменились, жестко возвращаем их к эталону
            if (Console.WindowWidth != _windowWidth)
            {
                Console.WindowWidth = _windowWidth;
                Console.BufferWidth = _windowWidth; // Синхронизируем буфер, чтобы убрать скроллбар
            }
            if (Console.WindowHeight != _windowHeight)
            {
                Console.WindowHeight = _windowHeight;
                Console.BufferHeight = _windowHeight; // Синхронизируем буфер
            }

            // Принудительно сбрасываем ползунки окна в самый верхний левый угол (0, 0)
            // Чтобы текст никогда не уплывал за пределы видимости
            if (Console.WindowTop != 0 || Console.WindowLeft != 0)
            {
                Console.SetWindowPosition(0, 0);
            }
        }
        catch
        {
            // Защита от капризов Windows при мгновенном изменении размеров окна
        }

    }
    static string CleanForSearch(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        // Оставляем только буквы и цифры, переводим в нижний регистр
        return new string(str.Where(char.IsLetterOrDigit).ToArray()).ToLower();
    }
    static int FindBestHints(string str, string[] hints, int currentIndex)
    {
        string strClean = CleanForSearch(str);

        // Если пользователь ничего не ввёл, просто двигаемся к следующему элементу массива по кругу (в случае превышения индекс = 0)
        if (string.IsNullOrEmpty(strClean))
        {
            return (currentIndex + 1) % hints.Length;
        }

        // Собираем ИНДЕКСЫ всех слов, которые начинаются на наш запрос
        List<int> matchedIndices = new List<int>();
        for (int i = 0; i < hints.Length; i++)
        {
            string hintClean = CleanForSearch(hints[i]);
            if (hintClean.StartsWith(strClean)) // Ищем совпадение С НАЧАЛА СЛОВА
            {
                matchedIndices.Add(i);
            }
        }

        // Если нашли подходящие слова (например, для "ст" это будут индексы Стола и Стула)
        if (matchedIndices.Count > 0)
        {
            // Ищем, есть ли среди найденных индексов тот, который идёт ПОСЛЕ текущего currentIndex
            foreach (int index in matchedIndices)
            {
                if (index > currentIndex)
                {
                    return index; // Возвращаем первое подходящее слово впереди
                }
            }

            // Если мы уже стояли на самом последнем подходящем слове (или currentIndex был вообще в другом месте),
            // то сбрасываемся на САМОЕ ПЕРВОЕ слово из нашего отфильтрованного списка (зацикливаем поиск)
            return matchedIndices[0];
        }

        // Если совпадений вообще нет, просто листаем весь массив дальше по кругу (в случае превышения индекс = 0)
        return (currentIndex + 1) % hints.Length;
    }


}

