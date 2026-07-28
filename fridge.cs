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
        Dictionary<string, double> storage = new Dictionary<string, double>();
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
                KeyValuePair<string, double>[] pairs = storage.ToArray();

                // Цикл всегда идёт столько раз, сколько в меню строк
                for (int i = 0; i < menuLines.Length; i++)
                {
                    // Если продукт под таким индексом есть — пишем его, иначе — просто пустой отступ
                    if (i < pairs.Length)
                    {
                        if (pairs[i].Value % 1 == 0) 
                        {
                            Console.Write($"{pairs[i].Key + ":",-20}{pairs[i].Value,5} шт.");
                        }
                        else //если значение не целое, то 3 знака
                        {
                            Console.Write($"{pairs[i].Key + ":",-20}{pairs[i].Value,9:F3} г.");
                        }
                    }
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

                foreach (KeyValuePair<string, double> pair in storage)
                {
                    if (pair.Value % 1 == 0)
                    {
                        Console.Write($"{pair.Key + ":",-20}{pair.Value,5} шт.");
                    }
                    else //если значение не целое, то 3 знака
                    {
                        Console.Write($"{pair.Key + ":",-20}{pair.Value,9:F3} г.");
                    }    

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
                var result = ParseProductAndCount("Увеличим что и сколько? (Стрелка вниз, число): ", things);

                string str = result.product;
                double count = result.count;

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

                var result = ParseProductAndCount("уменьшим что и сколько? (Стрелка вниз, число): ", storage.Keys.ToArray());
                string str = result.product;
                double count = result.count;

                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);

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
                //WindowRestore();
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

                    hintsIndex = FindBestHints(userStr, hints, hintsIndex, true);

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

    static (string product, double count) ParseProductAndCount(string inputMessage, string[] hints)
    {
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
        Console.Write(inputMessage);

        string userStr = "";    // То, что пользователь набрал своими руками (до 3 букв)
        string productStr = ""; // Текстовая строка (финализируется при первой цифре)
        string countStr = "";   // Числовая строка (заполняется только цифрами)
        
        int hintsIndex = -1; //индекс подсказки

        bool isArrow = false; // Флаг: нажата ли стрелка?
        bool isDigitActive = false; // Флаг: Начали ли вводить цифры?

        // ЦИКЛ: Сбор строки до нажатия Enter
        while (true)
        {            
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // 1. нажат ENTER — завершаем ввод
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                break;
            }

            // 2. нажата СТРЕЛКА ВНИЗ — листаем подсказки
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                // Стрелка работает, только если мы ещё НЕ начали вводить цифры
                if (isDigitActive || hints == null || hints.Length == 0) continue;

                hintsIndex = FindBestHints(userStr, hints, hintsIndex, true);

                // Стираем старый ввод с экрана
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                productStr = hints[hintsIndex]; // Подставляем значение из массива
                isArrow = true;     // стрелка нажата                
                Console.Write(productStr); // Печатаем новую подсказку
            }

            // 2. нажата СТРЕЛКА ВВЕРХ — листаем подсказки в обр. сторону
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                // Стрелка работает, только если мы ещё НЕ начали вводить цифры
                if (isDigitActive || hints == null || hints.Length == 0) continue;

                hintsIndex = FindBestHints(userStr, hints, hintsIndex, false);

                // Стираем старый ввод с экрана
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                productStr = hints[hintsIndex]; // Подставляем значение из массива
                isArrow = true;     // стрелка нажата                
                Console.Write(productStr); // Печатаем новую подсказку
            }

            // 3. нажат BACKSPACE — удаляем символ
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                // Стираем цифру, если уже идёт числовая строка
                if (isDigitActive)
                {
                    if (countStr.Length > 0)
                    {
                        countStr = countStr.Substring(0, countStr.Length - 1);
                        ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                        Console.Write(productStr + countStr);
                    }                        
                }
                // в противном случае - стираем подсказку и строчку пользователя
                else
                {
                    isArrow = false;
                    if (userStr.Length > 0)
                    {
                        userStr = userStr.Substring(0, userStr.Length - 1);                        
                    }
                    ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                    Console.Write(userStr);
                }                
            }
            
            // 4. нажата ЦИФРА (0-9)
            else if (char.IsDigit(keyInfo.KeyChar))
            {
                // Разрешаем вводить цифры, если только что нажата стрелка
                if (isArrow)
                {
                    isDigitActive = true; // Текст зафиксирован
                    countStr += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
            }

            // 4. нажата точка или запятая
            else if (keyInfo.KeyChar == ',' || keyInfo.KeyChar == '.')
            {
                if (countStr.IndexOf(',') != -1) continue;

                // Разрешаем вводить цифры, если только что нажата стрелка
                if (isArrow)
                {
                    isDigitActive = true; // Текст зафиксирован
                    countStr += ',';
                    Console.Write(',');
                }
            }

            // 5. набрана ОБЫЧНАЯ БУКВА (Пробел запрещён)
            else if (!char.IsControl(keyInfo.KeyChar) && keyInfo.KeyChar != ' ')
            {
                // Буквы разрешены, только если: Ещё НЕ нажата стрелка, Ещё НЕ введены цифры, введено МЕНЬШЕ 3 букв                
                if (!isArrow && !isDigitActive && userStr.Length < 3)
                {
                    userStr += keyInfo.KeyChar;                    
                    Console.Write(keyInfo.KeyChar);
                }
            }
        }

        // --- ФИНАЛЬНЫЙ ПРОСТЕЙШИЙ ПАРСИНГ ПОСЛЕ ENTER ---

        // Проверяем, совпадает ли финализированная текстовая строка с реальным продуктом из базы
        string finalProduct = hints.FirstOrDefault(h => h == productStr);
        
        if (finalProduct == null)        
            return ("", 0);        

        // Переводим нашу изолированную числовую строку в int
        double finalCount = 0;
        if (countStr.Length > 0)
        {
            double.TryParse(countStr, out finalCount);
        }

        //округляем до 3-х цифр после запятой
        finalCount = Math.Round(finalCount, 3, MidpointRounding.AwayFromZero);
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
    static int FindBestHints(string str, string[] hints, int currentIndex, bool isForwardDirection)
    {
        str = CleanForSearch(str);

        // 1. Если пользователь ничего не ввёл, просто двигаемся к следующему элементу массива по кругу (в случае превышения индекс = 0)
        if (string.IsNullOrEmpty(str))
        {
            if (isForwardDirection)
            {
                return (currentIndex + 1) % hints.Length;
            }
            else
            {
                // Защита от ухода в минус при листании назад
                return (currentIndex - 1 + hints.Length) % hints.Length;
            }
        }

        // 2. Собираем ИНДЕКСЫ всех слов, которые начинаются на наш запрос
        List<int> matchedIndices = new List<int>();
        for (int i = 0; i < hints.Length; i++)
        {
            string hintClean = CleanForSearch(hints[i]);
            if (hintClean.StartsWith(str)) // Ищем совпадение С НАЧАЛА СЛОВА
            {
                matchedIndices.Add(i);
            }
        }

        // 3. Если нашли подходящие слова (например, для "ст" это будут индексы Стола и Стула)
        if (matchedIndices.Count > 0)
        {
            // РЕЖИМ ВПЕРЁД (Стрелка вниз) — ищем первый индекс БОЛЬШЕ текущего
            if (isForwardDirection)
            {
                foreach (int index in matchedIndices)
                {
                    if (index > currentIndex) return index;
                }
                // Если мы уже стояли на самом последнем подходящем слове (или currentIndex был вообще в другом месте),
                // то сбрасываемся на САМОЕ ПЕРВОЕ слово из нашего отфильтрованного списка (зацикливаем поиск)
                return matchedIndices[0];
            }
            // РЕЖИМ НАЗАД (Стрелка вверх) — ищем первый индекс МЕНЬШЕ текущего (идём с конца списка!)
            else
            {
                for (int i = matchedIndices.Count - 1; i >= 0; i--)
                {
                    if (matchedIndices[i] < currentIndex) return matchedIndices[i];
                }
                return matchedIndices[matchedIndices.Count - 1]; // Сброс на конец списка подходящих
            }           
        }

        // 4. Если совпадений вообще нет, просто листаем весь массив дальше по кругу (в случае превышения индекс = 0)
        if (isForwardDirection)
        {
            return (currentIndex + 1) % hints.Length;
        }
        else
        {
            return (currentIndex - 1 + hints.Length) % hints.Length;
        }
    }


}

