using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype1Controller
{
    public class BluetoothManager
    {
        SerialPort BTPort;
        Buffer input_buffer;
        Buffer output_buffer;
        public event EventHandler BTInputReceived;


        public BluetoothManager(SerialPort port, Buffer in_buff, Buffer out_buff)                
        {
            input_buffer = in_buff;
            output_buffer = out_buff;
            BTPort = port;
        }


        // Open the BTport and associate each event to its handler.
        public void InitBTM(Parser p)
        {
            BTPort.Open();
            BTPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            BTInputReceived += p.BTInputReceivedHandler;                                
        }

        // On delete I close the port, you never know...
        ~BluetoothManager()                         
        {
            BTPort.Close();
        }

        // Returns the input reveived.
        public string SerialRead()                  
        {
            string command = BTPort.ReadLine();
            return command;
        }

        // Send message to the bluetooth output buffer.
        public void SerialWrite(string command)    
        {
            try
            {
                command = command + "\r";
                /*foreach (char c in command){
                    BTPort.Write(c.ToString());

                }*/
                BTPort.WriteLine(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: " + ex.Message);
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
            System.Console.WriteLine("Recieved message: " + command);
            input_buffer.Push(command);
            OnBTInputReceived(EventArgs.Empty);
        }

        protected virtual void OnBTInputReceived(EventArgs e)
        {
            BTInputReceived?.Invoke(this, e);                   // Delegate call (check reference on events).
        }

    }
}
