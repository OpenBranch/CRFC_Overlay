using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;

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

    public async Task UpdateData()
    {
        // Reference to the document you want to update
        DocumentReference docRef = firestoreDb.Collection("games").Document("testdocument");

        // Data to update (partial update of specific fields)
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "Team1", "ONU" },
            { "Team2", "ND" },
            { "stats.team1.interceptions", 420 }
        };

        await docRef.UpdateAsync(updates);
        Console.WriteLine("Document updated successfully.");
    }

    public async Task DeleteField()
    {
        // Reference to the document you want to update
        DocumentReference docRef = firestoreDb.Collection("games").Document("testdocument");

        // Data to update (partial update of specific fields)
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "Team1", FieldValue.Delete },
        };

        await docRef.UpdateAsync(updates);
        Console.WriteLine("Field Value Deleted Correctly");
    }

    public async Task AddField()
    {
        // Reference to the document you want to update
        DocumentReference docRef = firestoreDb.Collection("games").Document("testdocument");

        // Data to update (partial update of specific fields)
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "Team1", "ONU" },
        };

        await docRef.UpdateAsync(updates);
        Console.WriteLine("Field Value Added Correctly");
    }

    public async Task DeleteDocument()
    {
        // Reference to the document you want to update
        DocumentReference docRef = firestoreDb.Collection("games").Document("testdocument");

        // Delete the document
        await docRef.DeleteAsync();
        Console.WriteLine("Document deleted successfully.");
    }


    public async Task AddData()
    {
        // Reference to the "users" collection
        CollectionReference usersCollection = firestoreDb.Collection("games");

        // Create a new document in the "users" collection
        DocumentReference userDocRef = usersCollection.Document("testdocument");

        //Stats sub dictionary
        var Team1Stats = new Dictionary<string, object>
        {
            { "interceptions", 4 },
            { "passes", 0 },
            { "points", 9 },
            { "tackles", 28 },
            { "yardage", 50 }
        };

        var Team2Stats = new Dictionary<string, object>
        {
            { "interceptions", 8 },
            { "passes", 156 },
            { "points", 8 },
            { "tackles", 59 },
            { "yardage", 6 }
        };

        var stats = new Dictionary<string, object>
        {
            { "team1", Team1Stats },
            { "team2", Team2Stats }
        };

        // Data to be added
        var game = new Dictionary<string, object>
        {
            { "name", "George Test Game" },
            { "offense", "SEA-M" },
            { "scoreTeam1", 18 },
            { "scoreTeam2", 2 },
            { "stats", stats },
            { "Team1", "Ohio Northern University" },
            { "Team2",  "Notre Dame"},
            { "year", 2025 }
        };

        // Set the data in the document
        await userDocRef.SetAsync(game);

        Console.WriteLine("Document added successfully!");
    }
}

