﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace IndexerTestWASDK
{
    public enum IconType : byte
    {
        File = 0,
        Folder = 1
    }

    public class IndexedFileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Name { get; set; }
        public string Path { get; set; }
<<<<<<< Updated upstream
        public IconType Type { get; set; }
=======
        public string Type { get; set; }
        private string creationTime;
        public string CreationTime
        {
            get { return creationTime; }
            set
            {
                if (value != creationTime)
                {
                    creationTime = value;
                    NotifyPropertyChanged(nameof(CreationTime));
                }
            }
        }
        private string lastAccessTime;
        public string LastAccessTime
        {
            get { return lastAccessTime; }
            set
            {
                if (value != lastAccessTime)
                {
                    lastAccessTime = value;
                    NotifyPropertyChanged(nameof(LastAccessTime));
                }
            }
        }
        private string lastWriteTime;
        public string LastWriteTime
        {
            get { return lastWriteTime; }
            set
            {
                if (value != lastWriteTime)
                {
                    lastWriteTime = value;
                    NotifyPropertyChanged(nameof(LastWriteTime));
                }
            }
        }
        private double length;
        public double Length
        {
            get { return length; }
            set
            {
                if (value != length)
                {
                    length = value;
                    NotifyPropertyChanged(nameof(Length));
                }
            }
        }

>>>>>>> Stashed changes
        private ImageSource image;

        public ImageSource Image
        {
            get { return image; }
            set
            {
                if (value != image)
                {
                    image = value;
                    NotifyPropertyChanged(nameof(Image));
                }
            }
        }
    }
}
