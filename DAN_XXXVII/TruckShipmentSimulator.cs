using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAN_XXXVII
{
    class TruckShipmentSimulator
    {
        public Random random = new Random();
        private readonly string fileName = "PotentialRoutes.txt";
        public List<int> bestRoutes = new List<int>();

        public void DoShipment()
        {
            Thread t1 = new Thread(NumberGenerator)
            {
                Name = "number_generator"
            };
            Thread t2 = new Thread(ManagerJob)
            {
                Name = "manager"
            };
            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();
        }

        /// <summary>
        /// Generates 1000 numbers in range [1,5000] and logs them into file PotentialRoutes.txt
        /// </summary>
        public void NumberGenerator()
        {
            int n;
            lock (fileName)
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    for (int i = 0; i < 5000; i++)
                    {
                        n = random.Next(1, 5001);
                        sw.WriteLine(n);
                    }
                }
                Monitor.Pulse(fileName);
            }
            
        }

        public void ManagerJob()
        {
            lock (fileName)
            {
                //while (!File.Exists(fileName))
                //{
                    Monitor.Wait(fileName, 2);
                //}
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        int n = Convert.ToInt32(s);
                        if (n % 3 == 0)
                        {
                            bestRoutes.Add(n);      
                        }
                    }
                }
            }
            Console.WriteLine("Best routes are: ");
            bestRoutes.Sort();
            for(int i=0; i<10; i++)
            {
                Console.WriteLine(bestRoutes[i] + " ");
            }
        }
    }
}
