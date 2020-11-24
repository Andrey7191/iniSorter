using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IniSorter
{
    //Функциональный блок программы
    public static class IniLib
    {
        //Главный метод, отвечающий за сортировку всех ini-файлов в директории
        //Принимает в качестве параметра путь к директории, в которой необходимо провести сортировку
        //Возвращает код глобальной ошибки (например, если не удалось вообще найти директорию), количество отсортированных файлов и количество файлов с ошибками 
        public static (string globalErrorType, int doneCount, int errorsCount) SortIniFilesInDir(string inputPath)
        {
            string[] iniFiles;
            try
            {
                iniFiles = Directory.GetFiles(inputPath);
            }
            catch (DirectoryNotFoundException)
            {
                return ("DirectoryNotFound", 0, 0);
            }
            catch (Exception)
            {
                return ("DirectoryError", 0, 0);
            }

            Regex regIniFile = new Regex(@"^.+(\.ini){1}$");
            Regex regIniFileSorted = new Regex(@"^.+(_Sorted\.ini){1}$");

            int iniFilesDone = 0;
            int iniFilesErrors = 0;

            foreach (string iniFileName in iniFiles)
                if (regIniFile.IsMatch(iniFileName) && !regIniFileSorted.IsMatch(iniFileName))
                {
                    string error = SortIniFile(iniFileName, iniFileName.Insert(iniFileName.Length - 4, "_Sorted"));
                    if (error == "")
                        iniFilesDone++;
                    else
                        iniFilesErrors++;
                }
                
            if (iniFilesDone == 0 && iniFilesErrors== 0)
            {
                return ("FilesNotFound", 0, 0);
            }

            return ("", iniFilesDone, iniFilesErrors);
        }


        //Главный метод, отвечающий за сортировку одного ini-файла
        //Принимает на вход путь (включая название) к сортируемому файлу и путь (включая название), по которому нужно сохранить файл
        //Возвращает код ошибки или пустую строку, если сортировка прошла успешно
        public static string SortIniFile(string inputFilePath, string outputFilePath)
        {
            //Чтение информации из файла
            string[] fileContent;
            try
            {
                fileContent = File.ReadAllLines(inputFilePath);
            }
            catch (FileNotFoundException)
            {
                return "FileNotFound";
            }
            catch (ArgumentException)
            {
                return "EmptyPathName";
            }
            catch (Exception)
            {
                return "UnknownReadError";
            }

            //Перенос информации, полученной из файла, в сортированный словарь формата "название-секции : содержание-секции(параметр : значение)"
            SortedDictionary<string, SortedDictionary<string, string>> iniContent = new SortedDictionary<string, SortedDictionary<string, string>>();
            try
            {
                iniContent = SeparateIniFileContent(fileContent);
            }
            catch (Exception)
            {
                return "IncorrectSyntax";
            }
            if (iniContent.Count == 0)
                return "IncorrectSyntax";

            //Перенос информации из сортированных словарей в выходной файл
            string iniFileText = ConvertIniContentToString(iniContent);
            try
            {
                File.WriteAllText(outputFilePath, iniFileText);
            }
            catch (FileNotFoundException)
            {
                return "FileNotFound";
            }

            return "";
        }


        //Метод, разделяющий информацию, считанную из ini-файла, и представляющий её в виде сортированных словарей
        //Возвращает словарь формата "название-секции : содержание-секции(параметр : значение)"
        private static SortedDictionary<string, SortedDictionary<string, string>> SeparateIniFileContent(string[] fileContent)
        {
            SortedDictionary<string, SortedDictionary<string, string>> iniContent = new SortedDictionary<string, SortedDictionary<string, string>>();
            string sectionName = "";

            for (int i = 0; i < fileContent.Length; i++)
            {
                if (fileContent[i].IndexOf(";") >= 0)
                    fileContent[i] = fileContent[i].Split(";")[0];
                if (fileContent[i].IndexOf("#") >= 0)
                    fileContent[i] = fileContent[i].Split("#")[0];

                string lineType = GetIniLineType(fileContent[i]);
                switch (lineType)
                {
                    case "EmptyLine":
                        continue;
                    case "SectionName":
                        sectionName = fileContent[i].Trim()[1..^1].Trim();
                        if (!iniContent.ContainsKey(sectionName))
                            iniContent.Add(sectionName, new SortedDictionary<string, string>());
                        break;
                    case "SectionInner":
                        string[] splitedLine = fileContent[i].Split("=");
                        string parameter = splitedLine[0].Trim();
                        string value = splitedLine[1].Trim();
                        if (sectionName != "")
                            if (!iniContent.ContainsKey(parameter))
                                iniContent[sectionName].Add(parameter, value);
                            else
                                iniContent[sectionName][parameter] = value;
                        else
                            return iniContent;
                        break;
                    default:
                        iniContent.Clear();
                        return iniContent;
                }
            }

            return iniContent;
        }


        //Метод, распознающий в переданной строке тот или иной элемент синтаксиса ini-файла (заголовок секции, содержание секции, пустая строка)
        private static string GetIniLineType(string iniLine)
        {
            Regex regSectionName = new Regex(@"^\s*[\[]{1}\s*[a-zA-Z0-9\._\-\\\/%:'""'\*<>]+[^=\[\]]*[\]]{1}\s*$");
            Regex regSectionInner = new Regex(@"^\s*[a-zA-Z0-9\._\-\\\/%:'""'\*<>]+[^=\[\]]*={1}[^=\[\]]*$");
            Regex regEmpty = new Regex(@"^\s*$");

            string iniLineType;

            if (regSectionName.IsMatch(iniLine))
                iniLineType = "SectionName";
            else if (regSectionInner.IsMatch(iniLine))
                iniLineType = "SectionInner";
            else if (regEmpty.IsMatch(iniLine))
                iniLineType = "EmptyLine";
            else
                iniLineType = "UnknownType";

            return iniLineType;
        }


        //Метод, ответственный за "упаковку" содержания ini-файла в строку для последующей записи в ini-файл
        private static string ConvertIniContentToString(SortedDictionary<string, SortedDictionary<string, string>> iniContent)
        {
            string fileText = "";
            foreach (KeyValuePair<string, SortedDictionary<string, string>> section in iniContent)
            {
                fileText = fileText + "\n[" + section.Key + "]\n";
                foreach (KeyValuePair<string, string> sectionInner in iniContent[section.Key])
                    fileText = fileText + sectionInner.Key + "=" + sectionInner.Value + "\n";
            }
            return fileText;
        }

    }
}
