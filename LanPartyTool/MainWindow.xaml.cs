using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LanPartyTool.config;

namespace LanPartyTool
{
    public partial class MainWindow : Window
    {
        private readonly Config _config = Config.GetInstance();

        public MainWindow()
        {
            InitializeComponent();

            GameExeText.SetBinding(TextBox.TextProperty, new Binding()
            {
                Path = new PropertyPath("GameExe"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            CfgFileText.SetBinding(TextBox.TextProperty, new Binding()
            {
                Path = new PropertyPath("CfgFile"),
                Source = _config,
                Mode = BindingMode.OneWay
            });
        }
    }
}