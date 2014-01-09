using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asd.Award.Domain
{
    public class Product
    {

        public Product()
        {

        }

        public Product(string name, string type, int price)
        {
            this.Name = name;
            this.Type = type;
            this.Price = price;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public int Price { get; set; }
    }


    public class ProductDao
    {

        public IEnumerable<Product> Get()
        {
            var list = new List<Product>();
            list.Add(new Product("Samsung I5700","Phone",200));
            list.Add(new Product("Moto X700","Phone",150));
            list.Add(new Product("Nokia S600", "Phone", 100));

            return list;
        }

        public void Insert(Product product)
        {
        }

        public void Update(Product product)
        {
        }

        public void Delete(Product product)
        {
        }
    }
}
