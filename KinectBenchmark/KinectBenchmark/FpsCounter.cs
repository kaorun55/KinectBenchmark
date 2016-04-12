using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectBenchmark
{
    public class FpsCounter
    {
        public int FPS { get; set; } = 0;
        int count = 0;

        DateTime old = DateTime.Now;

        public int Update()
        {
            count++;

            var current = DateTime.Now;
            if ((current - old).TotalMilliseconds >= 1000)
            {
                old = current;

                FPS = count;
                count = 0;
            }

            return FPS;
        }
    }
}
