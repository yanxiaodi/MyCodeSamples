using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordDemo
{
    public class Student
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }

        public override string ToString()
        {
            return $"FirstName: {FirstName}, LastName: {LastName}";
        }
    }
}
