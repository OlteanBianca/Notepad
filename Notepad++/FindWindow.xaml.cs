using System;
using System.Windows;
using System.Windows.Input;

namespace Notepad__
{
    public partial class FindWindow : Window
    {
        public string TextToSearch = "";
        public bool search = false, next = false, preview = false;
        public int option = -1;

        public FindWindow()
        {
            InitializeComponent();
        }

        private void TextBox_ClearText(object sender, MouseButtonEventArgs e)
        {
            if (MyTextBox.Text == "Type here...")
            {
                MyTextBox.Text = "";
            }
        }

        private void FirstOption_Click(object sender, RoutedEventArgs e)
        {
            Close();
            TextToSearch = "";
        }

        private void SecondOption_Click(object sender, RoutedEventArgs e)
        {
            if (MyTextBox.Text != "")
            {
                TextToSearch = MyTextBox.Text;
                search = true;

                if(CurrentFile.IsChecked ?? false)
                {
                    option = 1;
                }
                if(AllFiles.IsChecked ?? false)
                {
                    option = 2;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            TextToSearch = "";
        }
    }
}
