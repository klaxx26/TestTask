using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;



namespace WSCS
{

    public class Receiver
    {

        public static void Receive()
        {
            try {

                var factory = new ConnectionFactory() { 
                    HostName = "localhost"
                 };
                using(var connection = factory.CreateConnection())
                using(var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    Console.WriteLine("Waiting for messages from Rabbit");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        try {

                            User user = JsonConvert.DeserializeObject<User>(message);  
                            Program.sockServ.SendMsg(user);
                        } finally {

                        }

                        Console.WriteLine("Received from Rabbit: {0}", message);
                    };
                    channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

                    Console.ReadLine();
                } 
            } catch (Exception e){
                Console.WriteLine(e.Message);
            }
        }

    }


}