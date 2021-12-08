using LiuGuang.Common;
using LiuGuang.Common.axp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace LiuGuang.AssetChecker.ViewModels
{
    public class MainWindowViewModel : BindDataBase
    {
        #region Fields
        private string clientPath = string.Empty;
        private string sourcePath = string.Empty;
        private bool runningTask = false;
        private int processCount = 0;
        private int totalFileCount = 100;
        #endregion

        #region Properties
        public string ClientPath
        {
            get => clientPath;
            set
            {
                if (SetProperty(ref clientPath, value))
                {
                    CheckCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public string SourcePath
        {
            get => sourcePath;
            set
            {
                if (SetProperty(ref sourcePath, value))
                {
                    CheckCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool RunningTask
        {
            set
            {
                if (SetProperty(ref runningTask, value))
                {
                    CheckCommand.RaiseCanExecuteChanged();
                    RaisePropertyChanged(nameof(CanSelectFile));
                }
            }
        }

        /// <summary>
        /// 是否可以选择文件
        /// </summary>
        public bool CanSelectFile
        {
            get => !runningTask;
        }

        /// <summary>
        /// 检测命令
        /// </summary>
        public AppCommand CheckCommand { get; }

        public int ProcessCount
        {
            get => processCount;
            set => SetProperty(ref processCount, value);
        }
        public int TotalFileCount
        {
            get => totalFileCount;
            set => SetProperty(ref totalFileCount, value);
        }
        #endregion

        public MainWindowViewModel()
        {
            CheckCommand = new AppCommand(DoCheckAsync, CanCheck);

        }

        /// <summary>
        /// 是否可以执行检测
        /// </summary>
        /// <returns></returns>
        public bool CanCheck()
        {
            if (runningTask)
            {
                return false;
            }
            if (string.IsNullOrEmpty(clientPath))
            {
                return false;
            }
            if (string.IsNullOrEmpty(sourcePath))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 处理检测
        /// </summary>
        private async void DoCheckAsync()
        {
            RunningTask = true;
            FileStream interfaceStream = null;
            try
            {
                await Task.Run(async () =>
                {
                    var imagesetList = await LoadImagesetListAsync();
                    ProcessCount = 0;
                    TotalFileCount = imagesetList.Count();
                    //读取Interface.axp
                    var interfacePackFilePath = Path.Combine(clientPath, "Data", "Interface.axp");
                    var interfacePackFile = new PackFile();
                    interfaceStream = new FileStream(interfacePackFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await interfacePackFile.LoadAsync(interfaceStream);
                    //读取Material.axp
                    var materialPackFilePath = Path.Combine(clientPath, "Data", "Material.axp");
                    var materialPackFile = new PackFile();
                    using (var materialStream = new FileStream(materialPackFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await materialPackFile.LoadAsync(materialStream);

                    }
                    foreach (var imageset in imagesetList)
                    {
                        await imageset.CheckFileAsync(sourcePath, interfaceStream, interfacePackFile, materialPackFile);
                        ProcessCount++;

                    }
                });
                ProcessCount = 0;
                MessageBox.Show("检查成功", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ProcessCount = 0;
                MessageBox.Show(ex.Message /*+ "\n" + ex.StackTrace*/, "出错了", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (interfaceStream != null)
                {
                    interfaceStream.Dispose();
                }
            }
            RunningTask = false;

        }

        public async Task<IEnumerable<Imageset>> LoadImagesetListAsync()
        {
            var schemePath = Path.Combine(sourcePath, "Interface", "Schema", "WoWLookSkin.scheme.xml");
            string fileContent;
            using (var streamReader = File.OpenText(schemePath))
            {
                fileContent = await streamReader.ReadToEndAsync();
            }
            var schemeXml = XElement.Parse(fileContent);
            var imagesetList = from imagesetEl in schemeXml.Descendants("Imageset")
                               select new Imageset()
                               {
                                   Name = imagesetEl.Attribute("Name")?.Value ?? string.Empty,
                                   Filename = imagesetEl.Attribute("Filename")?.Value ?? string.Empty,
                               };
            return imagesetList;
        }
    }
}
