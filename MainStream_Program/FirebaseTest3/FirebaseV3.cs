using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseV3
{
    private readonly FirestoreDb firestoreDb;
    private readonly List<(string Path, FirestoreChangeListener Listener)> listeners = new();
    private readonly Dictionary<string, Dictionary<string, object>> documentCache = new();
    private readonly HashSet<string> attachedCollectionPaths = new();
    private readonly HashSet<string> visitedDocumentPaths = new();

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
            path = CleanFirestorePath(path);
            CollectionReference collectionRef = ResolveCollectionPath(path);
            if (collectionRef == null)
            {
                Console.WriteLine($"[FirebaseV3] Invalid path: {path}");
                return;
            }

            if (attachedCollectionPaths.Contains(collectionRef.Path))
                return;

            attachedCollectionPaths.Add(collectionRef.Path);
            await collectionRef.GetSnapshotAsync();

            var listener = collectionRef.Listen(snapshot =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Change detected.");

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

                    switch (change.ChangeType)
                    {
                        case DocumentChange.Type.Added:
                            Console.WriteLine("    [New Document]");
                            LogModifiedFields(newData);
                            documentCache[docId] = newData;
                            CheckAndUpdateActiveGame(collectionRef.Path, docId, null, newData);
                            _ = ListenToDocumentSubcollectionsRecursive(collectionRef.Document(docId));
                            break;

                        case DocumentChange.Type.Removed:
                            Console.WriteLine("    [Document Removed]");
                            if (documentCache.TryGetValue(docId, out var oldData))
                            {
                                CheckAndUpdateActiveGame(collectionRef.Path, docId, oldData, null);
                                documentCache.Remove(docId);
                            }
                            break;

                        case DocumentChange.Type.Modified:
                            documentCache.TryGetValue(docId, out var previousData);
                            LogModifiedFields(newData, previousData);
                            CheckAndUpdateActiveGame(collectionRef.Path, docId, previousData, newData);
                            documentCache[docId] = newData;
                            _ = ListenToDocumentSubcollectionsRecursive(collectionRef.Document(docId));
                            break;
                    }
                }
            });

            listeners.Add((path, listener));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FirebaseV3] Error in ListenToCollectionPathAsync: {ex.Message}");
        }
    }

    private void LogModifiedFields(Dictionary<string, object> newData, Dictionary<string, object> oldData = null)
    {
        foreach (var field in newData)
        {
            if (oldData == null || !oldData.ContainsKey(field.Key) || !Equals(oldData[field.Key], field.Value))
            {
                Console.WriteLine($"      {field.Key}: {(oldData?.GetValueOrDefault(field.Key)?.ToString() ?? "null")} → {field.Value}");
            }
        }
    }

    private CollectionReference ResolveCollectionPath(string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1 || parts.Length % 2 == 0) return null;

        CollectionReference current = firestoreDb.Collection(parts[0]);
        for (int i = 1; i < parts.Length; i += 2)
        {
            current = current.Document(parts[i]).Collection(i + 1 < parts.Length ? parts[i + 1] : "");
        }
        return current;
    }

    private string CleanFirestorePath(string fullPath)
    {
        const string prefix = "/documents/";
        int index = fullPath.IndexOf(prefix);
        return index >= 0 ? fullPath[(index + prefix.Length)..] : fullPath;
    }

    public void StopAllListeners()
    {
        foreach (var (_, listener) in listeners)
        {
            listener.StopAsync();
        }
        listeners.Clear();
        attachedCollectionPaths.Clear();
        Console.WriteLine("[FirebaseV3] All listeners stopped.");
    }

    public async Task ListenToSubcollectionsRecursivelyAsync(string collectionPath, string documentId = null)
    {
        CollectionReference collectionRef = ResolveCollectionPath(collectionPath);
        if (collectionRef == null) return;

        if (!attachedCollectionPaths.Contains(collectionPath))
            await ListenToCollectionPathAsync(collectionPath);

        if (!string.IsNullOrWhiteSpace(documentId))
        {
            var docRef = collectionRef.Document(documentId);
            if (!(await docRef.GetSnapshotAsync()).Exists) return;
            await ListenToDocumentSubcollectionsRecursive(docRef);
        }
        else
        {
            foreach (var doc in await collectionRef.GetSnapshotAsync())
                await ListenToDocumentSubcollectionsRecursive(doc.Reference);
        }
    }

    private async Task ListenToDocumentSubcollectionsRecursive(DocumentReference docRef)
    {
        if (!visitedDocumentPaths.Add(docRef.Path)) return;

        foreach (var subCol in await docRef.ListCollectionsAsync().ToListAsync())
        {
            string subColPath = GetRelativeCollectionPath(subCol);
            if (!attachedCollectionPaths.Contains(subColPath))
                await ListenToCollectionPathAsync(CleanFirestorePath(subColPath));

            foreach (var subDoc in await subCol.GetSnapshotAsync())
                await ListenToDocumentSubcollectionsRecursive(subDoc.Reference);
        }
    }

    private string GetRelativeCollectionPath(CollectionReference collectionRef)
    {
        var segments = new Stack<string>();
        var current = collectionRef;
        while (current != null)
        {
            segments.Push(current.Id);
            if (current.Parent != null)
            {
                segments.Push(current.Parent.Id);
                current = current.Parent.Parent;
            }
            else break;
        }
        return string.Join("/", segments);
    }

    private void CheckAndUpdateActiveGame(string collectionPath, string docId, Dictionary<string, object> oldData, Dictionary<string, object> newData)
    {
        bool wasActive = oldData != null && oldData.TryGetValue("active", out object oldVal) && oldVal is bool oldBool && oldBool;
        bool isActive = newData.TryGetValue("active", out object newVal) && newVal is bool newBool && newBool;

        string fullPath = $"{collectionPath}/{docId}";

        if (!wasActive && isActive)
        {
            CurrentActiveGamePath = fullPath;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ActiveGameTracker] New active game: {CurrentActiveGamePath}");
        }
        else if (wasActive && !isActive && CurrentActiveGamePath == fullPath)
        {
            CurrentActiveGamePath = null;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ActiveGameTracker] Active game deactivated: {fullPath}");
        }
    }

    public void StartPeriodicSubcollectionRescan(TimeSpan interval)
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var pathsToScan = new List<string>(attachedCollectionPaths);
                    foreach (var path in pathsToScan)
                    {
                        CollectionReference colRef = ResolveCollectionPath(CleanFirestorePath(path));
                        if (colRef == null) continue;
                        await RescanDocumentTree(colRef);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Rescan Error] {ex.Message}");
                }
                await Task.Delay(interval);
            }
        });
    }

    private async Task RescanDocumentTree(CollectionReference collectionRef)
    {
        try
        {
            foreach (var doc in await collectionRef.GetSnapshotAsync())
            {
                await ListenToDocumentSubcollectionsRecursive(doc.Reference);
                foreach (var subCol in await doc.Reference.ListCollectionsAsync().ToListAsync())
                {
                    string subColPath = GetRelativeCollectionPath(subCol);
                    if (!attachedCollectionPaths.Contains(subColPath))
                        await ListenToCollectionPathAsync(CleanFirestorePath(subColPath));
                    await RescanDocumentTree(subCol);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RescanTree Error] {ex.Message}");
        }
    }

    public async Task ListenToDocumentPathAsync(string documentPath)
    {
        var parts = documentPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length % 2 != 0) return;

        var docRef = firestoreDb.Document(documentPath);
        if (!(await docRef.GetSnapshotAsync()).Exists) return;

        await ListenToDocumentSubcollectionsRecursive(docRef);
    }

    public FirestoreDb GetFirestoreDb() => firestoreDb;

    public string CurrentActiveGamePath { get; private set; }
}
