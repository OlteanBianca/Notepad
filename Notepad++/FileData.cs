using System.ComponentModel;
using System.IO;

namespace Notepad__
{
    public class FileData : INotifyPropertyChanged
    {
        private string name;
        private string content;
        public bool changed = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public FileData() { }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string Content
        {
            get => content;
            set
            {
                content = value;
                NotifyPropertyChanged("Content");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (propertyName == "Content")
                {
                    if (Name.Contains("* "))
                    {
                        Name = Name.Remove(0, 2);
                    }
                    changed = true;

                    File.WriteAllText(@"..\..\UnsavedFiles\" + Name, Content);
                    Name = "* " + Name;
                }
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
