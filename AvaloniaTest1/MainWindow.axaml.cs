using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Net;
using System.Reactive.Disposables;
using System.IO;
using System.IO.Ports;
using Avalonia.Threading;
using System;
using Modbus.Device;
using Modbus.Utility;
using Modbus.IO;
using Modbus.Data;
using Avalonia.Animation;
using Tmds.DBus.Protocol;
using ReactiveUI;
using System.Windows;
using Avalonia.Dialogs;
using static System.Net.Mime.MediaTypeNames;
using MsBox.Avalonia;
using AvaloniaTest1.Service;
using System.Text;

namespace AvaloniaTest1
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer=new DispatcherTimer();
        private SerialPort port;
        private ModbusSerialMaster serialMaster;
        private ushort[] data;


        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += TimerSec_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            poll.IsEnabled = false;
        }


        private async void TimerSec_Tick(object sender, EventArgs e)
        {
            if (serialMaster != null)
            {
                try
                {                    
                    data= await serialMaster.ReadHoldingRegistersAsync(1, 0, 20);                    
                }
                catch(Exception ex)
                {
                    ClassMessage.ShowMessage(ex.Message);
                }
                listData.Items.Add(ConvertUshortDataToString(data));
            }
        }

        private StringBuilder ConvertUshortDataToString(ushort [] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var elem in data)
            {
                sb.Append(elem.ToString() + " ");
            }
            return sb;
        }

        private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name == "connect")
            {
                string portName = this.selectPort.SelectedItem.ToString();
                PortOpen(portName);
            }
            if (button.Name == "poll")
            {
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                    poll.IsEnabled = false;
                    ClassMessage.ShowMessage("Таймер остановлен.");
                }
            }
        }

        private static string[] GetAllPorts()
        {
            return SerialPort.GetPortNames();
        }

        private void PortOpen(string portName)
        {
            if(port!=null && port.IsOpen)
            {
                serialMaster = null;
                port.Close();
                port = null;                
            }
            port = new SerialPort(portName)
            {
                BaudRate = 9600,
                Parity=Parity.None,
                DataBits=8,
                WriteTimeout = 1000,
                ReadTimeout = 1000                
            };
            try
            {
                port.Open();
                serialMaster = ModbusSerialMaster.CreateRtu(port);
                poll.IsEnabled = true;
                ClassMessage.ShowMessage("Порт открыт!");
            }
            catch(Exception ex)
            {
                ClassMessage.ShowMessage(ex.Message);
                return;
            }
        }

        private void Window_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string [] arr= GetAllPorts();
            Array.Sort(arr);
            this.selectPort.ItemsSource = arr;
            this.selectPort.SelectedIndex = 0;           
        }
    }
}