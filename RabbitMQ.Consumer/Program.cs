using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using ExampleRabbitMQ.Api.Domain;
using RabbitMQ.Api.Domain;

namespace ExampleRabbitMQ.Consumer
{
    public class Program
    {
        private const string amqpConnection = "amqps://ezsepkae:qQaqsEwupQIeN_xbzD8q3KupISGcq59f@jackal.rmq.cloudamqp.com/ezsepkae_";

        protected static void Main(string[] args)
        {
            var connection = CreateConnection(0);

            IModel channel1 = CreateChannel(connection);

            DeclareQueue(channel1, "studentQueue");
            DeclareQueue(channel1, "UnitQueue1");

            ConsummerQueue(channel1, "studentQueue");
            ConsummerQueue(channel1, "UnitQueue1");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Create connection
        /// </summary>
        /// <param name="options"></param>
        /// <returns> connection </returns>
        public static IConnection CreateConnection(int options)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            if (options == 0)
            {
                connectionFactory =  new ConnectionFactory() { HostName = "localhost" };
            }
            else
            {
                //Usando Serviço AMQP Rabbbit.MQ externo
                connectionFactory = new ConnectionFactory { Uri = new Uri(amqpConnection) };
            }

            return connectionFactory.CreateConnection();

        }
        public static IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

            return channel;
        }
       
        
        public static void DeclareQueue( IModel channel, string nameQueue)
        {
            channel.QueueDeclare(queue: nameQueue,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
        }
  
        private static void ConsummerQueue(IModel channel, string nameQueueProdutor)
        {
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);


                    if (nameQueueProdutor.Equals("studentQueue"))
                    {
                        var student = JsonSerializer.Deserialize<Student>(message);
                        Console.WriteLine($"Produtor : {nameQueueProdutor} => student {student.StudentId} | {student.Name} |  {student.Age}", message);
                    }
                    else
                    {
                        var unit = JsonSerializer.Deserialize<Unit>(message);
                        Console.WriteLine($"Produtor : {nameQueueProdutor} => Materia : {unit.materia} | nota {unit.nota} |", message);
                    }

                    // Ack sucesso na regra
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    // Nack erro voltou para filha ..
                    channel.BasicNack(ea.DeliveryTag, false, true); //tratando o erro - "Perdeu conexão ou erro na regra de negocio"
                }

            };

            channel.BasicConsume(queue: nameQueueProdutor,
                                 autoAck: false, //Informa que recebeu a mensagem (true = recebeu / false + não recebeu)
                                 consumer: consumer);
            //return consumer;
        }
    }
}
