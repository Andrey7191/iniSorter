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
            string helpText = "Введите: \n'sortfile путь-к-файлу' - для сортировки 1-го файла, \n'sortdir путь-к-директории' - для сортировки всех ini-файлов в директории, \n'help' - для вывода этой подсказки, \n'exit' - для выхода.";
            Console.WriteLine(helpText);
            while (true)
            {
                Console.Write("$ ");
                string command = Console.ReadLine();
                string commandType = GetСommandType(command);

                string commandArgument;
                string inputFilePath = "";
                string inputDirectoryPath = "";
                
                if (commandType=="exit")
                    break;

                switch (commandType)
                {
                    case "help":
                        Console.WriteLine(helpText);
                        break;
                    case "sortfile":
                        Regex regIniFile = new Regex(@"^.+(\.ini){1}$");
                        commandArgument = command.Trim().Split("sortfile")[1].Trim();
                        if (commandArgument == "")
                            Console.WriteLine("Название файла не может быть пустым! Попробуйте снова!");
                        else if (!regIniFile.IsMatch(commandArgument))
                            Console.WriteLine("Файл должен заканчиваться на '.ini'. Попробуйте снова!");
                        else
                            inputFilePath = commandArgument;
                        break;
                    case "sortdir":
                        commandArgument = command.Trim().Split("sortdir")[1].Trim();
                        if (commandArgument == "")
                            Console.WriteLine("Путь не может быть пустым! Попробуйте снова!");
                        else
                            inputDirectoryPath = commandArgument;
                        break;
                    default:
                        Console.WriteLine("Введена недопустимая команда! Введите'help' для вывода доступных команд.");
                        break;
                }

                if (inputFilePath != "")
                {
                    string outputFilePath = inputFilePath.Insert(inputFilePath.Length - 4, "_Sorted");
                    string error = IniLib.SortIniFile(inputFilePath, outputFilePath);
                    HandleSortIniFileErrors(error);
                }
                else if (inputDirectoryPath != "")
                {
                    (string globalErrorType, int doneCount, int errorsCount) result = IniLib.SortIniFilesInDir(inputDirectoryPath);
                    HandleSortIniFilesInDirErrors(result);
                }
            }
        }


        //Метод, отвечающий за определение типа команды пользователя
        //Возвращает: "exit", "help", "sortfile", "sortdir" или "UnknownType"
        private static string GetСommandType(string command)
        {
            Regex regExit = new Regex(@"^\s*(exit){1}\s*$");
            Regex regHelp = new Regex(@"^\s*(help){1}\s*$");
            Regex regSortFile = new Regex(@"^\s*(sortfile){1}.*$");
            Regex regSortDir = new Regex(@"^\s*(sortdir){1}.*$");

            string commandType;

            if (regExit.IsMatch(command))
                commandType = "exit";
            else if (regHelp.IsMatch(command))
                commandType = "help";
            else if (regSortFile.IsMatch(command))
                commandType = "sortfile";
            else if (regSortDir.IsMatch(command))
                commandType = "sortdir";
            else
                commandType = "UnknownType";

            return commandType;
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
