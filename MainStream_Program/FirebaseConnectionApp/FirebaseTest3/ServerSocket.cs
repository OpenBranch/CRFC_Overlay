using System.Data;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;

public class ServerSocket
{
    //Add any public values to control here
    public string ipAddress = "172.25.112.64";
    public int port = 1025;
    public Socket serverSocket;
    public Socket clientSocket;
    public bool openNewServer = true;
    public FirestoreService firestoreService;

    public bool timeUpdate = false;
    public int minute = -1;
    public int second = -1;
    public bool pauseClock = false;

    //Constructor
    public ServerSocket(string ipAddress, int port, FirestoreService firestoreService)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.firestoreService = firestoreService;
        Console.WriteLine("Server listening on " + ipAddress + ":" + port);
    }

    public void runServerSocket()
    {
        while (openNewServer)
        {
            createServerSocket();
            Console.WriteLine("Created a new instance for connection");
        }
    }

    public void createServerSocket()
    {
        try
        {
            //Create the server
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);

            //Accept connection
            clientSocket = serverSocket.Accept();
            //Program.logPrint("Client Connected");

            //Read message
            byte[] buffer = new byte[1024];
            int bytesRecieved = clientSocket.Receive(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRecieved);
            //Program.logPrint("Robot Request-> " + message);

            string response = "";
            if (message.Contains("UPDATECLOCK"))
            {
                response = updateClock(message);
            } else if (message.Contains("PAUSECLOCK")) {
                //Pause the countdown for now
                pauseClock = true;
                response = "Game Clock Paused on Stream";
            }
            else if (message.Contains("STARTCLOCK"))
            {
                //Pause the countdown for now
                pauseClock = false;
                response = "Game Clock Resumed on Stream";
            } else if (message.Contains("UPDATEALL"))
            {
                //Go to database and get the following (Scores for both teams, Current game Quarter, Yardage, Teams Playing)
                firestoreService.getActiveGameInformation();
            }
            else
            {
                response = "ERROR: Unknown Command";
            }

            //Send the response to the robot
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            clientSocket.Send(responseBytes);

            //Close connection
            clientSocket.Close();
            serverSocket.Close();
            //Program.logPrint("Connection closed");

            openNewServer = true;
            Console.WriteLine("Sent: " + response);
        }

        catch (Exception e)
        {
            openNewServer = false;
        }
    }

    public string updateClock(string message)
    {
        //The stream needs to parse the time value coming in and update the clock
        try
        {
            message = message.Replace("UPDATECLOCK(", "");
            message = message.Replace(")", "");
            minute = Int32.Parse(("" + message[0] + message[1]));
            second = Int32.Parse(("" + message[3] + message[4]));
            Console.WriteLine("Minute: " + minute);
            Console.WriteLine("Second: " + second);

            timeUpdate = true;
            return "Time Correctly Updated To " + minute + ":" + second;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in ServerSocket.updateClock(): {e.Message}");
        }
        return "An Error Occured in ServerSocket.updateClock()";

    }
}

