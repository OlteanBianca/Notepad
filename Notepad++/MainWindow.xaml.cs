using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System;
using Path = System.IO.Path;

namespace Notepad__
{
    public partial class MainWindow : Window
    {
        private bool check = false;
        private int index = -1;
        private readonly Files UserFiles = new Files();
        private FindWindow SearchWindow;
        private ReplaceWindow replaceWindow;
        public ObservableCollection<FileData> ListForFiles { get; set; } = new ObservableCollection<FileData> { };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            foreach (string name in UserFiles.OpenFiles)
            {
                Open(name);
            }
            tabControl.SelectedItem = ListForFiles[0];

            string path = @"C:\Users\bienc\Desktop";
            TreeViewItem root = new TreeViewItem { Header = Path.GetFileName(path) };
            root.Tag = path;
            root.IsExpanded = true;
            _ = myTreeView.Items.Add(root);
        }

        private void Open(string name)
        {
            string text = File.ReadAllText(@"..\..\UnsavedFiles\" + name);

            int index = UserFiles.OriginalPaths.FindIndex(val => val.Item2.Contains(name));
            if (!(index != -1 && UserFiles.OriginalPaths[index].Item3))
            {
                name = "* " + name;
            }

            FileData myfiles = new FileData()
            {
                Name = name,
                Content = text
            };

            ListForFiles.Add(myfiles);
        }

        private void NewFile(object sender, ExecutedRoutedEventArgs e)
        {
            string name = UserFiles.NewFile();
            Open(name);
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"c:\users\bienc\desktop\facultate\anul 2-sem 2\mvp\tema 1\notepad++\notepad++"
            };
            _ = openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                UserFiles.OpenFile(openFileDialog.FileName);
                Open(Path.GetFileName(openFileDialog.FileName));
            }
        }

        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            FileData selected_file = ListForFiles[tabControl.SelectedIndex];

            if (UserFiles.CheckIfFileHasOriginalPath(selected_file.Name))
            {
                selected_file.Name = UserFiles.SaveFile(selected_file.Name, selected_file.Content);
            }
            else
            {
                SaveAsFile(sender, e);
            }
        }

        private void SaveAsFile(object sender, ExecutedRoutedEventArgs e)
        {
            FileData selected_file = ListForFiles[tabControl.SelectedIndex];

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|C# file (*.cs)|*.cs",
                InitialDirectory = @"c:\users\bienc\desktop\facultate\anul 2-sem 2\mvp"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                selected_file.Name = UserFiles.SaveAsFile(selected_file.Name, saveFileDialog.FileName, selected_file.Content);
            }
        }

        private void Exit(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Find(object sender, ExecutedRoutedEventArgs e)
        {
            SearchWindow = new FindWindow();
            SearchWindow.SecondOption.Content = "Search";
            SearchWindow.FirstOption.Content = "Close";
            SearchWindow.Show();
        }

        private void Replace(object sender, ExecutedRoutedEventArgs e)
        {
            replaceWindow = new ReplaceWindow();
            replaceWindow.SecondOption.Content = "Replace";
            replaceWindow.FirstOption.Content = "Close";
            replaceWindow.Show();
        }

        private void About(object sender, ExecutedRoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow();
            window.Show();
        }

        private void Close_Clicked(object sender, RoutedEventArgs e)
        {
            FileData selected_file = ListForFiles[tabControl.SelectedIndex];

            if (selected_file.Name.Contains("* "))
            {
                MessageBoxResult result = MessageBox.Show("Save file?", "Close", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (UserFiles.CheckIfFileHasOriginalPath(selected_file.Name))
                    {
                        SaveFile(sender, e as ExecutedRoutedEventArgs);
                    }
                    else
                    {
                        SaveAsFile(sender, e as ExecutedRoutedEventArgs);
                    }
                }
                else if (result == MessageBoxResult.No)
                {
                    UserFiles.Delete(selected_file.Name);
                    _ = ListForFiles.Remove(selected_file);
                    return;
                }
                else
                {
                    return;
                }
            }
            if (UserFiles.CloseFile(selected_file.Name))
            {
                _ = ListForFiles.Remove(selected_file);
            }
        }

        private void Next_Clicked(object sender, RoutedEventArgs e)
        {
            index = UserFiles.GetIndex(1, SearchWindow.option);
            check = true;
        }

        private void Previous_Clicked(object sender, RoutedEventArgs e)
        {
            index = UserFiles.GetIndex(-1, SearchWindow.option);
            check = true;
        }

        private void Button_Clicked(object sender, RoutedEventArgs e)
        {
            UserFiles.button = (sender as Button).Name;
        }

        private void TextBox_MouseMove(object sender, MouseEventArgs e)
        {
            UserFiles.Buttons(sender as TextBox);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tabControl.SelectedItem != null)
            {
                FileData item = ListForFiles[tabControl.SelectedIndex];
                if (item.changed)
                {
                    UserFiles.UpdateList(item.Name);
                }
            }
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (SearchWindow != null && SearchWindow.TextToSearch != "")
            {
                if (SearchWindow.search)
                {
                    if (SearchWindow.option == 1)
                    {
                        UserFiles.SetIndexes(SearchWindow.TextToSearch, ListForFiles[tabControl.SelectedIndex].Content);
                    }
                    if (SearchWindow.option == 2)
                    {
                        UserFiles.SetAllIndexes(SearchWindow.TextToSearch, ListForFiles, tabControl.SelectedIndex);
                    }
                    index = UserFiles.GetIndex(0, SearchWindow.option);
                    SearchWindow.search = false;
                    check = true;
                }
                if (index != -1 && check)
                {
                    TextBox textbox = sender as TextBox;
                    check = false;
                    textbox.Select(index, SearchWindow.TextToSearch.Length);
                }
            }

            if (replaceWindow != null && replaceWindow.newName != "" && replaceWindow.oldName != "")
            {
                if (replaceWindow.replace)
                {
                    if (replaceWindow.option == 1)
                    {
                        TextBox textbox = sender as TextBox;
                        replaceWindow.replace = false;
                        textbox.Text = UserFiles.Replace(replaceWindow.oldName, replaceWindow.newName, textbox.Text);
                    }
                    if (replaceWindow.option == 2)
                    {
                        TextBox textbox = sender as TextBox;
                        replaceWindow.replace = false;
                        textbox.Text = UserFiles.ReplaceAll(replaceWindow.oldName, replaceWindow.newName, textbox.Text);
                    }
                    if (replaceWindow.option == 3)
                    {
                        replaceWindow.replace = false;
                        foreach (FileData file in ListForFiles)
                        {
                            file.Content = UserFiles.ReplaceAll(replaceWindow.oldName, replaceWindow.newName, file.Content);
                        }
                    }
                }
            }
        }

        private void MyTreeView_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem selected = myTreeView.SelectedItem as TreeViewItem;
            selected.Items.Clear();

            string path = selected.Tag.ToString();
            if (UserFiles.IsFile(path))
            {
                UserFiles.OpenFile(path);
                Open(Path.GetFileName(Path.GetFileName(path)));
            }
            else
            {
                List<string> treeItemItems = UserFiles.TreeView(path);

                foreach (string it in treeItemItems)
                {
                    TreeViewItem directoryNode = new TreeViewItem { Header = Path.GetFileName(it) };
                    directoryNode.Tag = it;
                    directoryNode.IsExpanded = true;
                    _ = selected.Items.Add(directoryNode);
                }
            }
        }

        private void GoToLine_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (goToLine.Text.Length != 0)
            {
                UserFiles.Line = Convert.ToInt32(goToLine.Text);
                UserFiles.button = "go to line";
            }
        }
    }
}