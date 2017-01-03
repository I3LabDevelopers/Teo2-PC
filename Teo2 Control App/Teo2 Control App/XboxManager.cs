using System;
using System.Linq;
using BrandonPotter.XBox;
using System.Threading;

namespace Prototype1Controller
{
    class XboxManager
    {

        XBoxControllerWatcher xbcw;
        XBoxController xbc;

        Thread input_handler;
        
        public event EventHandler XBInputReceived;
        Buffer input_buffer;

        bool diagonal_mapping;          // Enable/Disable mapping of diagonal movement (see MapAnalog())


        public XboxManager(Buffer in_buff)
        {
            diagonal_mapping = Convert.ToBoolean(Program.config["diagonal_mapping"]);
            input_buffer = in_buff;
        }
        
        
        // Associating event to its handler.
        public void InitXBM(Parser p, XBoxControllerWatcher xb_cw)
        {
            XBInputReceived += p.XBInputReceivedHandler;
            OnXBInputReceived(EventArgs.Empty);
            
            xbcw = xb_cw;
            xbcw.ControllerConnected += OnControllerConnected;
            xbcw.ControllerDisconnected += OnControllerDisconnected;

            input_handler = new Thread(XBInputHandler);
            input_handler.IsBackground = true;

        }

        protected virtual void OnXBInputReceived(EventArgs e)
        {
            XBInputReceived?.Invoke(this, e);                   // Delegate call (check reference on events).
        }

        
        private void OnControllerConnected(XBoxController controller)
        {
            Console.WriteLine("XBM: Controller Connected, Player " + controller.PlayerIndex.ToString());
            
            var connectedControllers = XBoxController.GetConnectedControllers();
            xbc = connectedControllers.FirstOrDefault();

            input_handler.Start();
        }
        
