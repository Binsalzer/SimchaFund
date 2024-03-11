using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.PortableExecutable;
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
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }


    public class HistoryEvent
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public bool IsDeposit { get; set; }
        public int ContributorCount { get; set; }
    }

    public class Simcha
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public int ContributorCount { get; set; }
    }

    public class Contribution
    {
        public bool Contribute { get; set; }
        public string Name { get; set; }
        public int PersonId { get; set; }
        public decimal Balance { get; set; }
        public bool AlwaysInclude { get; set; }
        public decimal Amount { get; set; }
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
            var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                return 0;
            }

            return reader.GetOrNull<decimal>("Total");
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

            if (!reader.Read())
            {
                return 0;
            }

            return reader.GetOrNull<decimal>("Total");

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
            while (reader.Read())
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

            foreach (Person p in people)
            {
                p.Balance = GetTotalBalanceForPerson(p.Id);
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
            cmd.Parameters.AddWithValue("@ammount", deposit.Amount);
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        private List<HistoryEvent> GetAllContributionsById(int id)
        {
            List<HistoryEvent> events = new();
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Contributions c
                                JOIN Simcha s
                                ON c.SimchaId=s.Id
                                WHERE PersonId =@id";
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new()
                {
                    Amount = (decimal)reader["Ammount"],
                    Date = (DateTime)reader["Date"],
                    Action = $"Contribution for the {(string)reader["Name"]}",
                    IsDeposit = false
                });
            }
            return events;
        }

        private List<HistoryEvent> AddAllDepositsToContributionsListById(int id, List<HistoryEvent> events)
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT d.* FROM Deposits d
                                JOIN People p
                                ON p.Id=d.PersonId
                                WHERE PersonId=@id";
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new()
                {
                    Amount = (decimal)reader["Ammount"],
                    Date = (DateTime)reader["Date"],
                    Action = "Deposit",
                    IsDeposit = true
                }); ;
            }
            return events;
        }

        public List<HistoryEvent> GetHistoryByPersonId(int id)
        {
            var events = GetAllContributionsById(id);
            events = AddAllDepositsToContributionsListById(id, events);



            return events.OrderByDescending(h => h.Date).ToList();
        }

        public string GetFullNameForPersonId(int id)
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT FirstName, LastName FROM People
                                WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }
            return $"{(string)reader["FirstName"]} {(string)reader["LastName"]}";
        }

        public decimal GetTotalOfAllDeposits()
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT SUM(Ammount) as Total FROM Deposits";
            con.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return 0;
            }
            return reader.GetOrNull<decimal>("Total");
        }

        public decimal GetTotalOfAllContributions()
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT SUM(Ammount) as Total FROM Contributions";
            con.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return 0;
            }
            return reader.GetOrNull<decimal>("Total");
        }

        public decimal GetTotalBalanceForFund()
        {
            return GetTotalOfAllDeposits() - GetTotalOfAllContributions();
        }

        public List<Simcha> GetBasicSimchaInfo()
        {
            List<Simcha> simchas = new();
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Simcha       
                                ORDER BY Date DESC";
            con.Open();
            var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                simchas.Add(new()
                {
                    Id = (int)reader["id"],
                    Name = (string)reader["Name"],
                    Date = (DateTime)reader["Date"],
                    Total = 0,
                    ContributorCount = 0
                }); 
            }
            return simchas;
        }

        public List<Simcha> GetAllSimchas()
        {
            var simpleSimchas = GetBasicSimchaInfo();
            List<Simcha> simchas = new();
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT s.*, SUM(c.Ammount) as 'Total', COUNT(c.PersonId) as ContributorCount FROM Simcha s
                                JOIN Contributions c
                                ON c.SimchaId=s.Id
                                GROUP BY s.Id, s.Name, s.Date
                                ORDER BY s.Date DESC";

            con.Open();
            var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                simchas.Add(new()
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"],
                    Date = (DateTime)reader["Date"],
                    Total=reader.GetOrNull<decimal>("Total"),
                    ContributorCount=reader.GetOrNull<int>("ContributorCount")
                });
            }

            List<Simcha> final = new();
       
            foreach(Simcha ss in simpleSimchas)
            {
                bool added = false;
                foreach (Simcha s in simchas)
                {
                    
                    if(s.Id==ss.Id)
                    {
                        final.Add(s);
                        added = true;
                    }
                }
                if(!added)
                {
                    final.Add(ss);
                }
            }

            return final;
        }

        public int GetAmmountOfPeopleInDB()
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) as PeopleCount FROM People";
            con.Open();
            var reader = cmd.ExecuteReader();
            if(!reader.Read())
            {
                return 0;
            }

            return reader.GetOrNull<int>("PeopleCount");
        }

        public void AddSimcha(DateTime date, string name)
        {
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO Simcha
                                VALUES(@date, @name)";
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@name", name);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public List<Contribution> GetContributionInfoBySimchaId(int id)
        {
            List<Contribution> contributions = new();
            SqlConnection con = new(_connection);
            var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT p.Id as 'PersonId', c.Ammount FROM Simcha s
                                JOIN Contributions c
                                On s.Id=c.SimchaId
                                JOIN People p
                                ON p.Id=C.PersonId
                                WHERE SimchaId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                contributions.Add(new()
                {
                    PersonId = (int)reader["PersonId"],
                    Amount = (decimal)reader["Ammount"]
                });
            }
            return contributions;
        }

        public List<Contribution> GetContributionsById(int id)
        {
            List<Contribution> contributions = new();
            foreach (Person p in GetPeopleInfo())
            {
                contributions.Add(new()
                {
                    Name = $"{p.FirstName} {p.LastName}",
                    Balance = p.Balance,
                    AlwaysInclude = p.AlwaysInclude,
                    PersonId = p.Id
                });
            }

            foreach (Contribution contribution in contributions)
            {
                var match = GetContributionInfoBySimchaId(id).First(c => c.PersonId == contribution.PersonId);
                if (match != null)
                {
                    contribution.Amount = match.Amount;
                    contribution.Contribute = true;
                }
            }

            return contributions;
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

