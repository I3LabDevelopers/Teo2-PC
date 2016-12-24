using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype1Controller
{
    // This class will manage the input commands both from Bluetooth and Xbox controller.
    // After being signaled of an input received Parser will parse the command and decide which 
    // decision to take and which command to send to the BT manager.
    public class Parser
    {        
        Buffer bt_input_buffer;
        Buffer xb_input_buffer;
        Buffer output_buffer;
        public event EventHandler CommandParsed;
        public Parser(Buffer bt_in_buff, Buffer xb_in_buff, Buffer out_buff)
        {
            bt_input_buffer = bt_in_buff;
            xb_input_buffer = xb_in_buff;
            output_buffer = out_buff;
        }

        public void InitParser(BluetoothManager btm)
        {
            CommandParsed += btm.CommandParsedHandler;                      // Associating CommanParsed event to its Handler.
        }

        // Extracts the command from the XB input buffer and call the XB parser.
        public void XBInputReceivedHandler(object sender, EventArgs e)
        {
            string command = xb_input_buffer.Pop();
            ParseXB(command);
        }

        // Extracts the command from the BT input buffer and call the BT parser.
        public void BTInputReceivedHandler(object sender, EventArgs e)
        {
            string command = bt_input_buffer.Pop();
            ParseBT(command);            
        }

        // Parses a command redirecting it to the adequate method.
        private void ParseBT(string command)
        {/*
            switch (command)
            {
                case "non-echo":
                    break;
                default:
                    EchoTest(command);
                    break;
            }
            */
        }

        private void ParseXB(string command)
        {
            if( command == "A")
            {
                output_buffer.Push("move 0 -1 0");
                OnCommandParsed(EventArgs.Empty);
            }

            // Parse the input received from the xbox LStick the x-axis value refers to the strage speed
            // the y-asix value to the forward speed (note that a value of -1 equals to moving forward)
            if (command.StartsWith("M"))
            {

                command = command.Split('M')[1];
                string[] comm_args = command.Split(' ');

                double x = ParseMotorSpeed(comm_args[0], 1.4, 1);           // Strafe parsing
                double y = ParseMotorSpeed(comm_args[1], -2, -1);           // Forward parsing
                double z = ParseMotorSpeed(comm_args[2], -4, -3);           // Angular parsing

                output_buffer.Push("move " + x + " " + y + " " + z);
                OnCommandParsed(EventArgs.Empty);

            }
            //XBTest(command);

        }

        private string EchoTest(string command)
        {
            command = "parsed command: " + command;
            output_buffer.Push(command);
            OnCommandParsed(EventArgs.Empty);
            return command;
        }

        protected virtual void OnCommandParsed(EventArgs e)
        {
            CommandParsed?.Invoke(this, e);                     // Delegate call (check reference on events).
        }
        
        // Associates to each stick value a corrispondent value in speed, currently supporting 2 
        // speed values, fast and slow. These need to be set accordingly to the different movement
        // functionality (forward, strafe, angular).
        private double ParseMotorSpeed(string stick_value, double fast, double slow)
        {
            double speed;
            switch (stick_value)
            {
                case "100":
                    speed = fast;
                    break;

                case "75":
                    speed = slow;
                    break;

                case "50":
                    speed = 0;
                    break;

                case "25":
                    speed = -slow;
                    break;

                case "0":
                    speed = -fast;
                    break;

                case "85":
                    speed = fast;
                    break;

                case "67":
                    speed = slow;
                    break;

                case "33":
                    speed = -slow;
                    break;

                case "15":
                    speed = -fast;
                    break;
                default:
                    speed = 0;
                    break;
            }
            return speed;
        }
    }
}
