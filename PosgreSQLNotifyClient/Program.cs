using System;
using System.Threading;
using Npgsql;
using PosgreSQLNotifyClient.Properties;

namespace PosgreSQLNotifyClient
{
    public class Program
    {
        private static NpgsqlConnection connection;

        public static void Main(string[] args)
        {
            string connectionString =
                $"Server={Settings.Default.PostreSqlDbServer};Port={Settings.Default.PostreSqlDbPort};User Id={Settings.Default.PostgreSqlLogin};Password={Settings.Default.PostgreSqlPass};Database={Settings.Default.PostgreSqlDbName};ContinuousProcessing=true;KeepAlive=60";
            Console.WriteLine($"Connecting to database: {connectionString}{Environment.NewLine}");
            connection = new NpgsqlConnection(connectionString);

            Console.WriteLine($"Main Thread id: {Thread.CurrentThread.ManagedThreadId}");

            connection.Open();
            connection.Notification += ConnectionNotification;
            connection.StateChange += ConnectionStateChange;

            using (var cmd = new NpgsqlCommand($"LISTEN \"{Settings.Default.NotificationName}\"", connection))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine($"Client is listening for notifications \"{Settings.Default.NotificationName}\". To emit notification, execute \"NOTIFY {Settings.Default.NotificationName}\" on selected database.");
            Console.WriteLine();

            Console.ReadLine();

            using (var cmd = new NpgsqlCommand($"UNLISTEN \"{Settings.Default.NotificationName}\"", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void ConnectionStateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now}] Connection stated changed from {e.OriginalState} to {e.CurrentState}!");
        }

        private static void ConnectionNotification(object sender, NpgsqlNotificationEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now}] Notification arrived! Thread id: {Thread.CurrentThread.ManagedThreadId}. Additional information: {e.AdditionalInformation}");
        }
    }
}
