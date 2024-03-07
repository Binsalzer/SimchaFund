using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFundHw.Data
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool AlwaysInclude { get; set; }
        public decimal Balance { get; set; }
    }

    public class Deposit
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public decimal Ammount { get; set; }
        public DateTime Date { get; set; }
    }


    public class Manager
    {
        private string _connection;


        public Manager(string connection)
        {
            _connection = connection;
        }

        public decimal GetTotalDeposits(int id)
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT SUM(d.Ammount) as 'Total' FROM Deposits d
                                JOIN People p
                                ON p.Id=d.PersonId
                                WHERE p.Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            
            return (decimal)cmd.ExecuteScalar();
        }

        public decimal GetTotalContributions(int id)
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT SUM(c.Ammount) as 'Total' FROM Contributions c
                                JOIN People p
                                ON p.Id=c.PersonId
                                WHERE p.Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            var reader = cmd.ExecuteReader();
            var total = reader.GetOrNull<decimal>("Total");
            if(total==null)
            {
                return 0;
            }

            return total;
        }

        public decimal GetTotalBalanceForPerson(int id)
        {
            return GetTotalDeposits(id) - GetTotalContributions(id);
        }

        public List<Person> GetPeopleInfo()
        {
            List<Person> people = new();

            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM People";

            con.Open();
            var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                people.Add(new()
                {
                    Id = (int)reader["Id"],
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    PhoneNumber = (string)reader["PhoneNumber"],
                    CreatedDate = (DateTime)reader["CreatedDate"],
                    AlwaysInclude = (bool)reader["AlwaysInclude"]
                }); 
            }

            foreach(Person p in people)
            {
                GetTotalBalanceForPerson(p.Id);
            }
            return people;
        }

        public int AddPerson(Person person)
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO People
                                VALUES (@firstName, @lastName, @phoneNumber, @createdDate, @alwaysInclude); SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@firstName", person.FirstName);
            cmd.Parameters.AddWithValue("@lastName", person.LastName);
            cmd.Parameters.AddWithValue("@phoneNumber", person.PhoneNumber);
            cmd.Parameters.AddWithValue("@createdDate", DateTime.Now);
            cmd.Parameters.AddWithValue("@alwaysInclude", person.AlwaysInclude);
            con.Open();
            return (int)(decimal)cmd.ExecuteScalar();
        }

        public void AddDeposit(Deposit deposit)
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO Deposits
                                VALUES(@personId, @ammount, @date)";
            cmd.Parameters.AddWithValue("@personId", deposit.PersonId);
            cmd.Parameters.AddWithValue("@ammount", deposit.Ammount);
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            con.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static class ReaderExtensions
    {
        public static T GetOrNull<T>(this SqlDataReader reader, string name)
        {
            object value = reader[name];
            if (value == DBNull.Value)
            {
                return default(T);
            }

            return (T)value;
        }
    }
}
