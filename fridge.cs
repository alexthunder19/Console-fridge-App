using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

class Program
{
    //нужны для перерисовки окна, если его уменьшат    
    private static readonly int _windowWidth = 80; //вместо 120
    private static readonly int _windowHeight = 30;

    //чтобы заблокировать изменение размера окна:
    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    //чтобы сразу переключить на Русский:
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);    

    // Системные константы Windows
    private static readonly uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
    private static readonly IntPtr _russianLayout = (IntPtr)0x04190419; // Код русской раскладки  


    static void Main()
    {
        Console.WindowWidth = _windowWidth;
        Console.BufferWidth = _windowWidth;
        Console.WindowHeight = _windowHeight;
        Console.BufferHeight = _windowHeight;        

        // --- Блокировка размера окна и переключение на русский ---
        IntPtr handle = GetConsoleWindow(); // Получаем идентификатор окна нашей консоли
        IntPtr sysMenu = GetSystemMenu(handle, false); // Получаем системное меню этого окна

        if (handle != IntPtr.Zero)
        {
            DeleteMenu(sysMenu, 0xF000, 0x00000000); // 0xF000 — это команда SC_SIZE (изменение размера)
            DeleteMenu(sysMenu, 0xF030, 0x00000000); // 0xF030 — это команда SC_MAXIMIZE (развернуть на весь экран)
            // Отправляем окну запрос на смену языка на русский
            PostMessage(handle, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, _russianLayout);
        }
        // ---------------------------------       

        // Создаём Sorted словарь storage
        SortedDictionary<string, double> storage = new SortedDictionary<string, double>();
        string[] things = new string[] { "10|Хлеба кусок" , "12|Пакет молока", "14|Сыра 100 г",  "14|пиццы кусок",
            "25|Латяо" , "25|Конжак" , "25|Конжак зел",
        "35|Колбаски", "35|Палка сырокопчёной", "35|Сигара",
            "45|Мороженка", "45|Конфета","45|Печенинка розовая",
            "55|Каша овсяная", "55|Каша 5 злаков", "55|Макароны", "55|Лапша б/п", "55|Фасоль", "55|Гречка 100 г",
            "65|Водка 100 г", "65|Пиво" };        


        // ---------------------------------

        // Находим путь к стандартной папке "Документы" текущего пользователя Windows
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // Объединяем путь с именем нашего файла
        string filePath = Path.Combine(docPath, "fridge_storage.txt");

        if (!File.Exists(filePath))
        {
            //заполняем Словарь        
            for (int i = 0; i < things.Length; i++)
            {
                storage.Add(things[i], 1);
            }

            // Сразу сохраняем этот стартовый список в файл, чтобы он там появился
            SaveStorageToFile(filePath, storage);
        }
        else
        {
            // ФАЙЛ ЕСТЬ: Читаем его построчно
            LoadStorageFromFile(filePath, storage);
        }


        // Локальная функция для показа меню и содержимого
        void ShowStorage()
        {
            ConsoleColor GetGroupColor(int groupNumber)
            {
                switch (groupNumber)
                {
                    case 1: return ConsoleColor.DarkYellow;
                    case 2: return ConsoleColor.Green;
                    case 3: return ConsoleColor.Red;
                    case 4: return ConsoleColor.Cyan;
                    case 5: return ConsoleColor.Yellow;
                    case 6: return ConsoleColor.Blue;
                    default: return ConsoleColor.Gray;
                }                                               
            }

            Console.Clear(); // Очищаем экран
            Console.WriteLine("         продукты:          ");

            // Массив строк меню — компактно и удобно          
            string[] menuLines = new string[]
            {
                    "+    Увеличить",
                    "-    Уменьшить",
                    "Ent  Приравнять",
                    "P    добавить позицию",
                    "N    удалить позицию",
                    "C    установить категорию",
                    "O    открыть файл",
                    "Esc  Выйти из программы"
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
                        int groupNum = 0; // По умолчанию - цвет нейтральный
                        string[] keyParts = pairs[i].Key.Split('|');

                        //первый символ до '|' - это номер группы
                        if (keyParts[0] != "" && int.TryParse(keyParts[0].Substring(0, 1), out int parsedValue))
                            groupNum = parsedValue;
                        string productName = keyParts[1];  //после '|' - это имя

                        // Включаем цвет группы для текущей строки продукта
                        Console.ForegroundColor = GetGroupColor(groupNum);

                        // Включаем цвет конкретно для этой группы
                        Console.ForegroundColor = GetGroupColor(groupNum);

                        if (pairs[i].Value % 1 == 0) 
                        {
                            Console.Write($"{productName + ":",-19}{pairs[i].Value,4} шт.   ");                            
                        }
                        else //если значение не целое, то 3 знака
                        {
                            Console.Write($"{productName + ":",-19}{pairs[i].Value,6:F1}     ");                            
                        }

                        Console.Write(new string(' ', 9)); // Дописываем пробелы до начала меню
                        Console.ResetColor(); // Сбрасываем цвет, чтобы текст меню справа не окрасился!
                    }
                    else if (i == 0 && pairs.Length == 0)
                        Console.Write($"{"продуктов нет :(",-39}");
                    else
                        Console.Write($"{"",-39}");

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
                int iMenu = (int)((storage.Count - (menuLines.Length - 2)) * 0.5);   //= (int)((storage.Count - (menuLines.Length - 2)) * 0.7);              

                foreach (KeyValuePair<string, double> pair in storage)
                {
                    int groupNum = 0; // По умолчанию - цвет нейтральный
                    string[] keyParts = pair.Key.Split('|');

                    //первый символ до '|' - это номер группы
                    if (keyParts[0] != "" && int.TryParse(keyParts[0].Substring(0, 1), out int parsedValue))
                        groupNum = parsedValue;
                    string productName = keyParts[1];  //после '|' - это имя

                    // Включаем цвет группы для текущей строки продукта
                    Console.ForegroundColor = GetGroupColor(groupNum);

                    if (pair.Value % 1 == 0)
                    {
                        Console.Write($"{productName + ":",-19}{pair.Value,4} шт.   ");                        
                    }
                    else //если значение не целое, то 3 знака
                    {
                        Console.Write($"{productName + ":",-19}{pair.Value,6}     ");                        
                    }

                    Console.Write(new string(' ', 9)); // Дописываем пробелы до начала меню
                    Console.ResetColor(); // Сразу сбрасываем, чтобы меню вывелось серым!

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



        while (true) /////////////////// пользователь работает /////////////////////////////////////////////////////////////////////////////////////
        {
            ShowStorage();
            // Получаем объект ConsoleKeyInfo
            ConsoleKey key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Add || key == ConsoleKey.OemPlus) //******************увеличить **********************************************
            {
                if (storage.Count == 0)
                {
                    Console.Write("    сначала добавь позицию");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1100);
                    continue;
                }

                //добавляем
                var result = ParseProductAndCount("Увеличим что и сколько? (Стрелка вниз, число): ", storage.Keys.ToArray());

                string str = result.product;
                double count = result.count;                

                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);

                if (count <= 0 || !storage.ContainsKey(str))
                {
                    Console.Write("    добавил 0... " );
                }
                else if (storage.ContainsKey(str))
                {
                    storage[str] += count; //приплюсуем к элементу Словаря
                    Console.Write($"    добавлены {count} \"{str.Split('|')[1]}\" " );
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                System.Threading.Thread.Sleep(1100);                
            }            


            else if (key == ConsoleKey.Subtract || key == ConsoleKey.OemMinus) //******************вычесть ******************************************
            {                
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
                if (count <= 0 || !storage.ContainsKey(str) || storage[str] == 0)
                    Console.Write("    вычли 0...");
                else if (storage.ContainsKey(str)) //иначе вычитаем
                {
                    if(count > storage[str])
                    {
                        Console.Write($"    съедены {storage[str]} \"{str.Split('|')[1]}\"");
                        storage[str] = 0;                        
                    }                        
                    else
                    {
                        Console.Write($"    съедены {count} \"{str.Split('|')[1]}\"");
                        storage[str] -= count; //вычтем у элемента Словаря 
                    }
                }
                Console.SetCursorPosition(0, Console.CursorTop);
                System.Threading.Thread.Sleep(1100);                
            }


            else if (key == ConsoleKey.Enter) //***************** приравнять ***************************************
            {
                if (storage.Count == 0)
                {
                    Console.Write("    сначала добавь позицию");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1100);
                    continue;
                }

                //добавляем
                var result = ParseProductAndCount("продукт и количество? (Стрелка вниз, число): ", storage.Keys.ToArray());

                string str = result.product;
                double count = result.count;                

                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);

                if (count == -1 || !storage.ContainsKey(str))
                {
                    Console.Write("    добавил 0... ");
                }
                else if (storage.ContainsKey(str))
                {
                    storage[str] = count; //занесём в элемент Словаря
                    Console.Write($"    теперь \"{str.Split('|')[1]}\" = {count}");
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                System.Threading.Thread.Sleep(1100);
            }


            if (key == ConsoleKey.P) //******************добавить позицию**********************************************
            {
                //добавляем
                string str = InputString("введи новую позицию: ", 18);
                if (str == "") //если ничё не ввёл - выходим
                {
                    //стираем
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    //выходим
                    Console.Write("    не добавлено...");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1300);
                    continue;

                }
                // пробежимся по всему хол-ку, может там уже есть такое название (пусть даже в другом регистре)
                bool isContains = false;
                foreach (KeyValuePair<string, double> pair in storage)
                {
                    string[] parts = pair.Key.Split('|'); //очистим от начальной '|'                    

                    if (parts[1].ToLower().Trim() == str.ToLower().Trim())
                    {
                        isContains = true;
                        break;
                    }
                }

                if (isContains)
                {
                    //стираем
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    //выходим
                    Console.Write($"    \"{str.ToLower()}\" уже есть на складе!");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1200);
                    continue;
                }

                double count = InputNumberDouble("введи количество продукта: ");
                if (count == -1) // если пользователь нажал Esc
                    continue;
                int categ = SetCategory("номер категории(1-6): ");
                if (categ == -1) // если пользователь нажал Esc
                    continue;

                str = $"{categ}|{str}"; // Собираем обратно ключ "0|Пакет молока"
                storage[str] = count;  //добавим в Словарь                
            }


            else if (key == ConsoleKey.N) //******************удалить позицию******************************************
            {
                if (storage.Count == 0)
                {
                    Console.Write("    нечего удалять, лучше добавь");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1100);
                    continue;
                }

                string str = InputStringWithHints("что удалить? (буквы + Стрелка): ", storage.Keys.ToArray());
                
                //стираем
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);

                // 1. Проверяем на пустую строку (отмена)
                if (string.IsNullOrEmpty(str))
                {
                    Console.Write("    не удалено...");
                }
                // 2. Если не пустая, проверяем, что товара нет в базе
                else if (!storage.ContainsKey(str))
                {
                    Console.Write($"    \"{str}\" нет в списке..");
                }
                // 3. Во всех остальных случаях (строка заполнена и товар точно есть) — удаляем!
                else
                {
                    storage.Remove(str);
                    Console.Write($"    \"{str.Split('|')[1]}\" удалены...");
                }                

                Console.SetCursorPosition(0, Console.CursorTop);
                System.Threading.Thread.Sleep(1000);                
            }

            if (key == ConsoleKey.C) //******************установить категорию **********************************************
            {
                //добавляем
                string str = InputStringWithHints("выбери продукт (буквы + Стрелка): ", storage.Keys.ToArray());
                if (str == "") //если ничё не ввёл - выходим
                {
                    //стираем
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    //выходим
                    Console.Write("    ну ладно...");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1300);
                    continue;

                }
                
                if (str.Contains('|')) //значит пользователь выбирал стрелочкой
                {                    
                    int categ = SetCategory("номер категории(1-6): ");
                    if (categ == -1) // если пользователь нажал Esc                    
                        continue;
                    double value = storage[str];
                    storage.Remove(str);
                    storage[$"{categ}|{str.Split('|')[1]}"] = value;                    
                }
                else  //значит пользователь вбивал руками(
                {
                    //стираем
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    //выходим
                    Console.Write($"    \"{str}\" нету!");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    System.Threading.Thread.Sleep(1200);
                }
            }



            else if (key == ConsoleKey.O) //***************** открыть файл в Блокноте *****************
            {
                Console.Write("    открываю Блокнот...");

                try
                {
                    // 1. Сначала сохраняем текущее состояние из памяти в файл, чтобы пользователь видел актуальный список
                    SaveStorageToFile(filePath, storage);

                    // 2. Запускаем системный Блокнот Windows и передаем ему путь к нашему файлу
                    var process = System.Diagnostics.Process.Start("notepad.exe", filePath);

                    // 3. Заставляем нашу консоль замереть и подождать, пока пользователь не закроет Блокнот!
                    process.WaitForExit();

                    // 4. Как только Блокнот закрыт — полностью очищаем старый словарь в памяти...
                    storage.Clear();

                    // 5. ...и вызываем ваш метод чтения файла заново! Программа подтянет все ручные изменения                    
                    LoadStorageFromFile(filePath, storage);

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("    список обновлен!    ");                    
                }
                catch
                {
                    Console.Write("    ошибка открытия файла...");
                }

                System.Threading.Thread.Sleep(1000);
            }



            else if (key == ConsoleKey.Escape) //******************выход**************************************************
            {
                // ПЕРЕД ВЫХОДОМ сохраняем все изменения в файл в Документы!
                SaveStorageToFile(filePath, storage);

                Console.Write("      до скорого! ");
                System.Threading.Thread.Sleep(500);
                break;
            }
        }
    }

    static void LoadStorageFromFile(string filePath, SortedDictionary<string, double> storage)
    {
        // ФАЙЛ ЕСТЬ: Читаем его построчно
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Каждая строка в файле имеет формат: Группа|Название|Количество
            // Например: 1|Пакет молока|14
            string[] parts = line.Split('|');
            if (parts.Length >= 3) //если '|' две
            {
                string key = $"{parts[0]}|{parts[1]}"; // Собираем обратно ключ "1|Пакет молока"


                // Парсим количество строго по правилам русской локали (с запятой)
                double.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out double count);

                storage[key] = count;
            }
            else if (parts.Length == 2) //если '|' только одна
            {
                if (line.IndexOf('|') < line.Length / 2) //и если '|' в начале строчки
                {
                    string key = $"{parts[0]}|{parts[1]}"; // Собираем обратно ключ "1|Пакет молока"
                    storage[key] = 0;
                }
                else    //и если '|' в конце строчки
                {
                    string key = $"8|{parts[0]}"; // Собираем обратно ключ "8|Пакет молока"
                    double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out double count);
                    storage[key] = count;
                }
            }
            else  //если '|' вообще нет
            {
                string key = $"8|{line}"; // Собираем обратно ключ "0|Пакет молока"
                storage[key] = 0;
            }
        }
    }
    static double InputNumberDouble(string inputMessage, double max = 400)
    {
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
        Console.Write(inputMessage);
        string str = "";

        // ЦИКЛ: Сбор букв до нажатия Enter
        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            // нажат Esc — выходим
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                return -1;
            }

            // нажат ENTER — завершаем ввод
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                break;
            }

            // нажат BACKSPACE — удаляем символ
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (str.Length > 0)
                {
                    // Стираем один символ                    
                    str = str.Substring(0, str.Length - 1);

                    // Перерисовываем экран через ClearUserErrors, чтобы убрать хвост подсказки
                    ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                    Console.Write(str);
                }
            }

            // нажата ЦИФРА (0-9)
            else if (char.IsDigit(keyInfo.KeyChar))
            {
                if (str.Length < 7)
                {
                    str += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
            }

            // нажата точка или запятая
            else if (keyInfo.KeyChar == ',' || keyInfo.KeyChar == '.')
            {
                if (str.IndexOf(',') != -1) continue;
                
                if (str.Length < 7)
                {                    
                    str += ',';
                    Console.Write(',');
                }
            }
        }

        // --- парсинг после ENTER ---

        // Переводим нашу числовую строку в double
        double num = 1;
        if (str.Length > 0)
        {
            // Заставляем систему парсить строку строго по правилам русской локали (с запятой)
            double.TryParse(str, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out num);
        }
        if (num > max)
            num = 1;

        //округляем до 3-х цифр после запятой
        num = Math.Round(num, 3, MidpointRounding.AwayFromZero);
        return num;
    }
    static string InputString(string inputMessage, int max)
    {        
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
        Console.Write(inputMessage);
        string str = "";

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            // нажат Esc — выходим
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                str = "";
                break;
            }

            // нажат ENTER — завершаем ввод
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                // Если одни пробелы       
                if (string.IsNullOrWhiteSpace(str))
                {
                    str = "";
                    break;
                }

                //Чистим строку
                str = str.Trim();
                //разбиваем строку по пробелам, игнорируя пустые элементы
                string[] words = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //сшиваем слова обратно, разделяя их ОДНИМ пробелом
                str = string.Join(" ", words);

                if (!str.Any(char.IsLetter))
                {
                    ClearUserErrors(startingCursorTop, inputMessage, $"  -а буквы где?!-");
                    str = "";
                    continue; // Возврат в начало цикла для нового ввода
                }
                
                break; 
            }                

            // нажат BACKSPACE — удаляем символ
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (str.Length > 0)
                {
                    // Стираем один символ                                               
                    str = str.Substring(0, str.Length - 1);

                    // Перерисовываем экран через ClearUserErrors, чтобы убрать хвост подсказки
                    ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                    Console.Write(str);
                }
            }

            // нажата обычная буква или знак — печатаем
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                if (str.Length < max)
                {
                    str += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
            }
        }
        
        Console.WriteLine();
        return str;
    }
    static string InputStringWithHints(string inputMessage, string[] hints)
    {
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
        Console.Write(inputMessage);

        // --- ОЧИСТКА ПОДСКАЗОК ДЛЯ ЭКРАНА ---
        string[] cleanHints = new string[hints.Length];
        for (int i = 0; i < hints.Length; i++)
        {
            cleanHints[i] = hints[i].Split('|')[1];
        }

        string str = "";
        string userStr = ""; // хранит ТОЛЬКО то, что вбито руками
        int hintsIndex = -1; // Индекс подсказки
        bool isArrow = false; // Флаг: нажата ли стрелка?

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            // нажат Esc — выходим
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine();
                return "";                
            }

            // нажат ENTER — завершаем ввод
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                // Если не нажата перед этим стрелка       
                if (isArrow)
                {
                    // Извлекаем ОРИГИНАЛЬНЫЙ КЛЮЧ с палочкой (например, "1|Пакет молока") по сохранённому индексу
                    str = hints[hintsIndex];
                }

                Console.WriteLine();
                return str;                
            }

            // нажата СТРЕЛКА ВНИЗ — листаем подсказки
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (cleanHints == null || cleanHints.Length == 0) continue;                

                hintsIndex = FindBestHints(userStr, cleanHints, hintsIndex, true);

                // Стираем старый ввод с экрана
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                // Подставляем значение из массива
                str = cleanHints[hintsIndex];

                isArrow = true;     // стрелка нажата  

                // Печатаем новую подсказку
                Console.Write(str);
            }

            // нажата СТРЕЛКА ВВЕРХ — листаем подсказки назад
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (cleanHints == null || cleanHints.Length == 0) continue;               

                hintsIndex = FindBestHints(userStr, cleanHints, hintsIndex, false);

                // Стираем старый ввод с экрана
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                // Подставляем значение из массива
                str = cleanHints[hintsIndex];

                isArrow = true;     // стрелка нажата  

                // Печатаем новую подсказку
                Console.Write(str);
            }

            // нажат BACKSPACE — удаляем символ
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                isArrow = false;     // стрелка сброшена  

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
                if (str.Length < 4)
                {
                    str += keyInfo.KeyChar;
                    userStr = str; // Запоминаем, что это ввёл именно пользователь
                    Console.Write(keyInfo.KeyChar);
                    isArrow = false;     // стрелка сброшена 
                }                
            }
        }       
    }

    static (string product, double count) ParseProductAndCount(string inputMessage, string[] hints)
    {
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
        Console.Write(inputMessage);

        // --- ОЧИСТКА ПОДСКАЗОК ДЛЯ ЭКРАНА ---
        string[] cleanHints = new string[hints.Length];
        for (int i = 0; i < hints.Length; i++)
        {
            cleanHints[i] = hints[i].Split('|')[1];
        }

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

            // нажат Esc — выходим
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine();
                return ("", 0);
            }

            // 1. нажат ENTER — завершаем ввод
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (isArrow)
                {
                    Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                    break;
                }                
            }

            // 2. нажата СТРЕЛКА ВНИЗ — листаем подсказки
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                // Стрелка работает, только если мы ещё НЕ начали вводить цифры
                if (isDigitActive || cleanHints == null || cleanHints.Length == 0) continue;

                hintsIndex = FindBestHints(userStr, cleanHints, hintsIndex, true);

                // Стираем старый ввод с экрана
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                productStr = cleanHints[hintsIndex]; // Подставляем значение из массива
                isArrow = true;     // стрелка нажата                
                Console.Write(productStr); // Печатаем новую подсказку
            }

            // 3. нажата СТРЕЛКА ВВЕРХ — листаем подсказки в обр. сторону
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                // Стрелка работает, только если мы ещё НЕ начали вводить цифры
                if (isDigitActive || cleanHints == null || cleanHints.Length == 0) continue;

                hintsIndex = FindBestHints(userStr, cleanHints, hintsIndex, false);

                // Стираем старый ввод с экрана
                ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                productStr = cleanHints[hintsIndex]; // Подставляем значение из массива
                isArrow = true;     // стрелка нажата                
                Console.Write(productStr); // Печатаем новую подсказку
            }

            // 4. нажат BACKSPACE — удаляем символ
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                // Стираем цифру, если уже идёт числовая строка
                if (isDigitActive)
                {
                    if (countStr.Length > 0)
                    {
                        countStr = countStr.Substring(0, countStr.Length - 1);
                        ClearUserErrors(startingCursorTop, inputMessage, "", 0);

                        // Добавляем пробел между продуктом и цифрами при перерисовке!
                        Console.Write(productStr + " " + countStr);
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
            
            // 5. нажата ЦИФРА (0-9)
            else if (char.IsDigit(keyInfo.KeyChar))
            {
                // Разрешаем вводить цифры, если только что нажата стрелка
                if (isArrow && countStr.Length < 7)
                {
                    // ЕСЛИ ЭТО САМАЯ ПЕРВАЯ ЦИФРА (или разделитель):
                    if (!isDigitActive)
                    {
                        isDigitActive = true; // Текст зафиксирован
                        Console.Write(" ");   // Рисуем ТОЛЬКО НА ЭКРАНЕ чисто визуальный пробел!
                    }                    
                    countStr += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
            }

            // 6. нажата точка или запятая
            else if (keyInfo.KeyChar == ',' || keyInfo.KeyChar == '.')
            {
                if (countStr.IndexOf(',') != -1) continue;

                // Разрешаем вводить цифры, если только что нажата стрелка
                if (isArrow && countStr.Length < 7)
                {
                    // ЕСЛИ ЭТО САМЫЙ ПЕРВЫЙ СИМВОЛ ЧИСЛА (вместо цифры нажали точку/запятую):
                    if (!isDigitActive)
                    {
                        isDigitActive = true; // Текст зафиксирован
                        Console.Write(" ");   // Рисуем ТОЛЬКО НА ЭКРАНЕ чисто визуальный пробел!
                    }                    
                    countStr += ',';
                    Console.Write(',');
                }
            }

            // 7. набрана ОБЫЧНАЯ БУКВА (Пробел запрещён)
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

        // Если пользователь вообще не нажал стрелку и ничего не выбрал - выходим
        if (hintsIndex == -1)
        {
            return ("", 0);
        }

        // Извлекаем ОРИГИНАЛЬНЫЙ КЛЮЧ с палочкой (например, "1|Пакет молока") по сохранённому индексу
        string finalProduct = hints[hintsIndex];

        // Переводим нашу изолированную числовую строку в double
        double finalCount = -1;
        if (countStr.Length > 0)
        {
            // Заставляем систему парсить строку строго по правилам русской локали (с запятой)
            double.TryParse(countStr, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out finalCount);
        }
        if (finalCount > 400)
            finalCount = -1;

        //округляем до 3-х цифр после запятой
        finalCount = Math.Round(finalCount, 2, MidpointRounding.AwayFromZero);
        return (finalProduct, finalCount);
    }
    static int SetCategory(string inputMessage)
    {
        int startingCursorTop = Console.CursorTop; // Запоминаем, где начинается строка ввода
        Console.Write(inputMessage);
        string str = "";

        // ЦИКЛ: Сбор букв до нажатия Enter
        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            // нажат Esc — выходим
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                return -1;
            }

            // нажат ENTER — завершаем ввод
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine(); // Переводим каретку на новую строку, как обычный ReadLine
                break;
            }

            // нажат BACKSPACE — удаляем символ
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (str.Length > 0)
                {
                    // Стираем один символ                    
                    str = str.Substring(0, str.Length - 1);

                    // Перерисовываем экран через ClearUserErrors, чтобы убрать хвост подсказки
                    ClearUserErrors(startingCursorTop, inputMessage, "", 0);
                    Console.Write(str);
                }
            }

            // нажата ЦИФРА (0-9)
            else if (char.IsDigit(keyInfo.KeyChar))
            {
                if (str.Length < 3)
                {
                    str += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
            }            
        }

        // --- парсинг после ENTER ---
        if (str.Length == 1)
            str += "5";
        int categ = 85;
        if (int.TryParse(str, out int num))
            categ = num;

        return categ;
    }

    // функция для очистки ВСЕХ неверных данных введённых пользователем и вывода временного сообщения об ошибке
    static void ClearUserErrors(int startingCursorTop, string inputMessage, string errorMessage = "   -Неверно! Читай внимательнее!-", int pauseTime = 1900)
    {
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
                // Если это самый первый клик вверх (индекс -1), 
                // принудительно прыгаем на самый последний элемент массива!
                if (currentIndex == -1)
                {
                    return hints.Length - 1;
                }

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
    static void SaveStorageToFile(string filePath, SortedDictionary<string, double> storage)
    {
        try
        {
            List<string> linesToWrite = new List<string>();

            foreach (KeyValuePair<string, double> pair in storage)
            {
                // Наш pair.Key уже содержит в себе "1|Пакет молока"
                // Нам осталось просто пристыковать через палочку количество (Value)
                // Принудительно переводим double в строку по правилам русской локали (с запятой)
                string countStr = pair.Value.ToString(System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));

                string fileLine = $"{pair.Key}|{countStr}"; // Получится "1|Пакет молока|1"
                linesToWrite.Add(fileLine);
            }

            // Записываем все строки в файл (если файл существовал, он полностью перезапишется свежими данными)
            File.WriteAllLines(filePath, linesToWrite);
        }
        catch
        {
            // Защита: если файл заблокирован другой программой, не падаем
        }
    }


}

