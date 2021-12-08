using LiuGuang.Common;
using LiuGuang.Common.axp;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace LiuGuang.UnAxp.ViewModels
{
    public class MainWindowViewModel : BindDataBase
    {
        #region Fields
        private string axpFilePath = string.Empty;
        private string outputPath = string.Empty;
        private bool runningTask = false;
        private int processCount = 0;
        private int totalFileCount = 100;
        #endregion

        #region Properties
        public string AxpFilePath
        {
            get => axpFilePath;
            set
            {
                if (SetProperty(ref axpFilePath, value))
                {
                    UnPackCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public string OutputPath
        {
            get => outputPath;
            set
            {
                if (SetProperty(ref outputPath, value))
                {
                    UnPackCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool RunningTask
        {
            set
            {
                if (SetProperty(ref runningTask, value))
                {
                    UnPackCommand.RaiseCanExecuteChanged();
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
        /// 解包命令
        /// </summary>
        public AppCommand UnPackCommand { get; }

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
            UnPackCommand = new AppCommand(DoUnpackAsync, CanUnpack);

        }

        /// <summary>
        /// 是否可以执行解包
        /// </summary>
        /// <returns></returns>
        public bool CanUnpack()
        {
            if (runningTask)
            {
                return false;
            }
            if (string.IsNullOrEmpty(axpFilePath))
            {
                return false;
            }
            if (string.IsNullOrEmpty(outputPath))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 处理解包
        /// </summary>
        private async void DoUnpackAsync()
        {
            RunningTask = true;
            try
            {
                await Task.Run(async () =>
                {
                    var packFile = new PackFile();
                    await packFile.UnPackAsync(axpFilePath, OutputPath, (processCount, totalCount) =>
                    {
                        ProcessCount = processCount;
                        TotalFileCount = totalCount;
                    });
                });
                ProcessCount = 0;
                MessageBox.Show("文件解包成功", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ProcessCount = 0;
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "出错了", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            RunningTask = false;

        }
    }
}
