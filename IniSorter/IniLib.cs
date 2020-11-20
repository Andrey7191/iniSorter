using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IniSorter
{
    //Функциональный блок программы
    static class IniLib
    {
        //Главный метод, отвечающий за сортировку всех ini-файлов в директории
        //Принимает в качестве параметра путь к директории, в которой необходимо провести сортировку
        //Возвращает код глобальной ошибки (например, если не удалось вообще найти директорию), количество отсортированных файлов и количество файлов с ошибками 
        public static (string globalErrorType, int doneCount, int errorsCount) SortIniFilesInDir(string input_path)
        {
            string[] ini_files;
            try
            {
                ini_files = Directory.GetFiles(input_path);
            }
            catch (DirectoryNotFoundException)
            {
                return ("DirectoryNotFound", 0, 0);
            }
            catch (Exception)
            {
                return ("DirectoryError", 0, 0);
            }

            Regex reg_ini_file = new Regex(@"^.+(\.ini){1}$");
            Regex reg_ini_file_sorted = new Regex(@"^.+(_Sorted\.ini){1}$");

            int ini_files_done = 0;
            int ini_files_errors = 0;
            for (int i = 0; i < ini_files.Length; i++)
                if (reg_ini_file.IsMatch(ini_files[i]) && !reg_ini_file_sorted.IsMatch(ini_files[i]))
                {
                    string error = SortIniFile(ini_files[i], ini_files[i].Insert(ini_files[i].Length - 4, "_Sorted"));
                    if (error == "")
                        ini_files_done++;
                    else
                        ini_files_errors++;
                }

            if (ini_files_done == 0 && ini_files_errors== 0)
            {
                return ("FilesNotFound", 0, 0);
            }

            return ("", ini_files_done, ini_files_errors);
        }


        //Главный метод, отвечающий за сортировку одного ini-файла
        //Принимает на вход путь (включая название) к сортируемому файлу и путь (включая название), по которому нужно сохранить файл
        //Возвращает код ошибки или пустую строку, если сортировка прошла успешно
        public static string SortIniFile(string input_file_path, string output_file_path)
        {
            string[] file_content;
            try
            {
                file_content = File.ReadAllLines(input_file_path);
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

            List<IniSection> ini_content = SeparateIniFileContent(file_content);
            if (ini_content.Count == 0)
                return "IncorrectSyntax";

            ini_content = SortIniSections(ini_content);

            string ini_file_text = ConvertIniSectionsToString(ini_content);
            try
            {
                File.WriteAllText(output_file_path, ini_file_text);
            }
            catch (FileNotFoundException)
            {
                return "FileNotFound";
            }

            return "";
        }


        //Два вспомогательных класса для хранения извлечённой из ini-файла информации с разбиением по семантике
        private class IniSection
        {
            public string section_name;
            public List<IniSectionInner> section_inners = new List<IniSectionInner>();
        }


        private class IniSectionInner
        {
            public string parameter = "";
            public string value = "";
        }



        //Метод, разделяющий информацию, считанную из ini-файла, и представляющий её с помощью вспомогательных классов
        private static List<IniSection> SeparateIniFileContent(string[] file_content)
        {
            List<IniSection> ini_content = new List<IniSection>();

            for (int i = 0; i < file_content.Length; i++)
            {
                if (file_content[i].IndexOf(";") >= 0)
                    file_content[i] = file_content[i].Split(";")[0];
                if (file_content[i].IndexOf("#") >= 0)
                    file_content[i] = file_content[i].Split("#")[0];

                string line_type = GetIniLineType(file_content[i]);

                if (line_type == "EmptyLine")
                {
                    continue;
                }
                else if (line_type == "SectionName")
                {
                    IniSection ini_section = new IniSection();

                    ini_section.section_name = file_content[i].Trim();
                    ini_content.Add(ini_section);
                }
                else if (line_type == "SectionInner")
                {
                    IniSectionInner section_inner = new IniSectionInner();
                    string[] split_line = file_content[i].Split("=");

                    section_inner.parameter = split_line[0].Trim();
                    section_inner.value = split_line[1].Trim();

                    if (ini_content.Count != 0)
                        ini_content[ini_content.Count - 1].section_inners.Add(section_inner);
                    else
                        return ini_content;
                }
                else
                {
                    ini_content.Clear();
                    return ini_content;
                }
            }

            return ini_content;
        }


        //Метод, распознающий в переданной строке тот или иной элемент синтаксиса ini-файла (заголовок секции, содержание секции, пустая строка)
        private static string GetIniLineType(string ini_line)
        {
            Regex reg_section_name = new Regex(@"^\s*[\[]{1}\s*[a-zA-Z0-9\._\-\\\/%:\, ]+\s*[\]]{1}\s*$");
            //Менее строгий вариант
            Regex reg_section_inner = new Regex(@"^\s*[a-zA-Z0-9\._\-\\\/%:\, ]+\s*={1}[^=\[\]]*$");
            //Более строгий вариант
            //Regex reg_section_inner = new Regex(@"^\s*[a-zA-Z0-9\._\-\\\/%:\, ]+\s*={1}[a-zA-Z0-9\._\-\\\/%:\, +""'']*$");
            
            Regex reg_empty = new Regex(@"^\s*$");

            string ini_line_type;

            if (reg_section_name.IsMatch(ini_line))
                ini_line_type = "SectionName";
            else if (reg_section_inner.IsMatch(ini_line))
                ini_line_type = "SectionInner";
            else if (reg_empty.IsMatch(ini_line))
                ini_line_type = "EmptyLine";
            else
                ini_line_type = "UnknownType";

            return ini_line_type;
        }


        //Метод, непосредственно ответственный за сортировку содержимого ini-файла
        private static List<IniSection> SortIniSections(List<IniSection> ini_sections)
        {
            IniSection temp_section;
            for (int i = 0; i < ini_sections.Count; i++)
            {
                for (int j = i + 1; j < ini_sections.Count; j++)
                {
                    if (String.Compare(ini_sections[i].section_name, ini_sections[j].section_name) == 1)
                    {
                        temp_section = ini_sections[i];
                        ini_sections[i] = ini_sections[j];
                        ini_sections[j] = temp_section;
                    }
                }
            }

            IniSectionInner temp_section_inner;
            for (int n = 0; n < ini_sections.Count; n++)
            {
                for (int i = 0; i < ini_sections[n].section_inners.Count; i++)
                {
                    for (int j = i + 1; j < ini_sections[n].section_inners.Count; j++)
                    {
                        if (String.Compare(ini_sections[n].section_inners[i].parameter, ini_sections[n].section_inners[j].parameter) == 1)
                        {
                            temp_section_inner = ini_sections[n].section_inners[i];
                            ini_sections[n].section_inners[i] = ini_sections[n].section_inners[j];
                            ini_sections[n].section_inners[j] = temp_section_inner;
                        }
                    }
                }
            }

            return ini_sections;
        }


        //Метод, ответственный за "упаковку" содержания ini-файла в строку для последующей записи в ini-файл
        private static string ConvertIniSectionsToString(List<IniSection> ini_content)
        {
            string file_text = "";
            for (int i = 0; i < ini_content.Count; i++)
            {
                string section_name_line = ini_content[i].section_name + "\n";
                for (int j = 0; j < ini_content[i].section_inners.Count; j++)
                {
                    string secrion_inner_line = ini_content[i].section_inners[j].parameter + "=" + ini_content[i].section_inners[j].value + "\n";
                    section_name_line = section_name_line + secrion_inner_line;

                }
                file_text = file_text + "\n" + section_name_line;
            }
            return file_text;
        }
    }
}
