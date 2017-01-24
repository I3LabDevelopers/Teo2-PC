using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype1Controller
{
    // This class implements a simple buffer, it will be used twice, to manage input and output commands.
    // The buffer has a max size of 100, it can be modified at creation.
    public class Buffer
    {
        List<string> buffer;         
        int max_size = 100;                                       
        readonly object lockingObject = new object();               
        public Buffer(int max_s = 100)
        {
            max_size = max_s;
            buffer = new List<string>();
        }

        // It is necessary to acquire the lock on the buffer because the event-based interactions can cause
        // two methods to access simultaneusly to it, same for the Pop().
        public void Push(string command)                                    
        {
            if (buffer.Count < max_size)
            {
                lock (lockingObject)
                {
                    buffer.Add(command);
                }
            }
        }

        public string Pop()                                     
        {
            string command = "Buffer error: Buffer empty";
            if (buffer.Count > 0)
            {
                lock (lockingObject)
                {
                    command = buffer[buffer.Count - 1];
                    buffer.Remove(command);
                }
            }
            return command;
        }
    }
}
