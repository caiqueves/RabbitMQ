﻿using System;
using System.Text;
using System.Text.Json;
using ExampleRabbitMQ.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ExampleRabbitMQ.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private ILogger<StudentController> _logger;

        public StudentController(ILogger<StudentController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult InsertOrder(Student order)
        {
            try
            {
                //Inserir na fila
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "studentQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = JsonSerializer.Serialize(order);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "studentQueue",
                                         basicProperties: null,
                                         body: body);
                }

                return Accepted(order);
            }
            catch (Exception ex)
            {

                _logger.LogError("Erro ao criar pedido", ex);

                return new StatusCodeResult(500);
            }
        }
    }
}