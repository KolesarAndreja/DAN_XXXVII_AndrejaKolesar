using System;
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
        //fileName is also and locker in some parts
        private readonly string fileName = "PotentialRoutes.txt";
        //list of best routes
        public List<int> bestRoutes = new List<int>();
        public SemaphoreSlim semaphore = new SemaphoreSlim(2);
        //array of threads. Every thread is representing one truck
        public Thread[] trucks = new Thread[10];
        public int count = 0;
        public int semaphoreEnter;
        public int semaphoreExit;




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

            Console.WriteLine("\nTRACKS LOADING: ");
            for (int i = 0; i < 10; i++)
            {
                int br = bestRoutes[i];
                trucks[i] = new Thread(TruckWork)
                {
                    Name = String.Format("Truck_{0}", i + 1)
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
            Console.Write("\nMANAGER: \nBest routes are selected and drivers can start with loading.\nList of selected routes: ");
            bestRoutes = bestRoutes.Distinct().ToList();
            bestRoutes.Sort();
            //displaying best routes
            for (int i = 0; i < 10; i++)
            {
                Console.Write(bestRoutes[i] + " ");
            }
            Console.WriteLine();
        }


        public void TwoByTwo()
        {
            while (true)
            {
                lock (fileName)
                {
                    semaphoreEnter++;
                    if (semaphoreEnter > 2)
                    {
                        Thread.Sleep(0);
                    }
                    else
                    {
                        semaphoreExit++;
                        break;
                    }
                }

            }
        }


        public void TruckWork(object route)
        {
            var name = Thread.CurrentThread.Name;
            TwoByTwo();
            semaphore.Wait();
            Console.WriteLine("{0} has started loading", name);
            int loadingTime = random.Next(500, 5001);
            Thread.Sleep(loadingTime);
            Console.WriteLine("{0} has finished loading", name);
            semaphore.Release();
            semaphoreExit--;
            if (semaphoreExit == 0)
            {
                semaphoreEnter = 0;
            }

            //finish with loading trucks, after that start with route assignment
            lock (fileName)
            {
                count++;
                if (count == 10)
                {
                    Console.WriteLine("\nROUTES AND TRUCKS SCHEDULE:");
                }
            }
            while (count != 10)
            {
                Thread.Sleep(0);
            }
            Console.WriteLine("{0} will drive through route {1}", name, route);

            //finish with route assignment and then start with driving
            lock (fileName)
            {
                count--;
                if (count == 0)
                {
                    Console.WriteLine("\nTRACK DELIVERY:");
                }
            }
            while (count != 0)
            {
                Thread.Sleep(0);
            }

            //truck delivery
            Console.WriteLine("The driver on a truck {0} has just started driving on route {1}. They can expect delivery between 500ms and 5sec.", name, route);
            int deliveryTime = random.Next(500, 5001);


            if (deliveryTime > 3000)
            {
                Thread.Sleep(3000);
                Console.WriteLine("ORDER ON ROUTE {0} CANCELED. {1} did not arrived in 3sec. Truck need 3s to come back to starting point.", route, name, deliveryTime);
            }
            else
            {
                Thread.Sleep(deliveryTime);
                Console.WriteLine("The {0} arrived at its destination {1}. Unloading time was {2}ms.", name, route, Convert.ToInt32(loadingTime / 1.5));
            }

        }
    }
}



