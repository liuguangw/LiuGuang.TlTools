using LiuGuang.Common;
using LiuGuang.Common.axp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace LiuGuang.LuaDecompiler.ViewModels
{
    public class MainWindowViewModel : BindDataBase
    {
        #region Fields
        private string luaPath = string.Empty;
        private string outputPath = string.Empty;
        private bool runningTask = false;
        private byte[] keyData;
        private int processCount = 0;
        private int totalFileCount = 100;
        #endregion

        #region Properties
        public string LuaPath
        {
            get => luaPath;
            set
            {
                if (SetProperty(ref luaPath, value))
                {
                    DecompileCommand.RaiseCanExecuteChanged();
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
                    DecompileCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool RunningTask
        {
            set
            {
                if (SetProperty(ref runningTask, value))
                {
                    DecompileCommand.RaiseCanExecuteChanged();
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
        /// 解码命令
        /// </summary>
        public AppCommand DecompileCommand { get; }

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
            DecompileCommand = new AppCommand(DoDecompileAsync, CanDecompile);

        }

        /// <summary>
        /// 是否可以执行解码
        /// </summary>
        /// <returns></returns>
        public bool CanDecompile()
        {
            if (runningTask)
            {
                return false;
            }
            if (string.IsNullOrEmpty(luaPath))
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
        /// 处理解码
        /// </summary>
        private async void DoDecompileAsync()
        {
            RunningTask = true;
            try
            {
                await Task.Run(async () =>
                {
                    if (keyData == null)
                    {
                        keyData = await LoadKeyDataAsync();
                    }
                    //读取lua文件列表
                    var files = Directory.GetFiles(LuaPath, "*.lua", SearchOption.AllDirectories);
                    //
                    ProcessCount = 0;
                    TotalFileCount = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var srcFilePath = files[i];
                        var distPath = Path.Combine(OutputPath, srcFilePath.Substring(luaPath.Length + 1));
                        await DecompileFileAsync(srcFilePath, distPath);
                        ProcessCount++;
                    }
                });
                ProcessCount = 0;
                MessageBox.Show("解码成功", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ProcessCount = 0;
                MessageBox.Show(ex.Message /*+ "\n" + ex.StackTrace*/, "出错了", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            RunningTask = false;

        }

        private async Task<byte[]> LoadKeyDataAsync()
        {
            var uri = new Uri("key.data", UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            byte[] buff;
            using (var stream = info.Stream)
            {
                buff = new byte[stream.Length];
                await stream.ReadAsync(buff, 0, buff.Length);
            }
            return buff;
        }

        private async Task DecompileFileAsync(string srcFilePath, string distPath)
        {
            //不是lua结尾的
            if (!srcFilePath.EndsWith(".lua"))
            {
                return;
            }
            byte[] flag = { 0x84, 0xcc, 0xfa, 0x75 };
            using (var stream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                //判断是否为加密格式
                var tmpBuff = new byte[4];
                await stream.ReadAsync(tmpBuff, 0, tmpBuff.Length);
                if (!Enumerable.SequenceEqual(flag, tmpBuff))
                {
                    return;
                }
                //seek
                stream.Seek(0x60, SeekOrigin.Begin);
                var dataLength = (int)stream.Length - 0x60;
                var data = await decryptDataAsync(stream, dataLength);
                //创建文件夹
                var distDir = Path.GetDirectoryName(distPath);
                if (!Directory.Exists(distDir))
                {
                    Directory.CreateDirectory(distDir);
                }
                //写入文件
                using (var outputStream = new FileStream(distPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await outputStream.WriteAsync(data, 0, dataLength);
                }
            }
        }

        private async Task<byte[]> decryptDataAsync(FileStream stream, int dataLength)
        {
            var data = new byte[dataLength];
            await stream.ReadAsync(data, 0, dataLength);
            for (var i = 0; i < data.Length; i++)
            {
                data[i] ^= keyData[i % keyData.Length];
            }
            return data;
        }
    }
}
