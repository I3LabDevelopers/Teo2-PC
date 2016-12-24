using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
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



        public XboxManager(Buffer in_buff, bool LStick_diag_mapping)
        {
            diagonal_mapping = LStick_diag_mapping;
            input_buffer = in_buff;
        }
        
        
        // Associating event to its handler.
        public void InitXBM(Parser p, XBoxControllerWatcher xb_cw)
        {
            XBInputReceived += p.XBInputReceivedHandler;
            input_buffer.Push("prova XBmanager");
            OnXBInputReceived(EventArgs.Empty);
            
            xbcw = xb_cw;
            xbcw.ControllerConnected += OnControllerConnected;
            // xbcw.ControllerDisconnected += OnControllerDisconnected;
        }

        protected virtual void OnXBInputReceived(EventArgs e)
        {
            XBInputReceived?.Invoke(this, e);                   // Delegate call (check reference on events).
        }




        private void OnControllerConnected(XBoxController controller)
        {
            Console.WriteLine("Controller Connected: Player " + controller.PlayerIndex.ToString());

            var connectedControllers = XBoxController.GetConnectedControllers();
            xbc = connectedControllers.FirstOrDefault();

            input_handler = new Thread(XBInputHandler);
            input_handler.IsBackground = true;
            input_handler.Start();
        }

        private void XBInputHandler()
        {
            bool last_A = false  , last_B = false, last_X = false, last_Y = false, last_UA = false, 
                last_LArrow = false, last_DArrow = false, last_RArrow = false, last_LB = false,
                last_LT = false, last_RB = false, last_RT = false, last_BK = false, last_ST = false;

            double last_LS_x = 50, last_LS_y = 50, last_RS_x = 50; //, last_RS_y = 50; RS_Y unused.

            while (true)
            {
                if (xbc.IsConnected)
                {
                    double curr_LS_x = xbc.ThumbLeftX, curr_LS_y = xbc.ThumbLeftY,
                        curr_RS_x = xbc.ThumbRightX, curr_RS_y = xbc.ThumbRightY;


                    if (xbc.ButtonAPressed && !last_A)
                    {
                        input_buffer.Push("A");
                        OnXBInputReceived(EventArgs.Empty);

                    }


                    // Listening to Lstick, we map 17 points positioned in two circonference centered in 
                    // the analog value (50,50) (See MapAnalog method for more details).
                    // If there is a viariation of at least 15 (x and y ranges from 0 to 100)
                    // I will update the values, if their mapping is changed (i.e. I moved the stick closer to 
                    // another fixed point) I push the new LS value and the old RS value to the XB input buffer
                    // and I update the last_LS values.
                    if( Math.Sqrt( Math.Pow( curr_LS_x - last_LS_x, 2 ) + Math.Pow( curr_LS_y - last_LS_y, 2 ) ) > 15 )
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
                    last_A = xbc.ButtonAPressed;
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
