using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleRabbitMQ.Api.Domain
{
    public sealed class Student
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public float Age { get; set; }
    }
}
