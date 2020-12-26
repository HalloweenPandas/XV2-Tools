﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;
using System.IO;
using System.Windows;

namespace EEPK_Organiser.Settings
{
    public enum AppTheme
    {
        Light,
        Dark,
        WindowsDefault
    }

    [YAXSerializeAs("Settings")]
    public class AppSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [YAXDontSerialize]
        public bool ValidGameDir
        {
            get
            {
                return (File.Exists(String.Format("{0}/bin/DBXV2.exe", GameDirectory)));
            }
        }

        #region Values
        private bool _assetReuseNameMatch = false;
        private bool _textureReuse_Identical = true;
        private bool _textureReuse_NameMatch = false;
        private bool _fileCleanUp_Delete = false;
        private bool _fileCleanUp_Prompt = true;
        private bool _fileCleanUp_Ignore = false;
        private string _gameDir = null;
        private bool _autoContainerRename = true;
        private int _fileCacheLimit = 7;
        private AppTheme _currentTheme = AppTheme.WindowsDefault;
        #endregion

        public bool UpdateNotifications { get; set; } = true;
        public bool LoadTextures { get; set; } = true;
        public bool AssetReuse_NameMatch
        {
            get
            {
                return this._assetReuseNameMatch;
            }
            set
            {
                if (value != this._assetReuseNameMatch)
                {
                    this._assetReuseNameMatch = value;
                    NotifyPropertyChanged("AssetReuse_NameMatch");
                }
            }
        }
        public bool TextureReuse_Identical
        {
            get
            {
                return this._textureReuse_Identical;
            }
            set
            {
                if (value != this._textureReuse_Identical)
                {
                    this._textureReuse_Identical = value;
                    NotifyPropertyChanged("TextureReuse_Identical");
                }
            }
        }
        public bool TextureReuse_NameMatch
        {
            get
            {
                return this._textureReuse_NameMatch;
            }
            set
            {
                if (value != this._textureReuse_NameMatch)
                {
                    this._textureReuse_NameMatch = value;
                    NotifyPropertyChanged("TextureReuse_NameMatch");
                }
            }
        }
        public bool FileCleanUp_Delete
        {
            get
            {
                return this._fileCleanUp_Delete;
            }
            set
            {
                if (value != this._fileCleanUp_Delete)
                {
                    this._fileCleanUp_Delete = value;
                    NotifyPropertyChanged("FileCleanUp_Delete");
                }
            }
        }
        public bool FileCleanUp_Prompt
        {
            get
            {
                return this._fileCleanUp_Prompt;
            }
            set
            {
                if (value != this._fileCleanUp_Prompt)
                {
                    this._fileCleanUp_Prompt = value;
                    NotifyPropertyChanged("FileCleanUp_Prompt");
                }
            }
        }
        public bool FileCleanUp_Ignore
        {
            get
            {
                return this._fileCleanUp_Ignore;
            }
            set
            {
                if (value != this._fileCleanUp_Ignore)
                {
                    this._fileCleanUp_Ignore = value;
                    NotifyPropertyChanged("FileCleanUp_Ignore");
                }
            }
        }
        public string GameDirectory
        {
            get
            {
                return this._gameDir;
            }
            set
            {
                if (value != this._gameDir)
                {
                    this._gameDir = value;
                    NotifyPropertyChanged("GameDirectory");
                }
            }
        }
        public bool AutoContainerRename
        {
            get
            {
                return this._autoContainerRename;
            }
            set
            {
                if (value != this._autoContainerRename)
                {
                    this._autoContainerRename = value;
                    NotifyPropertyChanged("AutoContainerRename");
                }
            }
        }
        public int FileCacheLimit
        {
            get
            {
                return this._fileCacheLimit;
            }
            set
            {
                if (value != this._fileCacheLimit)
                {
                    this._fileCacheLimit = value;
                    NotifyPropertyChanged("FileCacheLimit");
                }
            }
        }
        public bool UseLightTheme
        {
            get
            {
                return _currentTheme == AppTheme.Light;
            }
            set
            {
                if (value)
                    _currentTheme = AppTheme.Light;

                NotifyPropertyChanged(nameof(UseLightTheme));
            }
        }
        public bool UseDarkTheme
        {
            get
            {
                return _currentTheme == AppTheme.Dark;
            }
            set
            {
                if (value)
                    _currentTheme = AppTheme.Dark;

                NotifyPropertyChanged(nameof(UseDarkTheme));
            }
        }
        public bool UseWindowsTheme
        {
            get
            {
                return _currentTheme == AppTheme.WindowsDefault;
            }
            set
            {
                if (value)
                    _currentTheme = AppTheme.WindowsDefault;

                NotifyPropertyChanged(nameof(UseWindowsTheme));
            }
        }


