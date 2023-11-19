using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReferenceAndValueType
{
    public class Person
    {
        public int Age { get; set; }

        public string Name { get; set; }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void PassingAddress()
        {
            Person result = default(Person);

            SetPerson(result);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void PassingAddress1()
        {
            Person result = default(Person);

            SetPerson(ref result);

            Assert.AreEqual(100, result.Age);
        }

        public void SetPerson(Person p)
        {
            p = new Person()
            {
                Age = 100
            };
        }

        public void SetPerson(ref Person p)
        {
            p = new Person()
            {
                Age = 100
            };
        }
    }
}