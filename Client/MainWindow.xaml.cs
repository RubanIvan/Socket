using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>конфигурация сокета</summary>
        private Socket sListner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>процесс прослушки</summary>
        private Thread ThreadLisner;

        /// <summary>буффер приема </summary>
        private byte[] ResiveBuff = new byte[1024];

        //строка для входящего соединения
        private StringBuilder ResiveString = new StringBuilder();

        //сокет для установленного соединения
        //private Socket handler;


        public MainWindow()
        {
            InitializeComponent();
            GridCreateServer.Visibility = Visibility.Visible;
            GridServerScreen.Visibility = Visibility.Hidden;
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            GridCreateServer.Visibility = Visibility.Hidden;
            GridServerScreen.Visibility = Visibility.Visible;
            RichTextBox.AppendText("Клиент запущен \n");

            SocketInit();
        }

        /// <summary>инициализация сокета</summary>
        private void SocketInit()
        {
            IPEndPoint IpEndPoint = new IPEndPoint(IPAddress.Parse(TextBoxAddress.Text), int.Parse(TextBoxPort.Text));
            //подключаемся к адресу
            try
            {
                sListner.Connect(IpEndPoint);
            }
            catch (SocketException s)
            {
                MessageBox.Show(s.Message, "Error");
                Application.Current.Shutdown();
            }
           
            StartLisner();
        }

        /// <summary>стартуем новую нить которая ждет и обрабатывает входшие данные</summary>
        private void StartLisner()
        {

            #region ThreadLisner
            ThreadLisner = new Thread(() =>
            {
                while (true)
                {
                    ResiveString.Clear();

                    //данные пришли
                    while (true)
                    {
                        int ByteRes=0;
                        try
                        {
                            //получаем данные
                           ByteRes = sListner.Receive(ResiveBuff);
                        }
                        catch (SocketException s)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(s.Message, "Error");
                                Application.Current.Shutdown();
                            });
                            ThreadLisner.Abort();
                        }

                        //собираем строку до кучи
                        ResiveString.Append(Encoding.UTF8.GetString(ResiveBuff, 0, ByteRes));

                        //запоминаем длинну до изменений
                        int len = ResiveString.Length;
                        //меняем строку
                        ResiveString = ResiveString.Replace("<End>", "\n");

                        //если длинна изменилась т.е. была произведена замена. значит мы встретили маркер конца
                        if (ResiveString.Length < len) break;
                    }

                    //добавляем текст в текстбокс
                    Dispatcher.Invoke(() =>
                    {

                        TextRange rangeOfText1 = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
                        rangeOfText1.Text = "-Server-:";
                        rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                        rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                        rangeOfText1 = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
                        rangeOfText1.Text = ResiveString.ToString();
                        rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                        rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                        RichTextBox.ScrollToEnd();


                    });

                    //снова ждем входящих данных
                }
            });
            #endregion
            ThreadLisner.Name = "ThreadListner";
            ThreadLisner.Start();

        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if (sListner != null && sListner.Connected)
            {
                try
                {
                    sListner.Send(Encoding.UTF8.GetBytes(TextBoxSendText.Text + "<End>"));
                }
                catch (SocketException s)
                {   
                    MessageBox.Show(s.Message, "Error");
                    Application.Current.Shutdown();
                }

                TextRange rangeOfText1 = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
                rangeOfText1.Text = "-Client-: ";
                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Chocolate);
                rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                rangeOfText1 = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
                rangeOfText1.Text = TextBoxSendText.Text + "\n";
                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                RichTextBox.ScrollToEnd();

                TextBoxSendText.Text = string.Empty;

            }
        }


        /// <summary>убиваем процесс слушатель в случае закрытии окна</summary>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            //if (handler != null) handler.Close();
            if (sListner != null) sListner.Close();
            if (ThreadLisner != null) ThreadLisner.Abort();
        }


    }
}
