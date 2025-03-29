using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string serverIp = "192.168.101.207"; // Change this to the server's IP
        int port = 1025; // Change this to the server's port


        System.Threading.Thread.Sleep(2000);
        await UpdateAllStreamData(serverIp, port);
        System.Threading.Thread.Sleep(2000);

        /* Testing Clock Set, Stop, Start Functions
        await StartCountdown(serverIp, port);
        System.Threading.Thread.Sleep(5000);
        await PauseCountdown(serverIp, port);
        System.Threading.Thread.Sleep(5000);
        await ResumeCountdown(serverIp, port);
        */
    }

    static async Task StartCountdown(string serverIp, int port)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                Console.WriteLine("Connecting to server...");
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Connected!");

                using (NetworkStream stream = client.GetStream())
                {
                    // Sending data to the server
                    string message = "UPDATECLOCK(11:52)";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(data, 0, data.Length);
                    Console.WriteLine($"Sent: {message}");

                    // Receiving response from the server
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {response}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task UpdateAllStreamData(string serverIp, int port)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                Console.WriteLine("Connecting to server...");
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Connected!");

                using (NetworkStream stream = client.GetStream())
                {
                    // Sending data to the server
                    string message = "UPDATEALL";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(data, 0, data.Length);
                    Console.WriteLine($"Sent: {message}");

                    // Receiving response from the server
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {response}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task PauseCountdown(string serverIp, int port)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                Console.WriteLine("Connecting to server...");
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Connected!");

                using (NetworkStream stream = client.GetStream())
                {
                    // Sending data to the server
                    string message = "PAUSECLOCK";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(data, 0, data.Length);
                    Console.WriteLine($"Sent: {message}");

                    // Receiving response from the server
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {response}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task ResumeCountdown(string serverIp, int port)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                Console.WriteLine("Connecting to server...");
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Connected!");

                using (NetworkStream stream = client.GetStream())
                {
                    // Sending data to the server
                    string message = "STARTCLOCK";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(data, 0, data.Length);
                    Console.WriteLine($"Sent: {message}");

                    // Receiving response from the server
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {response}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
