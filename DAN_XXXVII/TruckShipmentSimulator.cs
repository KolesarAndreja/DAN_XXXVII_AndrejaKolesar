﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public SemaphoreSlim semaphore = new SemaphoreSlim(2);
        public Thread[] trucks = new Thread[10];
        public int count = 0;


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

            Console.WriteLine("TRACKS LOADING: ");
            for (int i = 0; i < 10; i++)
            {
                int br = bestRoutes[i];
                trucks[i] = new Thread(TruckWork)
                {
                    Name = String.Format("Truck_{0}", i+1)
                };
                trucks[i].Start(bestRoutes[i]);
            }
        
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

        /// <summary>
        /// Manager is choosing the best routes
        /// Best routes are routes that are divisable by 3
        /// </summary>
        public void ManagerJob()
        {
            lock (fileName)
            {
                while (!File.Exists(fileName))
                {
                    Monitor.Wait(fileName, 3000);
                }
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
            Console.WriteLine("All possible routes are shown in the file {0}", fileName);
            Console.Write("MANAGER: \nBest routes are selected and truck drivers can start with truck loading and after that with driving.\nList of selected routes: ");
            bestRoutes = bestRoutes.Distinct().ToList();
            bestRoutes.Sort();
            //displaying best routes
            for(int i=0; i<10; i++)
            {
                Console.Write(bestRoutes[i] + " ");
            }
            Console.WriteLine();
        }

        public void TruckWork(object route)
        {
            var name = Thread.CurrentThread.Name;
            Console.WriteLine("{0} wants do loading ", name);
            semaphore.Wait();
            Console.WriteLine("{0} has started loading", name);
            int loadingTime = random.Next(500, 5001);
            Thread.Sleep(loadingTime);
            Console.WriteLine("{0} has finished loading", name);
            semaphore.Release();

            lock (fileName)
            {
                count++;
                if (count == 10)
                {
                    Console.WriteLine("\nROUTES AND TRUCKS SCHEDULE");
                }
            }
            while (count != 10)
            {
                Thread.Sleep(0);
            }

            Console.WriteLine("{0} will drive through route {1}", name, route);
            Delivery();

        }       


        public void Delivery()
        {
            Stopwatch sw = new Stopwatch();
        }
    }
}



