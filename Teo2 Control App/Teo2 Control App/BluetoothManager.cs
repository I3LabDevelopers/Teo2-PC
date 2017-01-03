using System;
using System.IO.Ports;
using System.Threading;

namespace Prototype1Controller
{
    public class BluetoothManager
    {
        SerialPort BTPort;
        Buffer input_buffer;
        Buffer output_buffer;
        public event EventHandler BTInputReceived;
        Thread connection_handler;
        bool connected;
        int last_received;


        public BluetoothManager(Buffer in_buff, Buffer out_buff)                
        {
            input_buffer = in_buff;
            output_buffer = out_buff;
        }


        // Open the BTport and associate each event to its handler. It tries to connect to each port on the list
        // if atcive port, if no ports are available it launches an exception.
        public void InitBTM(Parser p)
        {






            string[] ports = SerialPort.GetPortNames();
            connected = false;
            for (int i = 0; i < ports.Length && !connected; ++i)
            {
                connected = Connect(ports[i]);
            }
            if (!connected)
            {
                throw new ArgumentException("Cannot connect to Teo2, retry later");
            }

            last_received = DateTime.Now.Second;

            BTPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            BTInputReceived += p.BTInputReceivedHandler;

            connection_handler = new Thread(ConnectionHandler);
            connection_handler.IsBackground = true;
            connection_handler.Start();
        }

        // On delete I close the port, you never know...
        ~BluetoothManager()                         
        {
            BTPort.Close();
        }

        // Returns the input reveived.
        public string SerialRead()                  
        {
            string command = "BTM_internal_error";
            if (BTPort.BytesToRead > 0)
            {
                try
                {
                    command = BTPort.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("BTM Serial Read error: " + ex.Message);
                }
            }
            return command;
        }

        // Send message to the bluetooth output buffer.
        public void SerialWrite(string command)    
        {
            if (connected)
            {
                try
                {
                    command = command + "\r";               // Teo2 needs the carriage return.
                    BTPort.WriteLine(command);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("BTM Serial Write error: " + ex.Message);
                }
            }
        }

        // Send the parsed command to Teo.
        public void CommandParsedHandler (object sender, EventArgs e)
        {
            string command = output_buffer.Pop();
            Console.WriteLine("Sending message: " + command);
            SerialWrite(command);
        }

        // Push the message received from BT input buffer to input buffer.
        public void DataReceivedHandler (object sender, SerialDataReceivedEventArgs e)
        {
            string command = SerialRead();
            if (command != "BTM_internal_error")
            {
                Console.WriteLine("Recieved message: " + command);
                input_buffer.Push(command);
                OnBTInputReceived(EventArgs.Empty);
            }

            // Update last_received value everytime a message is received        
            last_received = DateTime.Now.Second;                    
        }

        protected virtual void OnBTInputReceived(EventArgs e)
        {
            BTInputReceived?.Invoke(this, e);                   // Delegate call (check reference on events).
        }

        // Open the port, if it doesn't succeed it report error (true), if it opens it check if Teo2
        // is sending the start command, if not close the port and report error (true).
        private bool Connect(string port_name)
        {
            Console.WriteLine("Trying port: " + port_name);
            BTPort = new SerialPort(port_name, 9600);
            try
            {
                BTPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("BTM Connect: " + ex.Message);
                return false;
            }
            BTPort.DiscardInBuffer();                           // Avoid to read old values

            /*
            SerialPort TEST;                                // Test w/o Teo
            TEST = new SerialPort("COM2", 9600);
            TEST.Open();
            TEST.WriteLine("ready");
            */

            Thread.Sleep(int.Parse(Program.config["reconnection_time"]) * 1000);                // Give Teo2 some time to send start message

            //TEST.Close();

            if (SerialRead() != Program.config["ready_message"])
            {
                BTPort.Close();
                Console.WriteLine("...Teo2 is not sending messages...");
                return false;
            }

            last_received = DateTime.Now.Second;
            BTPort.DiscardInBuffer();                             // Avoid to read the start message again later
            Console.WriteLine("Connected to port: " + port_name);    
            return true;
        }

        
        // Constantly check reconnection_time is passed from the last message received from Teo2, if so 
        // try to reconnect to the same port it was connected before.
        private void ConnectionHandler()
        {
            while (true)
            {
                if (Math.Abs(DateTime.Now.Second - last_received) > int.Parse(Program.config["reconnection_time"]))
                {
                    Console.WriteLine("BTM: Lost connection with Teo2");
                    connected = false;

                    BTPort.Close();
                    connected = Connect(BTPort.PortName);
                    if (!connected)
                    {
                        throw new ArgumentException("BTM: Cannot connect to Teo2, retry later");
                    }
                }
            }
        }
    }
}
