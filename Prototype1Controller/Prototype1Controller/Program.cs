using BrandonPotter.XBox;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Prototype1Controller
{   
    class Program
    {
        static void Main(string[] args)
        {
            
             Console.WriteLine("Connected ports");                    // Shows the connected ports, just in case...
             string[] ports = SerialPort.GetPortNames();
             for (int i = 0; i < ports.Length; ++i)
             {
                 System.Console.WriteLine(ports[i]);
             }


             Buffer BTinput = new Buffer();
             Buffer XBinput = new Buffer();
             Buffer output = new Buffer();

             //SerialPort BTTESTport = new SerialPort("COM2", 9600);
             //BTTESTport.Open();
             Console.WriteLine("Insert the Bluetooth port name");
             string port = Console.ReadLine();
             SerialPort BTport = new SerialPort(port, 9600);
             BluetoothManager btmg = new BluetoothManager(BTport, BTinput, output);     // Attaching input and output ports to new Bluetooth manager.
             XboxManager xbmg = new XboxManager(XBinput, true);
             Parser parser = new Parser(BTinput, XBinput, output);

             btmg.InitBTM(parser);
             parser.InitParser(btmg);

             XBoxControllerWatcher xbcw = new XBoxControllerWatcher();

             xbmg.InitXBM(parser, xbcw);


             Console.WriteLine("Press any key to continue...");

             while (Console.ReadLine() != "stop" )                           
             {
                 //Console.WriteLine("Teo received this (TEST): " + BTTESTport.ReadLine());

                 Console.WriteLine("Insert the message to send");
                 string command = Console.ReadLine();

                 btmg.SerialWrite(command);                      // Writing the command to the ouput port.
                 //BTTESTport.WriteLine(command);

                 Console.WriteLine("Press any key to continue... (stop to exit)");
             }
             //BTTESTport.Close();
            


        }

    }
}



