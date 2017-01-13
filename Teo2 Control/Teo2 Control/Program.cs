using BrandonPotter.XBox;
using System;
using System.Media;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Prototype1Controller
{
    class Program
    {
        public static SoundPlayer sound_player;
        public static Dictionary<string, string> config;
        public static Dictionary<string, string> preset;

        static void Main(string[] args)
        {
            sound_player = new System.Media.SoundPlayer();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GenericHandler);
                        
            config = new Dictionary<string, string>();
            try
            {
                config = ConvertFromXML("XML/config.xml");
            }
            catch (Exception ex)
            {
                Console.WriteLine("XML reading exception: " + ex.Message);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Connected ports");                    // Shows the connected ports, just in case...
            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; ++i)
            {
                System.Console.WriteLine(ports[i]);
            }
            
            Buffer BTinput = new Buffer();
            Buffer XBinput = new Buffer();
            Buffer output = new Buffer();

            BluetoothManager btmg = new BluetoothManager(BTinput, output);  
            
            Parser parser = new Parser(BTinput, XBinput, output);

            preset = new Dictionary<string, string>();
            try
            {
                preset = ConvertFromXML("XML/" + config["selected_preset"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("XML reading exception: " + ex.Message);
                Console.ReadKey();
                return;
            }
            
            parser.InitParser(btmg, preset);

            try
            {
                btmg.InitBTM(parser);
            }
            catch (Exception ex)
            {
                Console.WriteLine("BTM error: " + ex.Message);
                Console.ReadKey();
                return;
            }
            
            XboxManager xbmg = new XboxManager(XBinput);

            XBoxControllerWatcher xbcw = new XBoxControllerWatcher();
            xbmg.InitXBM(parser, xbcw);


            Console.WriteLine("Setup complete");

            while (true) ;
            
            //Console.WriteLine("Press any key to continue...");
            
            /*
            while (Console.ReadLine() != "stop" )                           
            {

                Console.WriteLine("Insert the message to send");
                string command = Console.ReadLine();

                btmg.SerialWrite(command);                      // Writing the command to the ouput port.
                

                Console.WriteLine("Press any key to continue... (stop to exit)");

            }*/
        }
        
        private static Dictionary<string, string> ConvertFromXML(string file_name)
        {
            XElement rootElement;
            try
            {
                rootElement = XElement.Load(file_name);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var el in rootElement.Elements())
            {
                dict.Add(el.Name.LocalName, el.Value);
            }

            return dict;
        }

        static void GenericHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("Unhandled Exception caught: " + e.Message);
            Console.ReadKey();
            Application.Exit();
        }

    }
}



