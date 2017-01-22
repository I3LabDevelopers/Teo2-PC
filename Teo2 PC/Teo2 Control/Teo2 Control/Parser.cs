using System;
using System.Collections.Generic;

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
        Dictionary<string, string> parsing_map;
        bool automatic_behaviours = true;

        // Some actions require to stop the output of certain commands releted to movement. A timeout 
        // is set to cover the case of unblock message from Teo2 lost.

        bool motion_block = false;                                  
        int motion_block_timer = 0;
        int motion_block_timeout = int.Parse(Program.config["motion_block_timeout"]);

        public Parser(Buffer bt_in_buff, Buffer xb_in_buff, Buffer out_buff)
        {
            bt_input_buffer = bt_in_buff;
            xb_input_buffer = xb_in_buff;
            output_buffer = out_buff;
        }

        public void InitParser(BluetoothManager btm, Dictionary<string, string> preset)
        {
            CommandParsed += btm.CommandParsedHandler;                      // Associating CommanParsed event to its Handler.
            parsing_map = preset;
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
        {
            if ( command == Program.config["end_motion"])
                motion_block = false;
            
            if (Program.commands.ContainsKey(command))
            {
                Program.sound_player.SoundLocation = Program.commands[command];
                Program.sound_player.PlaySync();
                output_buffer.Push(Program.config["end_of_sound_message"]);
                OnCommandParsed(EventArgs.Empty);
            }
        }

        private void ParseXB(string command)
        {
            // Release the block if the time is out.
            if (Math.Abs(DateTime.Now.Second - motion_block_timer) >= motion_block_timeout)
                motion_block = false;
            
            if (!motion_block)
            {
                switch (command)
                {
                    case "A":
                        SetMood("scared");
                        Program.sound_player.SoundLocation = Program.config["scared"];
                        Program.sound_player.PlaySync();
                        break;

                    case "B":
                        SetMood("angry");
                        Program.sound_player.SoundLocation = Program.config["angry"];
                        Program.sound_player.PlaySync();
                        break;

                    case "X":
                        SetMood("sad");
                        Program.sound_player.SoundLocation = Program.config["sad"];
                        Program.sound_player.PlaySync();
                        break;

                    case "Y":
                        SetMood("happy");
                        Program.sound_player.SoundLocation = Program.config["happy"];
                        Program.sound_player.PlaySync();
                        break;

                    case "UP":
                        Program.sound_player.SoundLocation = Program.preset["up"];
                        Program.sound_player.PlaySync();
                        break;

                    case "DOWN":
                        Program.sound_player.SoundLocation = Program.preset["down"];
                        Program.sound_player.PlaySync();
                        break;

                    case "LEFT":
                        Program.sound_player.SoundLocation = Program.preset["left"];
                        Program.sound_player.PlaySync();
                        break;

                    case "RIGHT":
                        Program.sound_player.SoundLocation = Program.preset["right"];
                        Program.sound_player.PlaySync();
                        break;

                    case "RB":
                        Program.sound_player.SoundLocation = Program.preset["r_shoulder"];
                        Program.sound_player.PlaySync();
                        break;

                    case "LB":
                        Program.sound_player.SoundLocation = Program.preset["l_shoulder"];
                        Program.sound_player.PlaySync();
                        break;

                    case "RT":
                        Program.sound_player.SoundLocation = Program.preset["r_trigger"];
                        Program.sound_player.PlaySync();
                        break;

                    case "LT":
                        Program.sound_player.SoundLocation = Program.preset["l_trigger"];
                        Program.sound_player.PlaySync();
                        break;

                    case "BACK":
                        output_buffer.Push(Program.config["idle_message"]);
                        OnCommandParsed(EventArgs.Empty);
                        break;

                    case "START":
                        if (automatic_behaviours)
                        {
                            automatic_behaviours = false;
                            output_buffer.Push(Program.config["auto_off_message"]);
                        }
                        else
                        {
                            automatic_behaviours = true;
                            output_buffer.Push(Program.config["auto_on_message"]);
                        }
                        OnCommandParsed(EventArgs.Empty);
                        break;

                    default:
                        break;
                }

                if (command.StartsWith("M"))
                    SetMotorsSpeed(command);
            }
        }

        protected virtual void OnCommandParsed(EventArgs e)
        {
            CommandParsed?.Invoke(this, e);                     // Delegate call (check reference on events).
        }
        
        private void SetMood(string mood)
        {
            output_buffer.Push(mood);
            OnCommandParsed(EventArgs.Empty);
            motion_block = true;
            motion_block_timer = DateTime.Now.Second;
        }

        // Parse the input received from the xbox LStick the x-axis value refers to the strage speed
        // the y-asix value to the forward speed (note that a value of -1 equals to moving forward)
        private void SetMotorsSpeed(string command)
        {
            command = command.Split('M')[1];
            string[] comm_args = command.Split(' ');

            double x = ParseMotorSpeed(comm_args[0], 1.4, 1);           // Strafe parsing
            double y = ParseMotorSpeed(comm_args[1], -2, -1);           // Forward parsing
            double z = ParseMotorSpeed(comm_args[2], -4, -3);           // Angular parsing

            output_buffer.Push("move " + x + " " + y + " " + z);
            OnCommandParsed(EventArgs.Empty);
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


        /*
        private string EchoTest(string command)
        {
            command = "parsed command: " + command;
            output_buffer.Push(command);
            OnCommandParsed(EventArgs.Empty);
            return command;
        }
        */
    }
}
