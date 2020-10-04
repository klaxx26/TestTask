using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace WSCS
{
    class Program
    {

        public static SServ sockServ;

        static void Main()
        {

            try {

                sockServ = new SServ("127.0.0.1", 8000);

                Thread receiver = new Thread(new ThreadStart(Receiver.Receive));
                receiver.Start();
 
            } catch (Exception e){
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();

        }

    }
}
