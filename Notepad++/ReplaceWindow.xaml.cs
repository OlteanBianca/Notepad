using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Notepad__
{
    public partial class ReplaceWindow : Window
    {
        public string oldName = "";
        public string newName = "";
        public bool replace = false;
        public int option = -1;

        public ReplaceWindow()
        {
            InitializeComponent();
        }

        private void TextBox_ClearText(object sender, MouseButtonEventArgs e)
        {
            if ((sender as TextBox).Text == "Type here...")
            {
                (sender as TextBox).Text = "";
            }
        }

        private void FirstOption_Click(object sender, RoutedEventArgs e)
        {
            Close();
            oldName = "";
            newName = "";
        }

        private void SecondOption_Click(object sender, RoutedEventArgs e)
        {
            if (OldName.Text != "" && NewName.Text != "")
            {
                oldName = OldName.Text;
                newName = NewName.Text;
                replace = true;

                if (AllCurentFile.IsChecked ?? false)
                {
                    option = 2;
                }
                if (AllFiles.IsChecked ?? false)
                {
                    option = 3;
                }
                if (FirstCurrentFile.IsChecked ?? false)
                {
                    option = 1;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            oldName = "";
            newName = "";
        }
    }
}
