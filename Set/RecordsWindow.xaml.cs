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
using System.IO;

namespace Set
{
    /// <summary>
    /// Interaction logic for RecordsWindow.xaml
    /// </summary>
    public partial class RecordsWindow : Window
    {
        const int iRecordsNumber = 10;
        int[] m_arrScore;
        string[] m_arrName;
        public RecordsWindow(string sNewName, int iNewScore)
        {
            InitializeComponent();
            m_arrScore = new int[iRecordsNumber];
            m_arrName = new string[iRecordsNumber];

            ReadRecorndsFromFile();
            AddNewRecord(sNewName, iNewScore);
            SaveRecordsToFile();
            ShowRecords();
        }
        private void AddNewRecord(string sNewName, int iNewScore)
        {
            for (int i = 0; i < iRecordsNumber; i++)
            {
                if (iNewScore < m_arrScore[i])
                {
                    continue;
                }
                // shifting values
                for (int j = iRecordsNumber - 1; j > i; j--)
                {
                    m_arrScore[j] = m_arrScore[j - 1];
                    m_arrName[j] = m_arrName[j - 1];
                }
                // adding new value
                m_arrScore[i] = iNewScore;
                m_arrName[i] = sNewName.Replace('~', ' ');
                break;
            }
        }
        private void ReadRecorndsFromFile()
        {
            for (int i = 0; i < iRecordsNumber; i++)
            {
                m_arrName[i] = "Some One";
                m_arrScore[i] = 100 - i * 10;
            }
            string sFileName = "Records.list";
            if (File.Exists(sFileName) == false)
            {
                return;
            }
            StreamReader reader = new StreamReader(sFileName);
            string sLine = "";
            int iLineNumber = 0;
            do
            {
                sLine = reader.ReadLine();
                if (sLine != null && sLine.Split('~').Length == 2)
                {
                    m_arrName[iLineNumber] = sLine.Split('~')[0];
                    m_arrScore[iLineNumber] = int.Parse(sLine.Split('~')[1]);
                    iLineNumber++;
                }
            } while (sLine != null);
            reader.Close();
        }
        private void SaveRecordsToFile()
        {
            StreamWriter writer = new StreamWriter("Records.list");
            for (int i = 0; i < iRecordsNumber; i++)
            {
                writer.WriteLine(m_arrName[i] + "~" + m_arrScore[i].ToString());
            }
            writer.Close();
        }
        private void ShowRecords()
        {
            foreach (UIElement elem in gridRecords.Children)
            {
                TextBlock textBlock = elem as TextBlock;
                if (textBlock != null)
                {
                    switch (Grid.GetColumn(textBlock))
                    {
                        case 0:
                            textBlock.Text = m_arrName[Grid.GetRow(textBlock)];
                            break;
                        case 1:
                            textBlock.Text = m_arrScore[Grid.GetRow(textBlock)].ToString();
                            break;

                    }
                }
            }
        }
    }
}
