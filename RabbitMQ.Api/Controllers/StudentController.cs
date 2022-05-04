using System;
using System.Text;
using System.Text.Json;
using ExampleRabbitMQ.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Api.Domain;
using RabbitMQ.Client;

namespace ExampleRabbitMQ.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private ILogger<StudentController> _logger;

        private const string amqpConnection = "amqps://ezsepkae:qQaqsEwupQIeN_xbzD8q3KupISGcq59f@jackal.rmq.cloudamqp.com/ezsepkae_";

        public StudentController(ILogger<StudentController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Insert with using of one queue 
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult InsertStudent(Student student)
        {
            try
            {
                var connection = CreateConnection(0);

                IModel channel1 = CreateChannel(connection);

                DeclareQueue(channel1, "studentQueue");
                
                string message = JsonSerializer.Serialize(student);
                var body = Encoding.UTF8.GetBytes(message);

                //Inserir na fila
                channel1.BasicPublish(exchange: "",
                                     routingKey: "studentQueue",
                                     basicProperties: null,
                                     body: body);

                return Accepted(student);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao cadastrar estudante", ex);

                return new StatusCodeResult(200);
            }
        }

        /// <summary>
        /// Insert with using of exchange and three queue
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        [HttpPost("Unit")]
        public IActionResult InsertGrades(Unit unit)
        {
            try
            {
                var connection = CreateConnection(0);

                IModel channel2 = CreateChannel(connection);

                DeclareQueue(channel2, $"UnitQueue{1}");
                DeclareQueue(channel2, $"UnitQueue{2}");
                DeclareQueue(channel2, $"UnitQueue{3}");

                channel2.ExchangeDeclare("Unit", type: "fanout");

                channel2.QueueBind($"UnitQueue{1}", exchange: "Unit", routingKey: "");
                channel2.QueueBind($"UnitQueue{2}", exchange: "Unit", routingKey: "");
                channel2.QueueBind($"UnitQueue{3}", exchange: "Unit", routingKey: "");

                string message = JsonSerializer.Serialize(unit);
                var body = Encoding.UTF8.GetBytes(message);

                //Inserir na fila
                channel2.BasicPublish(exchange: "Unit",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);

                return Accepted(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao cadastrar notas", ex);

                return new StatusCodeResult(200);
            }
        }

        private static IConnection CreateConnection(int options)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            
            if (options == 0)
            {
                connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            }
            else
            {
                //Usando Serviço AMQP Rabbbit.MQ externo
                connectionFactory = new ConnectionFactory { Uri = new Uri(amqpConnection) };
            }

            return connectionFactory.CreateConnection();

        }

        private static IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

            return channel;
        }

        private static void DeclareQueue(IModel channel, string nameQueue)
        {
            channel.QueueDeclare(queue: nameQueue,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
        }
    }
}