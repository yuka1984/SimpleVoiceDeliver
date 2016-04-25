using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
namespace NSpeexTest
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainViewModel Model
        {
            get
            {
                return DataContext as MainViewModel;
            }

            set
            {
                DataContext = value;
            }
        }

        public MainView()
        {
            InitializeComponent();
        }
    }
}
