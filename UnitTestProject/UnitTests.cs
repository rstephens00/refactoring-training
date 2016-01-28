﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Refactoring;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTests
    {
        private List<User> users;
        private List<User> originalUsers;
        private List<Product> products;
        private List<Product> originalProducts;

        [TestInitialize]
        public void Test_Initialize()
        {
            // Load users from data file
            originalUsers = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(@"Data\Users.json"));
            users = DeepCopy<List<User>>(originalUsers);

            // Load products from data file
            originalProducts = JsonConvert.DeserializeObject<List<Product>>(File.ReadAllText(@"Data\Products.json"));
            products = DeepCopy<List<Product>>(originalProducts);
        }

        [TestCleanup]
        public void Test_Cleanup()
        {
            // Restore users
            string json = JsonConvert.SerializeObject(originalUsers, Formatting.Indented);
            File.WriteAllText(@"Data\Users.json", json);
            users = DeepCopy<List<User>>(originalUsers);

            // Restore products
            string json2 = JsonConvert.SerializeObject(originalProducts, Formatting.Indented);
            File.WriteAllText(@"Data\Products.json", json2);
            products = DeepCopy<List<Product>>(originalProducts);
        }

        [TestMethod]
        public void Test_StartingTuscFromMainDoesNotThrowAnException()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("2\r\nJason\r\nsfa\r\n1\r\n1\r\n8\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Program.Main(new string[] { });
                }
            }
        }

        [TestMethod]
        public void Test_TuscDoesNotThrowAnException()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("2\r\nJason\r\nsfa\r\n1\r\n1\r\n8\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }
            }
        }

        [TestMethod]
        public void Test_InvalidUserIsNotAccepted()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("1\r\nJoel\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains(Resources.msgInvalidUser));
            }
        }

        [TestMethod]
        public void Test_EmptyUserDoesNotThrowAnException()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("1\r\n\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }
            }
        }

        [TestMethod]
        public void Test_InvalidPasswordIsNotAccepted()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("2\r\nJason\r\nsfb\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains(Resources.msgIncorrectPassword));
            }
        }

        [TestMethod]
        public void Test_UserCanCancelPurchase()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("2\r\nJason\r\nsfa\r\n1\r\n0\r\n8\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains(Resources.msgInvalidQuantity));

            }
        }

        [TestMethod]
        public void Test_ErrorOccursWhenBalanceLessThanPrice()
        {
            // Update data file
            List<User> tempUsers = DeepCopy<List<User>>(originalUsers);
            tempUsers.Where(u => u.Name == "Jason").Single().Balance = 0.0;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("1\r\nJason\r\nsfa\r\n1\r\n1\r\n8\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(tempUsers, products);
                }

                Assert.IsTrue(writer.ToString().Contains(Resources.msgInsufficientFunds));
            }
        }

        [TestMethod]
        public void Test_ErrorOccursWhenProductOutOfStock()
        {
            // Update data file
            List<Product> tempProducts = DeepCopy<List<Product>>(originalProducts);
            tempProducts.Where(u => u.Name == "Chips").Single().Quantity = 0;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("2\r\nJason\r\nsfa\r\n1\r\n1\r\n8\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, tempProducts);
                }

                Assert.IsTrue(writer.ToString().Contains(string.Format(Resources.msgOutOfStock, "Chips")));
            }
        }

        private static T DeepCopy<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
