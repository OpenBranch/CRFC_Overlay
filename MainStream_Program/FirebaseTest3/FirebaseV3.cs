using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseV3
{
    private readonly FirestoreDb firestoreDb;

    // Track listeners along with the Firestore-relative collection path they’re attached to
    private readonly List<(string Path, FirestoreChangeListener Listener)> listeners = new();

    // Cache to track previous document state for change detection
    private Dictionary<string, Dictionary<string, object>> documentCache = new();

    public FirebaseV3(string projectId, string serviceAccountJsonPath)
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountJsonPath);
        firestoreDb = FirestoreDb.Create(projectId);
        Console.WriteLine($"[FirebaseV3] Firestore initialized for project: {projectId}");
    }

    public async Task ListenToCollectionPathAsync(string path)
    {
        try
        {
            CollectionReference collectionRef = ResolveCollectionPath(path);
            if (collectionRef == null)
            {
                Console.WriteLine($"[FirebaseV3] Invalid path: {path}");
                return;
            }

            await collectionRef.GetSnapshotAsync();

            var listener = collectionRef.Listen(snapshot =>
            {
                Console.WriteLine($"[Listener] Change detected in: {collectionRef.Path}");

                if (snapshot == null || snapshot.Changes.Count == 0)
                {
                    Console.WriteLine("  [Initial] No document changes.");
                    return;
                }

                foreach (var change in snapshot.Changes)
                {
                    string docId = change.Document.Id;
                    var newData = change.Document.ToDictionary();

                    Console.WriteLine($"  [CHANGE] Type: {change.ChangeType}, DocID: {docId}");

                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        Console.WriteLine("    [New Document]");
                        foreach (var field in newData)
                            Console.WriteLine($"      {field.Key}: {field.Value}");
                    }
                    else if (change.ChangeType == DocumentChange.Type.Removed)
                    {
                        Console.WriteLine("    [Document Removed]");
                        documentCache.Remove(docId);
                    }
                    else if (change.ChangeType == DocumentChange.Type.Modified)
                    {
                        if (!documentCache.ContainsKey(docId))
                        {
                            Console.WriteLine("    [No previous cache found, printing full document]");
                            foreach (var field in newData)
                                Console.WriteLine($"      {field.Key}: {field.Value}");
                        }
                        else
                        {
                            var oldData = documentCache[docId];
                            foreach (var kvp in newData)
                            {
                                if (!oldData.ContainsKey(kvp.Key) || !Equals(oldData[kvp.Key], kvp.Value))
                                {
                                    Console.WriteLine($"      [Modified] {kvp.Key}: {oldData.GetValueOrDefault(kvp.Key)} → {kvp.Value}");
                                }
                            }
                        }

                        documentCache[docId] = newData;
                    }

                    if (change.ChangeType != DocumentChange.Type.Removed)
                    {
                        documentCache[docId] = newData;
                    }
                }
            });

            listeners.Add((path, listener)); // Use relative path only
            Console.WriteLine($"[Listener] Attached to collection: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FirebaseV3] Error in ListenToCollectionPathAsync: {ex.Message}");
        }
    }

    private CollectionReference ResolveCollectionPath(string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1 || parts.Length % 2 == 0)
        {
            Console.WriteLine($"[FirebaseV3] Path must end with a collection (odd segments): {path}");
            return null;
        }

        CollectionReference current = firestoreDb.Collection(parts[0]);

        for (int i = 1; i < parts.Length; i += 2)
        {
            string docId = parts[i];
            current = current.Document(docId).Collection(i + 1 < parts.Length ? parts[i + 1] : "");
        }

        return current;
    }

    public async Task<string> GetActiveGamePathAsync()
    {
        foreach (var (path, _) in listeners)
        {
            try
            {
                CollectionReference collectionRef = ResolveCollectionPath(path);
                if (collectionRef == null)
                {
                    Console.WriteLine($"[ActiveGame] Could not resolve collection from path: {path}");
                    continue;
                }

                QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    Console.WriteLine($"[ActiveGame] Checking document: {collectionRef.Path}/{doc.Id}");

                    if (doc.Exists)
                    {
                        var data = doc.ToDictionary();
                        foreach (var kvp in data)
                        {
                            Console.WriteLine($"    {kvp.Key}: {kvp.Value}");
                        }

                        if (data.TryGetValue("active", out object activeVal) &&
                            activeVal is bool isActive &&
                            isActive)
                        {
                            string activePath = $"{collectionRef.Path}/{doc.Id}";
                            Console.WriteLine($"[ActiveGame] Found active game at: {activePath}");
                            return activePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ActiveGame] Error checking path '{path}': {ex.Message}");
            }
        }

        Console.WriteLine("[ActiveGame] No active game found.");
        return null;
    }

    public void StopAllListeners()
    {
        foreach (var (_, listener) in listeners)
        {
            listener.StopAsync();
        }

        listeners.Clear();
        Console.WriteLine("[FirebaseV3] All listeners stopped.");
    }

    public FirestoreDb GetFirestoreDb() => firestoreDb;
}
