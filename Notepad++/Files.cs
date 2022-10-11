using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Notepad__
{
    class Files
    {
        // OpenFiles -> full path
        // OriginalPaths -> ( full path, name, saved/unsaved)

        public List<Tuple<string, string, bool>> OriginalPaths { get; set; } = new List<Tuple<string, string, bool>>();
        public List<string> OpenFiles { get; set; } = new List<string>();

        private List<List<int>> FoundIndexes = new List<List<int>>();

        private int newFilesCount = 1, currentIndex = 0, currentTab = -1;

        public int Line { private get; set; }
        public string button = "";
        private readonly string pathUnsaved = @"..\..\UnsavedFiles\";

        public Files()
        {
            ReadFile();

            Line = 0;
            DirectoryInfo directory = new DirectoryInfo(pathUnsaved);
            foreach (FileInfo it in directory.GetFiles())
            {
                OpenFiles.Add(it.Name);
            }
        }

        public void ReadFile()
        {
            string[] lines = File.ReadAllLines(@"..\..\oldPath.txt");

            foreach (string line in lines)
            {
                bool saved = line[0] != '0'; // true if saved, else false

                OriginalPaths.Add(new Tuple<string, string, bool>(line.Remove(0, 1), Path.GetFileName(line), saved));
            }
        }

        public bool IsFile(string path)
        {
            return Path.GetExtension(path) == ".txt" || Path.GetExtension(path) == ".xlsx"
                || Path.GetExtension(path) == ".docx" || Path.GetExtension(path) == ".pdf";
        }

        public bool CheckIfFileHasOriginalPath(string name)
        {
            name = UpdateName(name);
            return OriginalPaths.Find(val => val.Item2.Contains(name)) != null;
        }

        public string UpdateName(string name)
        {
            return name.Contains("* ") ? name.Remove(0, 2) : name;
        }

        public void OpenFile(string fileName)
        {
            OriginalPaths.Add(new Tuple<string, string, bool>(fileName, Path.GetFileName(fileName), true));
            OpenFiles.Add(Path.GetFileName(fileName));

            if (!File.Exists(pathUnsaved))
            {
                File.Copy(fileName, pathUnsaved + Path.GetFileName(fileName), true);
            }
        }

        public string NewFile()
        {
            string newName = "New " + newFilesCount.ToString() + ".txt";
            while (File.Exists(pathUnsaved + newName))
            {
                newFilesCount++;
                newName = "New " + newFilesCount.ToString() + ".txt";
            }
            using (StreamWriter sw = File.CreateText(pathUnsaved + newName)) { }
            using (StreamReader sr = File.OpenText(pathUnsaved + newName)) { }

            OpenFiles.Add(newName);
            newFilesCount = 1;

            return newName;
        }

        public string SaveFile(string name, string info)
        {
            name = UpdateName(name);
            int index = OriginalPaths.FindIndex(val => val.Item2.Contains(name));
            string originalPath = OriginalPaths[index].Item1;

            File.WriteAllText(pathUnsaved + name, info);
            File.WriteAllText(originalPath, info);

            OriginalPaths[index] = new Tuple<string, string, bool>(originalPath, name, true);
            return name;
        }

        public string SaveAsFile(string old_path, string new_path, string content)
        {
            old_path = UpdateName(old_path);
            OriginalPaths.Add(new Tuple<string, string, bool>(new_path, Path.GetFileName(new_path), true));

            File.WriteAllText(new_path, content);
            File.Delete(pathUnsaved + old_path);
            File.Copy(new_path, pathUnsaved + Path.GetFileName(new_path));

            return Path.GetFileName(new_path);
        }

        public void Delete(string name)
        {
            name = UpdateName(name);
            _ = OpenFiles.Remove(name);
            File.Delete(pathUnsaved + name);
        }

        public bool CloseFile(string name)
        {
            name = UpdateName(name);

            Tuple<string, string, bool> it = OriginalPaths.Find(val => val.Item2 == name);
            if (it != null && it.Item3)
            {
                _ = OriginalPaths.Remove(it);
                _ = OpenFiles.Remove(it.Item1);
                File.Delete(pathUnsaved + name);
                return true;
            }
            return false;
        }

        public void UpdateList(string name)
        {
            name = UpdateName(name);

            int it = OriginalPaths.FindIndex(val => val.Item2 == name);
            if (it != -1)
            {
                OriginalPaths[it] = new Tuple<string, string, bool>(OriginalPaths[it].Item1, OriginalPaths[it].Item2, false);
            }
        }

        public List<string> TreeView(string root)
        {
            List<string> all_files = new List<string>();

            string[] next = null;
            try
            {
                next = Directory.GetFiles(root);
            }
            catch { }

            if (next != null && next.Length != 0)
            {
                foreach (string file in next)
                {
                    if (IsFile(file))
                    {
                        all_files.Add(file);
                    }
                }
            }
            try
            {
                next = Directory.GetDirectories(root);
                foreach (string subdir in next)
                {
                    if (Path.GetFileName(subdir)[0] != '$')
                    {
                        all_files.Add(subdir);
                    }
                }
            }
            catch { }
            return all_files;
        }

        public void SetIndexes(string textToSearch, string content)
        {
            int index = 0;
            FoundIndexes = new List<List<int>>();
            List<int> curentIndexes = new List<int>();
            currentIndex = 0;
            currentTab = 0;
            do
            {
                index = content.IndexOf(textToSearch, index);
                if (index != -1)
                {
                    curentIndexes.Add(index);
                    index++;
                }
            } while (index != -1);

            if (curentIndexes.Count != 0)
            {
                FoundIndexes.Add(curentIndexes);
            }
        }

        public void SetAllIndexes(string textToSearch, ObservableCollection<FileData> ListForFiles, int tab)
        {
            FoundIndexes = new List<List<int>>();
            currentIndex = 0;
            currentTab = tab;

            foreach (FileData file in ListForFiles)
            {
                int index = 0;
                List<int> curentIndexes = new List<int>();
                do
                {
                    index = file.Content.IndexOf(textToSearch, index);
                    if (index != -1)
                    {
                        curentIndexes.Add(index);
                        index++;
                    }
                } while (index != -1);

                FoundIndexes.Add(curentIndexes);
            }
        }

        public int GetIndex(int direction, int option)
        {
            if (FoundIndexes[currentTab].Count != 0)
            {
                if (direction == 1)
                {
                    if (currentIndex < FoundIndexes[currentTab].Count - 1)
                    {
                        currentIndex++;
                    }
                    else if (currentIndex == FoundIndexes[currentTab].Count - 1)
                    {
                        currentIndex = 0;
                        if (option == 2)
                        {
                            GetTab(FoundIndexes.Count, direction);
                        }
                    }
                }
                else if (direction == -1)
                {
                    if (currentIndex > 0)
                    {
                        currentIndex--;
                    }
                    else if (currentIndex == 0)
                    {
                        if (option == 2)
                        {
                            GetTab(FoundIndexes.Count, direction);
                        }
                        currentIndex = FoundIndexes[currentTab].Count - 1;
                    }
                }
                return FoundIndexes[currentTab][currentIndex];
            }
            return -1;
        }

        public void GetTab(int length, int direction)
        {
            do
            {
                if (direction == 1)
                {
                    if (currentTab == length - 1)
                    {
                        currentTab = 0;
                    }
                    else
                    {
                        currentTab++;
                    }
                }
                else if (direction == -1)
                {
                    if (currentTab == 0)
                    {
                        currentTab = length - 1;
                    }
                    else
                    {
                        currentTab--;
                    }
                }
            } while (FoundIndexes[currentTab].Count == 0);
        }

        public string Replace(string oldWord, string newWord, string content)
        {
            int index = content.IndexOf(oldWord);
            if (index != -1)
            {
                Regex regex = new Regex(Regex.Escape(oldWord));
                return regex.Replace(content, newWord, 1);
            }
            return content;
        }

        public string ReplaceAll(string oldWord, string newWord, string content)
        {
            return content.Replace(oldWord, newWord);
        }

        public void Buttons(TextBox textbox)
        {
            switch (button)
            {
                case "uppercase":
                    {
                        textbox.SelectedText = textbox.SelectedText.ToUpper();
                        break;
                    }
                case "lowercase":
                    {
                        textbox.SelectedText = textbox.SelectedText.ToLower();
                        break;
                    }
                case "cut":
                    {
                        textbox.Cut();
                        break;
                    }
                case "paste":
                    {
                        if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
                        {
                            if (textbox.SelectionLength > 0)
                            {
                                textbox.SelectionStart += textbox.SelectionLength;
                            }
                            textbox.Paste();
                            break;
                        }
                        break;
                    }
                case "copy":
                    {
                        textbox.Copy();
                        break;
                    }
                case "removeEmptyLines":
                    {
                        textbox.Text = Regex.Replace(textbox.Text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                        break;
                    }
                case "readOnly":
                    {
                        textbox.IsReadOnly = !textbox.IsReadOnly;
                        break;
                    }
                case "go to line":
                    {
                        if (Line < textbox.LineCount && Line > 0)
                        {
                            textbox.SelectionStart = textbox.GetCharacterIndexFromLineIndex(Line - 1);
                            textbox.SelectionLength = textbox.GetLineLength(Line - 1);
                            textbox.CaretIndex = textbox.SelectionStart;
                            textbox.ScrollToLine(Line - 1);
                            textbox.Focus();
                            textbox.Select(textbox.SelectionStart, textbox.GetLineLength(Line - 1));
                        }
                        break;
                    }
                default:
                    break;
            }
            button = "";
        }

        ~Files()
        {
            using (StreamWriter file = File.CreateText(@"..\..\oldPath.txt"))
            {
                foreach (Tuple<string, string, bool> val in OriginalPaths)
                {
                    file.WriteLine(Convert.ToInt32(val.Item3) + val.Item1);
                }
            }
        }
    }
}