        private void OnControllerDisconnected(XBoxController controller)
        {
            Console.WriteLine("XBM: controller disconnected, stopping Teo2...");
            input_buffer.Push("M 0 0 0");
            OnXBInputReceived(EventArgs.Empty);
        }
        private void XBInputHandler()
        {
            bool last_A = false, last_B = false, last_X = false, last_Y = false,
                 last_UArrow = false, last_LArrow = false, last_DArrow = false, last_RArrow = false,
                 last_LB = false, last_LT = false, last_RB = false, last_RT = false,
                 last_BK = false, last_ST = false;

            double last_LS_x = 50, last_LS_y = 50, last_RS_x = 50; //, last_RS_y = 50; RS_Y unused.

            while (true)
            {
                if (xbc.IsConnected)
                {
                    double curr_LS_x = xbc.ThumbLeftX, curr_LS_y = xbc.ThumbLeftY,
                        curr_RS_x = xbc.ThumbRightX, curr_RS_y = xbc.ThumbRightY;

                    if (xbc.ButtonUpPressed && !last_UArrow)
                    {
                        input_buffer.Push("UP");
                        OnXBInputReceived(EventArgs.Empty);
                        
                    }
                    last_UArrow = xbc.ButtonUpPressed;

                    if (xbc.ButtonDownPressed && !last_DArrow)
                    {
                        input_buffer.Push("DOWN");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_DArrow = xbc.ButtonDownPressed;

                    if (xbc.ButtonLeftPressed && !last_LArrow)
                    {
                        input_buffer.Push("LEFT");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_LArrow = xbc.ButtonLeftPressed;

                    if (xbc.ButtonRightPressed && !last_RArrow)
                    {
                        input_buffer.Push("RIGHT");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_RArrow = xbc.ButtonRightPressed;

                    if (xbc.ButtonShoulderRightPressed && !last_RB)
                    {
                        input_buffer.Push("RB");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_RB = xbc.ButtonShoulderRightPressed;

                    if (xbc.ButtonShoulderLeftPressed && !last_LB)
                    {
                        input_buffer.Push("LB");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_LB = xbc.ButtonShoulderLeftPressed;

                    if (xbc.TriggerRightPressed && !last_RT)
                    {
                        input_buffer.Push("RT");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_RT = xbc.TriggerRightPressed;

                    if (xbc.TriggerLeftPressed && !last_LT)
                    {
                        input_buffer.Push("LT");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_LT = xbc.TriggerLeftPressed;

                    if (xbc.ButtonAPressed && !last_A)
                    {
                        input_buffer.Push("A");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_A = xbc.ButtonAPressed;

                    if (xbc.ButtonBPressed && !last_B)
                    {
                        input_buffer.Push("B");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_B = xbc.ButtonBPressed;

                    if (xbc.ButtonXPressed && !last_X)
                    {
                        input_buffer.Push("X");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_X = xbc.ButtonXPressed;

                    if (xbc.ButtonYPressed && !last_Y)
                    {
                        input_buffer.Push("Y");
                        OnXBInputReceived(EventArgs.Empty);

                    }
                    last_Y = xbc.ButtonYPressed;


                    // Listening to Lstick, we map 17 points positioned in two circonference centered in 
                    // the analog value (50,50) (See MapAnalog method for more details).
                    // If there is a viariation of at least 15 (x and y ranges from 0 to 100)
                    // I will update the values, if their mapping is changed (i.e. I moved the stick closer to 
                    // another fixed point) I push the new LS value and the old RS value to the XB input buffer
                    // and I update the last_LS values.
                    if ( Math.Sqrt( Math.Pow( curr_LS_x - last_LS_x, 2 ) + Math.Pow( curr_LS_y - last_LS_y, 2 ) ) > 15 )
                    {
                        MapAnalog(ref curr_LS_x, ref curr_LS_y);
                        if (curr_LS_x != last_LS_x || curr_LS_y != last_LS_y)
                        {
                            input_buffer.Push( "M" + curr_LS_x + " " + curr_LS_y + " " + last_RS_x);
                            OnXBInputReceived(EventArgs.Empty);
                            last_LS_x = curr_LS_x;
                            last_LS_y = curr_LS_y;
                        }
                    }


                    // Listening to Rstick_X, we map 5 points 0 25 50 75 100 (50 being the steady state)
                    // If there is a viariation of at least 15 (x ranges from 0 to 100) I will update
                    // the value, if its mapping is changed (i.e. I moved the stick closer to 
                    // another fixed point) I push the new RS_X value and the old LS value to the XB 
                    // input buffer and I update the last_RS_X value.
                    if ( Math.Abs(last_RS_x - curr_RS_x) > 15 )
                    {
                        curr_RS_x = (int)curr_RS_x / 25 * 25;
                        if ( curr_RS_x != last_RS_x )
                        {
                            input_buffer.Push("M" + last_LS_x + " " + last_LS_y + " " + curr_RS_x);
                            OnXBInputReceived(EventArgs.Empty);
                            last_RS_x = curr_RS_x;
                        }
                    }
                }
            }
        }


        // Given x and y values of the stick position this method maps it to the closest point.
        // 17 points distributed over two circumferences of radius 25 centered in the point (50,50)
        // are considered in this implementation (center, 8 in the ases, 8 in the bisectors).
        // The 8 mapping in the bisector can beswitched on and of at Xboxmanager creation by
        // setting the member diagonal_mapping.

        private void MapAnalog(ref double x, ref double y)
        {
            double[] distance = new double[17];
            for (int i = 0; i < 17; ++i)
                distance[i] = 1000;


            distance[0] = Math.Sqrt(Math.Pow(x - 0, 2) + Math.Pow(y - 50, 2));
            distance[1] = Math.Sqrt(Math.Pow(x - 25, 2) + Math.Pow(y - 50, 2));
            distance[2] = Math.Sqrt(Math.Pow(x - 50, 2) + Math.Pow(y - 50, 2));
            distance[3] = Math.Sqrt(Math.Pow(x - 75, 2) + Math.Pow(y - 50, 2));
            distance[4] = Math.Sqrt(Math.Pow(x - 100, 2) + Math.Pow(y - 50, 2));
            distance[5] = Math.Sqrt(Math.Pow(x - 50, 2) + Math.Pow(y - 100, 2));
            distance[6] = Math.Sqrt(Math.Pow(x - 50, 2) + Math.Pow(y - 75, 2));
            distance[7] = Math.Sqrt(Math.Pow(x - 50, 2) + Math.Pow(y - 25, 2));
            distance[8] = Math.Sqrt(Math.Pow(x - 50, 2) + Math.Pow(y - 0, 2));


            if (diagonal_mapping)
            {
                distance[9] = Math.Sqrt(Math.Pow(x - 15, 2) + Math.Pow(y - 85, 2));
                distance[10] = Math.Sqrt(Math.Pow(x - 33, 2) + Math.Pow(y - 67, 2));
                distance[11] = Math.Sqrt(Math.Pow(x - 67, 2) + Math.Pow(y - 33, 2));
                distance[12] = Math.Sqrt(Math.Pow(x - 85, 2) + Math.Pow(y - 15, 2));
                distance[13] = Math.Sqrt(Math.Pow(x - 15, 2) + Math.Pow(y - 15, 2));
                distance[14] = Math.Sqrt(Math.Pow(x - 33, 2) + Math.Pow(y - 33, 2));
                distance[15] = Math.Sqrt(Math.Pow(x - 67, 2) + Math.Pow(y - 67, 2));
                distance[16] = Math.Sqrt(Math.Pow(x - 85, 2) + Math.Pow(y - 85, 2));
            }

            int min = 0;
            for (int i = 1; i < 17; ++i)
            {
                if (distance[i] < distance[min])
                    min = i;
            }

            switch (min)
            {
                case 0:
                    x = 0;
                    y = 50;
                    break;
                case 1:
                    x = 25;
                    y = 50;
                    break;
                case 2:
                    x = 50;
                    y = 50;
                    break;
                case 3:
                    x = 75;
                    y = 50;
                    break;
                case 4:
                    x = 100;
                    y = 50;
                    break;
                case 5:
                    x = 50;
                    y = 100;
                    break;
                case 6:
                    x = 50;
                    y = 75;
                    break;
                case 7:
                    x = 50;
                    y = 25;
                    break;
                case 8:
                    x = 50;
                    y = 0;
                    break;
                case 9:
                    x = 15;
                    y = 85;
                    break;
                case 10:
                    x = 33;
                    y = 67;
                    break;
                case 11:
                    x = 67;
                    y = 33;
                    break;
                case 12:
                    x = 85;
                    y = 15;
                    break;
                case 13:
                    x = 15;
                    y = 15;
                    break;
                case 14:
                    x = 33;
                    y = 33;
                    break;
                case 15:
                    x = 67;
                    y = 67;
                    break;
                case 16:
                    x = 85;
                    y = 85;
                    break;
                default:
                    break;
            }
        }
    }
}
