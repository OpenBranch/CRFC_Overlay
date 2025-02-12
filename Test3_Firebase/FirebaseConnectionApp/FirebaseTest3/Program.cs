using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/* Next Steps
 *  - Setup text files and format stream to display that information
 *  - C# program edit text files to update in real time
 *  - I am sure the timer needing to update every second might cause some problems
 *  - Pulling logo images dynamically from links?
 *  - Socket communication between my computer and Aaron's computer, closing and re-opening connection
 *  - Aaron's program sends me a signal saying reload data (Sends document name) which triggers the program to search that document on db and pull relevant information to text files
 *  - Aaron's program sends a socket communication for time changed which updates my program's timer on stream for the game clock
 */ 



class Program
{
    static async Task Main()
    {
        int port = 5000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);

        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        listener.Start();
        Console.WriteLine($"Server started. Listening on port {port}...");

        // Replace "your-project-id" with your actual Firebase project ID
        var firestoreService = new FirestoreService("robotic-football-game-stats");

        try
        {
            //await firestoreService.ReadData();
            //await firestoreService.AddData();
            //await firestoreService.UpdateData();
            //await firestoreService.DeleteField();
            //await firestoreService.AddField();
            //await firestoreService.DeleteDocument();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine("Client connected.");

        // Force socket closure immediately
        client.LingerState = new LingerOption(true, 0);

        using (NetworkStream stream = client.GetStream())
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Received: {message}");

                byte[] response = Encoding.UTF8.GetBytes($"Server received: {message}");
                await stream.WriteAsync(response, 0, response.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        client.Close();
        client.Dispose();
        Console.WriteLine("Client disconnected.");
    }

}

