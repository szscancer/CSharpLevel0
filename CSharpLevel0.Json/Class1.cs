﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLevel0.Json
{
    /// <summary>
    /// Newtonsoft.Json官方文档 https://www.newtonsoft.com/json/help/html/Introduction.htm
    /// </summary>
    public class JsonManager
    {
        private static Product _product;
        static JsonManager()
        {
            _product = new Product();
            _product.Name = "Apple";
            _product.ExpiryDate = new DateTime(2008, 12, 28);
            _product.Price = 3.99M;
            _product.Sizes = new string[] { "Small", "Medium", "Large" };
        }

        public static void Manage()
        {
            //ManageSimple();
            //ManageWithSettings();
            ManageWithAttribute();
        }

        private static void ManageSimple()
        {
            string output = JsonConvert.SerializeObject(_product);
            Console.WriteLine(output);
            Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
            Console.WriteLine(deserializedProduct);
        }

        #region Settings

        private static void ManageWithSettings()
        {
            ManageWithDateFormat();
            ManageWithMissingMember();
            ManageWithReferenceLoop();
            ManageWithObjectCreation();
        }

        private static void ManageWithDateFormat()
        {
            string output = JsonConvert.SerializeObject(_product, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat//时间戳,1970,1,1至今毫秒数
            });
            Console.WriteLine(output);
            //Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
            Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            });
            Console.WriteLine(deserializedProduct);
        }

        private static void ManageWithMissingMember()
        {
            string output = JsonConvert.SerializeObject(_product, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,//反序列化时对找不到的成员该如何处理（忽略或者报异常）
                NullValueHandling = NullValueHandling.Ignore,//默认为Include
                DefaultValueHandling = DefaultValueHandling.Include,//针对DefaultValueAttribute的处理
            });
            Console.WriteLine(output);
        }

        private static void ManageWithReferenceLoop()
        {
            Employee joe = new Employee { Name = "Joe User" };
            Employee mike = new Employee { Name = "Mike Manager" };
            joe.Manager = mike;
            mike.Manager = mike;

            string json = JsonConvert.SerializeObject(joe, Formatting.Indented/*缩进，即生成格式化json*/, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            Console.WriteLine(json);
        }

        private static void ManageWithObjectCreation()
        {
            string json = @"{
                            'Name': 'James',
                            'Offices': [
                              'Auckland',
                              'Wellington',
                              'Christchurch'
                            ]
                          }";

            UserViewModel model1 = JsonConvert.DeserializeObject<UserViewModel>(json);

            foreach (string office in model1.Offices)
            {
                Console.WriteLine(office);
            }

            UserViewModel model2 = JsonConvert.DeserializeObject<UserViewModel>(json, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });

            foreach (string office in model2.Offices)
            {
                Console.WriteLine(office);
            }
        }

        #endregion

        #region Attribute

        private static void ManageWithAttribute()
        {
            var person = new Person()
            {
                Name = "John Smith",
                BirthDate = new DateTime(2000, 12, 15),
                LastModified = DateTime.Now,
                Department = "DA"
            };
            var output = JsonConvert.SerializeObject(person);
            Console.WriteLine(output);
        }

        #endregion
    }

    class Product
    {
        public string Name { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Price { get; set; }
        public string[] Sizes { get; set; }
    }

    class Employee
    {
        public string Name { get; set; }
        public Employee Manager { get; set; }
    }

    class UserViewModel
    {
        public string Name { get; set; }
        public IList<string> Offices { get; private set; }

        public UserViewModel()
        {
            Offices = new List<string>
            {
                "Auckland",
                "Wellington",
                "Christchurch"
            };
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Person
    {
        // "John Smith"
        [JsonProperty]
        public string Name { get; set; }

        // "2000-12-15T22:11:03"
        [JsonProperty]
        public DateTime BirthDate { get; set; }

        // new Date(976918263055)
        [JsonProperty]
        public DateTime LastModified { get; set; }

        // not serialized because mode is opt-in
        public string Department { get; set; }
    }
}
