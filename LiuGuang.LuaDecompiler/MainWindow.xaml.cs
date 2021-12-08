using LiuGuang.LuaDecompiler.ViewModels;
using System.Windows;

namespace LiuGuang.LuaDecompiler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private MainWindowViewModel GetViewModel()
        {
            return DataContext as MainWindowViewModel;
        }

        /// <summary>
        /// 选择客户端目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectLuaPath(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GetViewModel().LuaPath = dialog.SelectedPath;
            }
        }


        /// <summary>
        /// 选择明文目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectOutputPath(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GetViewModel().OutputPath = dialog.SelectedPath;
            }
        }
    }
}
