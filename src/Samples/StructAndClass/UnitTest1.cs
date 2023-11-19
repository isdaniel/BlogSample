using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StructAndClass
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void StructEquals1()
        {
            StructType s = new StructType();
            StructType s1 = new StructType();
            Assert.IsTrue(s.Equals(s1));
        }

        [TestMethod]
        public void StructEquals2()
        {
            StructType s = new StructType();
            StructType s1 = new StructType();

            s.a = new ClassA();

            Assert.AreEqual(s.Equals(s1),false);
        }

        [TestMethod]
        public void StructEquals3()
        {
            StructType s = new StructType();
            StructType s1 = new StructType();

            ClassA aObj = new ClassA();

            s.a = aObj;
            s1.a = aObj;

            Assert.IsTrue(s.Equals(s1));
        }


        [TestMethod]
        public void ClassEqualsTest()
        {
            ClassType c = new ClassType();
            ClassType c1 = new ClassType();
            
            Assert.AreEqual(c.Equals(c1),false);
        }


        [TestMethod]
        public void PropertyStruct()
        {
            int updateVal = 100;
            MyClass myClass = new MyClass()
            {
                classA = new ClassA()
            };

            myClass.structA.UpdagteVal(updateVal);
            myClass.classA.UpdagteVal(updateVal);

            Assert.AreEqual(updateVal, myClass.classA.iVal);

            Assert.AreEqual(0, myClass.structA.iVal);
        }
    }

    public class MyClass
    {
        public StructA structA { get; set; }
        public ClassA classA { get; set; }
    }

    public struct StructA
    {
        public int iVal { get; private set; }

        public void UpdagteVal(int input)
        {
            iVal = input;
        }
    }

    public class ClassA
    {
        public int iVal { get; private set; }
        public void UpdagteVal(int input)
        {
            iVal = input;
        }
    }

    public struct StructType 
    {
        public ClassA a { get; set; }
    }

    public class ClassType
    {
        public ClassA a { get; set; }
    }
}
