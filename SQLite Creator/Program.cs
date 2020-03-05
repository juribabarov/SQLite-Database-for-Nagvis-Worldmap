using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace SQLite_Creator
{
    public class FileData
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }

        public FileData(string name, string alias, string lat, string lng)
        {
            Name = name;
            Alias = alias;
            Lat = lat;
            Lng = lng;
        }

        public override string ToString()
        {
            return "Name: " + Name + ", Alias: " + Alias + ", Lat: " + Lat + ", Lng: " + Lng;
        }
    }

    class Program
    {
        private const string CreateTableQuery = @"CREATE TABLE IF NOT EXISTS objects (" +
            "object_id PRIMARY KEY," +
            "lat REAL NULL," +
            "lng REAL NULL," +
            "lat2 REAL NULL," +
            "lng2 REAL NULL," +
            "object NULL);" +
            "CREATE TABLE IF NOT EXISTS version (" +
            "version VARCHAR(100) PRIMARY KEY);"+
            "INSERT INTO version VALUES('1.9b8');";


        private const string DatabaseFile = "worldmap.db";
        private const string DatabaseSource = "data source=" + DatabaseFile + ";PRAGMA journal_mode=WAL;";
        //private const string SourceFile = "koordinaten-test.csv";
        private const string SourceFile = "koordinaten.txt";

        private static List<FileData> data = new List<FileData>();

        private static void LoadData()
        {
            if (!File.Exists(SourceFile))
            {
                Console.WriteLine("File not found!");
                return;
            }

            using (StreamReader reader = new StreamReader(SourceFile))
            {
                Console.WriteLine("--- Reading source file ---");

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] s = line.Split(";");
                    FileData d = new FileData(s[0], s[1], s[2], s[3]);
                    data.Add(d);
                    Console.WriteLine(d.ToString());
                }

                Console.WriteLine("--- End of file ---");
            }
        }

        private static void FillDatabase()
        {
            if (File.Exists(DatabaseFile))
            {
                File.Delete(DatabaseFile);
            }

            SQLiteConnection.CreateFile(DatabaseFile);

            using (var connection = new SQLiteConnection(DatabaseSource))
            {
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();

                    command.CommandText = CreateTableQuery;
                    command.ExecuteNonQuery();

                    foreach (FileData item in data)
                    {
                        string o = "{\"x\":\"" + item.Lat + "\",\"y\":\"" + item.Lng + "\",\"type\":\"host\",\"host_name\":\"" + item.Name + "\",\"object_id\":\"" + item.Name + "\"}";

                        command.CommandText = "INSERT INTO objects (object_id, lat, lng, lat2, lng2, object) VALUES ('" + item.Name + "', '" + item.Lat + "', '" + item.Lng + "', '', '', '" + o + "')";
                        command.ExecuteNonQuery();
                    }

                    //command.CommandText = "SELECT * FROM objects";

                    //using (SQLiteDataReader reader = command.ExecuteReader())
                    //{
                    //    while (reader.Read())
                    //    {
                    //        Console.WriteLine(reader.GetString(0) + ", " + reader.GetDouble(1) + ", " + reader.GetDouble(2) + ", " + reader.GetDouble(3) + ", " + reader.GetDouble(4) + ", " + reader.GetString(5));
                    //    }
                    //}

                    connection.Close();
                }
            }
        }

        static void Main(string[] args)
        {
            LoadData();
            FillDatabase();
        }
    }
}
