﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAN_XXXVII
{
    class Program
    {
        static void Main(string[] args)
        {
            TruckShipmentSimulator simulator = new TruckShipmentSimulator();
            simulator.DoShipment();
            Console.ReadLine();
        }
    }
}
