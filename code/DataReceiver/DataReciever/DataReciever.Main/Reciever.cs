using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace DataReciever.Main
{

    class Reciever
    {
        public string RabbitHostName { get; }
        public string RabbitUsername { get; }
        public string RabbitPassword { get; }

        public Reciever()
        {
            RabbitHostName = ConfigurationManager.AppSettings["RabbitHostName"];
            RabbitUsername = ConfigurationManager.AppSettings["RabbitUsername"];
            RabbitPassword = ConfigurationManager.AppSettings["RabbitPassword"];

        }
        public void RecieveValues()
        {

            var factory = new ConnectionFactory() { HostName = RabbitHostName, UserName = RabbitUsername, Password = RabbitPassword };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();



                    Console.WriteLine("recieving values");
                    channel.QueueDeclare("mongoose_entity_varchar",
                        true,
                        false,
                        false,
                        null);
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: "mongoose_entity_varchar", autoAck: false, consumer: consumer);


                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var output = JsonConvert.DeserializeObject<sync_value_varchar>(message);

                     

                        Data.StoreValue(output);
                        Console.WriteLine(output.value);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                   
             
                
            
        }
        public void RecieveEntities()
        {
            var factory = new ConnectionFactory() { HostName = RabbitHostName, UserName = RabbitUsername, Password = RabbitPassword };

            var connection = factory.CreateConnection();
            
                Console.WriteLine("recieving entities");
            var channel = connection.CreateModel();
                
                    channel.QueueDeclare("mongoose_entity",
                        true,
                        false,
                        false,
                        null);
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: "mongoose_entity", autoAck: false, consumer: consumer);

                    consumer.Received += (model, ea) =>
                                    {
                                        var body = ea.Body;
                                        var message = Encoding.UTF8.GetString(body);
                                        var output = JsonConvert.DeserializeObject<sync_entity>(message);

                                        Data.StoreEntity(output);
                                        Console.WriteLine(output.entity_type);

                                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                                    };
                  
                       
                    
                
            
        }
    }
}
