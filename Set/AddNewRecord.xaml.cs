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

namespace Set
{
    /// <summary>
    /// Interaction logic for AddNewRecord.xaml
    /// </summary>
    public partial class AddNewRecord : Window
    {
        int m_iScore = 0;
        bool m_bNewGame = false;

        public bool NewGame
        {
            get { return m_bNewGame; }
        }
        public string NewName
        {
            get { return textBoxName.Text; }
        }
        public AddNewRecord(int iScore)
        {
            InitializeComponent();
            m_iScore = iScore;
            textBoxName.Text = "Some One";
            textBlockScore.Text = "Score: " + m_iScore.ToString();
        }
        private void buttonNewGame_Click(object sender, RoutedEventArgs e)
        {
            m_bNewGame = true;
            Close();
        }
        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
            m_bNewGame = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxName.Focus();
            textBoxName.SelectAll();
        }
    }
}
