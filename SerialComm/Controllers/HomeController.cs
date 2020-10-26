using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SerialComm.Models;

namespace SerialComm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static SerialPort _serialPort;
        static bool _continue;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    //Console.WriteLine("reading...");
                    string message = _serialPort.ReadLine();
                    string moduleState = message.Split('/')[0];
                    string moduleAmount = message.Split('/')[1];
                    Console.WriteLine($"read message - {message} : state - {moduleState} / amount - {moduleAmount}");
                    //Console.WriteLine("reading...");
                }
                catch (Exception) 
                {
                    // do nothing.
                }
            }
        }

        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("COM port({0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "")
            {
                portName = defaultPortName;
            }
            return portName;
        }

        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Parity({0}):", defaultPortParity.ToString());
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity);
        }

        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Data Bits({0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits);
        }

        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available Stop Bits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
        }

        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake);
        }

        public IActionResult Index()
        {
            string name;
            string message;

            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            //_serialPort.PortName = SetPortName(_serialPort.PortName);   // /dev/ttyS0
            //_serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);   // 115200
            //_serialPort.Parity = SetPortParity(_serialPort.Parity);         // None
            //_serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);   // 8
            //_serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);   // One
            //_serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);    // None

            // Setting
            _serialPort.PortName = "/dev/ttyS0";   // /dev/ttyS0
            _serialPort.BaudRate = 115200;   // 115200
            _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), "None");         // None
            _serialPort.DataBits = 8;   // 8
            _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");   // One
            _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");    // None

            // Set the read/write timeouts  
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _continue = true;
            readThread.Start();

            //Console.Write("Name: ");
            //name = Console.ReadLine();

            Console.WriteLine("Type QUIT to exit");

            int i = 0;
            int j = 0;
            int[] commands = { 0, 1 };  // 0 preoutput 1 output
            //int[] commands = { 0 };  // ROTARY : 0 output
            //double[] amounts = { 000.0, 024.0, 036.0, 048.0, 057.6 };   // SAUCE
            double[] amounts = { 000.0, 037.5, 056.3, 075.0, 090.0, 112.5 };   // SCREW
            //double[] amounts = { 010.0, 055.0 };    // ROTARY             
            int command;
            double amount;
            string unit;
            while (_continue)
            {
                message = Console.ReadLine();
                //message = "P/111/ml";
                command = commands[i];    // SCREW, SAUCE
                //command = commands[0];  // ROTARY
                amount = amounts[j];
                unit = "ml";    // SAUCE
                //unit = "g"; // ROTARY, SCREW
                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {
                    //_serialPort.WriteLine(String.Format("<{0}>: {1}", name, message));
                }
                string output = String.Format("{0}/{1}/{2}", command, ((int)amount).ToString("D3") + ((amount * 10) % 10).ToString(), unit);
                Console.WriteLine($"Write to STM: {output}");
                _serialPort.WriteLine(output);

                if (i == 1)
                {
                    if (j < amounts.Length - 1)
                    {
                        ++j;
                    }
                    else
                    {
                        j = 0;
                    }                    
                }

                if (i < commands.Length - 1)
                {
                    ++i;
                }
                else
                {
                    i = 0;
                }
                
                // TEST
                //_serialPort.WriteLine(String.Format("2"));
                //Thread.Sleep(10);
                //_serialPort.WriteLine(String.Format("q"));
                //Thread.Sleep(10);
            }

            readThread.Join();
            _serialPort.Close();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
