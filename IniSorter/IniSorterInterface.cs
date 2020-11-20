using System;
using System.Text.RegularExpressions;

//IniSorter - программа для сортировки содержания ini-файлов.
//Перевозкин А. А., 2020 г.

namespace IniSorter
{
    //Интерфейс программы
    class IniSorterInterface
    {
        //Главный метод интерфейса программы. Отвечает за обработку команд пользователя, проверку правильности ввода.
        //Доступно 4 команды: сортировка одного ini-файла, сортировка всех ini-файлов в директории, вывод подсказки, выход из программы.
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в программу IniSorter! \nПрограмма предназначена для сортировки содержимого Ваших ini-файлов. \nВсе отсортированный файла сохраняются в ту же директорию с добавление '_Sorted' к названию.");
            string help_text = "Введите: \n'sortfile путь-к-файлу' - для сортировки 1-го файла, \n'sortdir путь-к-директории' - для сортировки всех ini-файлов в директории, \n'help' - для вывода этой подсказки, \n'exit' - для выхода.";
            Console.WriteLine(help_text);
            while (true)
            {
                Console.Write("$ ");
                string command = Console.ReadLine();

                string[] splited_direction;
                string input_file_path = "";
                string input_directory = "";
                Regex reg_ini_file = new Regex(@"^.+(\.ini){1}$");

                string command_type = GetСommandType(command);

                if (command_type=="exit")
                {
                    break;
                }
                else if (command_type == "help")
                {
                    Console.WriteLine(help_text);
                }
                else if (command_type == "sortfile")
                {
                    splited_direction = command.Trim().Split("sortfile");
                    splited_direction[1] = splited_direction[1].Trim();
                    if (splited_direction[1]=="")
                        Console.WriteLine("Название файла не может быть пустым! Попробуйте снова!");
                    else if (!reg_ini_file.IsMatch(splited_direction[1]))
                        Console.WriteLine("Файл должен заканчиваться на '.ini'. Попробуйте снова!");
                    else
                        input_file_path = splited_direction[1];
                }
                else if (command_type == "sortdir")
                {
                    splited_direction = command.Trim().Split("sortdir");
                    splited_direction[1] = splited_direction[1].Trim();
                    if (splited_direction[1] == "")
                        Console.WriteLine("Путь не может быть пустым! Попробуйте снова!");
                    else
                        input_directory = splited_direction[1];
                }
                else
                {
                    Console.WriteLine("Введена недопустимая команда! Введите'help' для вывода доступных команд.");
                }

                if (input_file_path != "")
                {
                    string output_file_path = input_file_path.Insert(input_file_path.Length - 4, "_Sorted");
                    string error = IniLib.SortIniFile(input_file_path, output_file_path);
                    HandleSortIniFileErrors(error);
                }
                else if (input_directory != "")
                {
                    (string globalErrorType, int doneCount, int errorsCount) result = IniLib.SortIniFilesInDir(input_directory);
                    HandleSortIniFilesInDirErrors(result);
                }
            }


        }

        //Метод, отвечающий за определение типа команды пользователя
        //Возвращает: "exit", "help", "sortfile", "sortdir" или "UnknownType"
        private static string GetСommandType(string command)
        {
            Regex reg_exit = new Regex(@"^\s*(exit){1}\s*$");
            Regex reg_help = new Regex(@"^\s*(help){1}\s*$");
            Regex reg_sort_file = new Regex(@"^\s*(sortfile){1}.*$");
            Regex reg_sort_dir = new Regex(@"^\s*(sortdir){1}.*$");

            string command_type;

            if (reg_exit.IsMatch(command))
                command_type = "exit";
            else if (reg_help.IsMatch(command))
                command_type = "help";
            else if (reg_sort_file.IsMatch(command))
                command_type = "sortfile";
            else if (reg_sort_dir.IsMatch(command))
                command_type = "sortdir";
            else
                command_type = "UnknownType";

            return command_type;
        }


        //Метод, отвечающий за обработку результатов функции сортировки одного файла. Результаты работы выводятся на экран.
        private static void HandleSortIniFileErrors(string error)
        {
            switch (error)
            {
                case "":
                    Console.WriteLine("Файл отсортирован и сохранён. Можете отсортировать ещё один файл!");
                    break;
                case "FileNotFound":
                    Console.WriteLine("Файл не найден. Попробуйте снова!");
                    break;
                case "IncorrectSyntax":
                    Console.WriteLine("Файл не соответствует синтаксису ini, не может быть отсортирован. Попробуйте снова!");
                    break;
                case "UnknownReadError":
                    Console.WriteLine("Не удалось прочитать информацию из файла. Попробуйте снова!");
                    break;
                case "EmptyPathName":
                    Console.WriteLine("Был указан пустой путь! Попробуйте снова!");
                    break;
                default:
                    Console.WriteLine("Возникла непредвиденная ошибка. Попробуйте снова!");
                    break;
            }
        }


        //Метод, отвечающий за обработку результатов функции сортировки всех файлов в директории. Результаты работы выводятся на экран.
        private static void HandleSortIniFilesInDirErrors((string globalErrorType, int doneCount, int errorsCount) result)
        {
            switch (result.globalErrorType)
            {
                case "":
                    Console.WriteLine("Отсортировано файлов: " + result.doneCount + ", ошибок: " + result.errorsCount);
                    break;
                case "FilesNotFound":
                    Console.WriteLine("Ini-файлы в указанной директории не найдены. Попробуйте снова!");
                    break;
                case "DirectoryNotFound":
                    Console.WriteLine("Директория не найдена. Попробуйте снова!");
                    break; 
                case "DirectoryError":
                    Console.WriteLine("Не удалось прочитать файлы из директории. Попробуйте снова!");
                    break;
                default:
                    Console.WriteLine("Возникла непредвиденная ошибка. Попробуйте снова!");
                    break;
            }
        }

    }
}
