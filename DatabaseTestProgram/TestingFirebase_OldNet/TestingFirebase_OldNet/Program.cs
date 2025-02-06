using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;

namespace FirebaseMedium
{
    class connection
    {
        //firebase connection Settings
        public IFirebaseConfig fc = new FirebaseConfig()
        {
            AuthSecret = "7PoHQ9WXRAxvyDwMlngZFxFswDU9g2cMXcSu9YY0",
            BasePath = "https:////robotic-football-game-stats-default-rtdb.firebaseio.com//"
        };

        public IFirebaseClient client;
        //Code to warn console if class cannot connect when called.
        public connection()
        {
            try
            {
                client = new FireSharp.FirebaseClient(fc);
                Console.WriteLine("Connected to Database");
            }
            catch (Exception)
            {
                Console.WriteLine("Error Caught");
            }
        }
    }

    class data
    {
        //datas for database
        public string Name { get; set; }
        public string Surname { get; set; }
        public int age { get; set; }
    }

    class Crud
    {
        connection conn = new connection();

        //set datas to database
        public void SetData(string Name, string Surname, int age)
        {
            try
            {
                var SetData = conn.client.Set("people/" + Name, "test");
            }
            catch (Exception)
            {
                Console.WriteLine("Set Data Exception");
            }

        }

        //Update datas
        public void UpdateData(string Name, string Surname, int age)
        {
            try
            {
                data set = new data()
                {
                    Name = Name,
                    Surname = Surname,
                    age = age
                };
                var SetData = conn.client.Update("people/" + Name, set); ;
            }
            catch (Exception)
            {
                Console.WriteLine("Update Data Exception");
            }
        }

        //Delete datas
        public void DeleteData(string Name)
        {
            try
            {
                var SetData = conn.client.Delete("people/" + Name);
            }
            catch (Exception)
            {
                Console.WriteLine("Delete Data Exception");
            }
        }

        //List of the datas
        public Dictionary<string, data> LoadData()
        {
            try
            {
                FirebaseResponse al = conn.client.Get("List of People");
                Dictionary<string, data> ListData = JsonConvert.DeserializeObject<Dictionary<string, data>>(al.Body.ToString());
                return ListData;
            }
            catch (Exception)
            {
                Console.WriteLine("Dictionary Exception");
                return null;
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Crud crud = new Crud();
            crud.SetData("Ahmet Beyazıt", "Süleymanoğlu", 14);
            crud.SetData("Elif", "özkaz", 14);
            crud.SetData("Ömer", "elmas", 14);
            crud.SetData("İsmail", "türkmen", 14);
            foreach (var item in crud.LoadData())
            {
                Console.WriteLine("Name :" + item.Value.Name);
                Console.WriteLine("Surname :" + item.Value.Surname);
                Console.WriteLine("age :" + item.Value.age);
            }
            crud.DeleteData("Elif");
            crud.UpdateData("İsmail", "türkmen", 35);
            Console.WriteLine("\nUpdated Data\n\n");
            foreach (var item in crud.LoadData())
            {
                Console.WriteLine("Name :" + item.Value.Name);
                Console.WriteLine("Surname :" + item.Value.Surname);
                Console.WriteLine("age :" + item.Value.age);
            }
            Console.ReadLine();
        }
    }
}