        public static AppSettings LoadSettings()
        {
            try
            {
                //Try to load the settings
                YAXSerializer serializer = new YAXSerializer(typeof(AppSettings), YAXSerializationOptions.DontSerializeNullObjects);
                var settings = (AppSettings)serializer.DeserializeFromFile(GeneralInfo.SETTINGS_PATH);
                settings.InitSettings();
                settings.ValidateSettings();
                return settings;
            }
            catch
            {
                //If it fails, create a new instance and save it to disk.
                var newSettings = new AppSettings()
                {
                };

                newSettings.InitSettings();
                newSettings.SaveSettings();

                return newSettings;
            }
            finally
            {
                GeneralInfo.UpdateEepkToolInterlop();
            }

        }

        public void SaveSettings()
        {
            try
            {
                ValidateSettings();
                if (!Directory.Exists(Path.GetDirectoryName(GeneralInfo.SETTINGS_PATH)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(GeneralInfo.SETTINGS_PATH));
                }

                YAXSerializer serializer = new YAXSerializer(typeof(AppSettings));
                serializer.SerializeToFile(this, GeneralInfo.SETTINGS_PATH);
            }
            catch
            {
                MessageBox.Show("Failed to save settings.", "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                GeneralInfo.UpdateEepkToolInterlop();
            }
        }

        private void InitSettings()
        {
            if(string.IsNullOrWhiteSpace(GameDirectory) || !ValidGameDir)
            {
                GameDirectory = FindGameDirectory();
            }
            
        }

        private void ValidateSettings()
        {
            if (TextureReuse_Identical == false && TextureReuse_NameMatch == false)
            {
                TextureReuse_Identical = true;
            }

            if (FileCleanUp_Delete == false && FileCleanUp_Ignore == false && FileCleanUp_Prompt == false)
            {
                FileCleanUp_Prompt = true;
            }
        }

        private string FindGameDirectory()
        {
            List<string> alphabet = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "O", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            
            foreach(var letter in alphabet)
            {
                string _path1 = String.Format(@"{0}:{1}Program Files (x86){1}Steam{1}steamapps{1}common{1}DB Xenoverse 2{1}bin{1}DBXV2.exe", letter, System.IO.Path.DirectorySeparatorChar);
                string _path2 = String.Format(@"{0}:{1}Games{1}Steam{1}steamapps{1}common{1}DB Xenoverse 2{1}bin{1}DBXV2.exe", letter, System.IO.Path.DirectorySeparatorChar);
                string _path3 = String.Format(@"{0}:{1}Steam{1}steamapps{1}common{1}DB Xenoverse 2{1}bin{1}DBXV2.exe", letter, System.IO.Path.DirectorySeparatorChar);
                
                if (File.Exists(_path1))
                {
                    return Path.GetDirectoryName(Path.GetDirectoryName(_path1));
                }
                else if (File.Exists(_path2))
                {
                    return Path.GetDirectoryName(Path.GetDirectoryName(_path2));
                }
                else if (File.Exists(_path3))
                {
                    return Path.GetDirectoryName(Path.GetDirectoryName(_path3));
                }
            }

            return null;
        }

        public AppTheme GetCurrentTheme()
        {
            return _currentTheme;
        }

    }

}
