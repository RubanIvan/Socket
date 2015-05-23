using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Server
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
        private Socket handler;


        public MainWindow()
        {
            InitializeComponent();
            GridCreateServer.Visibility = Visibility.Visible;
            GridServerScreen.Visibility = Visibility.Hidden;
        }

        /// <summary>меняем местами гриды. показываем основное окно</summary>
        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            GridCreateServer.Visibility = Visibility.Hidden;
            GridServerScreen.Visibility = Visibility.Visible;
            RichTextBox.AppendText("Сервер запущен \n");

            SocketInit();
        }

        /// <summary>инициализация сокета</summary>
        private void SocketInit()
        {
            IPEndPoint IpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), int.Parse(TextBoxPort.Text));
            sListner.Bind(IpEndPoint);
            sListner.Listen(1);
            StartLisner();
        }

        /// <summary>стартуем новую нить которая ждет и обрабатывает входшие данные</summary>
        private void StartLisner()
        {

            #region ThreadLisner
            ThreadLisner = new Thread(() =>
                {
                    //ждем входяшие соединение
                    handler = sListner.Accept();

                    while (true)
                    {
                        ResiveString.Clear();

                        //данные пришли
                        while (true)
                        {
                            //получаем данные
                            int ByteRes = handler.Receive(ResiveBuff);

                            //собираем строку до кучи
                            ResiveString.Append(Encoding.ASCII.GetString(ResiveBuff, 0, ByteRes));

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
                            rangeOfText1.Text = "-Client-:";
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


        /// <summary>убиваем процесс слушатель в случае закрытии окна</summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            sListner.Shutdown(SocketShutdown.Both);
            sListner.Close();

            ThreadLisner.Abort();
            
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if (handler != null && handler.Connected)
            {
                handler.Send(Encoding.ASCII.GetBytes(TextBoxSendText.Text + "<End>"));

                TextRange rangeOfText1 = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
                rangeOfText1.Text = "-Server-: ";
                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Chocolate);
                rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                rangeOfText1 = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
                rangeOfText1.Text = TextBoxSendText.Text+"\n";
                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                RichTextBox.ScrollToEnd();

                TextBoxSendText.Text = string.Empty;

            }
        }



    }
}
