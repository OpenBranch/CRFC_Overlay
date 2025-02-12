using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Replace "your-project-id" with your actual Firebase project ID
        var firestoreService = new FirestoreService("robotic-football-game-stats");

        try
        {
            await firestoreService.ReadData();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}

public class FirestoreService
{
    private FirestoreDb firestoreDb;

    public FirestoreService(string projectId)
    {
        // Provide the path to the service account key file
        string pathToServiceAccountKey = "D:\\GitHub_Repos\\CRFC_FirebaseSDK_Credentials.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", pathToServiceAccountKey);

        // Initialize FirestoreDb instance
        firestoreDb = FirestoreDb.Create(projectId);
    }

    public async Task ReadData()
    {
        // Reference to a Firestore collection
        CollectionReference collection = firestoreDb.Collection("games");

        // Query the collection and get documents
        QuerySnapshot snapshot = await collection.GetSnapshotAsync();

        // Iterate through the documents
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                Console.WriteLine($"Document ID: {document.Id}");
                foreach (var field in document.ToDictionary())
                {
                    Console.WriteLine($"{field.Key}: {field.Value}");
                }
            }
        }
    }
}

