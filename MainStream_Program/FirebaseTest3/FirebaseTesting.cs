using System.Collections;
using Google.Cloud.Firestore;

public class FirestoreService
{
    public bool updateAllData = false;
    public Dictionary<string, object> activeGame;
    public Dictionary<string, object> activeDrive;
    public bool updateQuarter = false;
    public int currentQuarter = 0;
    public int prevDown = 0;
    public int ftToFirstDown = -1;
    public bool prevGoalNext = false;
    public bool updateDown = false;
    public bool updateFt = false;
    public bool updateGoalNext = false;
    public int team1PrevScore = 0;
    public bool team1ScoreChange = false;
    public int team2PrevScore = 0;
    public bool team2ScoreChange = false;
    public bool updateScrollMessage = true;
    public string prevScrollMessage = "";
    public bool updateClock = false;
    public int prevClockMinutes = 0;
    public int prevClockSeconds = 0;
    public bool updatePauseClock = true;
    public bool prevClockPauseValue = false;
    public bool updateTournamentCollections = false;
    public bool prevTournamentCollectionsValue = false;

    private FirestoreDb firestoreDb;
    private List<FirestoreChangeListener> listenersList;
    private string activeGameID = "";
    private string activeDriveID = "";
    private List<string> collectionsList = new List<string>();
    

    public FirestoreService(string projectId, string tempFilePath)
    {
        // Provide the path to the service account key file
        //string pathToServiceAccountKey = "D:\\GitHub_Repos\\CRFC_FirebaseSDK_Credentials.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempFilePath);

        // Initialize FirestoreDb instance
        firestoreDb = FirestoreDb.Create(projectId);

        //Setup all of the listeners
        setupFirestoreListeners();
    }

    public async Task updateSocketServerIP(string IPUpdate, int port)
    {
        // Reference to a Firestore collection
        CollectionReference collection = firestoreDb.Collection("StreamData");
        QuerySnapshot snapshot = await collection.Limit(1).GetSnapshotAsync();

        Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "StreamHostIP", IPUpdate },
                { "StreamHostPort", port }
            };
        DocumentReference docRef = collection.Document("StreamDataDocument");

        //Does this document exist already?
        if (snapshot.Count > 0)
        {
            await docRef.UpdateAsync(updates);
        } else
        {
            await docRef.CreateAsync(updates);
        }
        //Console.WriteLine("Field Value Added Correctly");
    }

    public async Task getActiveGameInformation()
    {
        // Reference to a Firestore collection
        CollectionReference collection = firestoreDb.Collection("tournaments");

        // Query the collection and get documents
        QuerySnapshot snapshot = await collection.GetSnapshotAsync();

        // Iterate through the documents
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                if (document.Id == "" + DateTime.Now.Year)
                {
                    //Console.WriteLine("Document ID: " + document.Id);
                    DocumentReference currentDocument = document.Reference;
                    IReadOnlyCollection<CollectionReference> subcollections = await currentDocument.ListCollectionsAsync().ToListAsync();
                    foreach (CollectionReference subcollection in subcollections)
                    {
                        //Console.WriteLine($"Subcollection: {subcollection.Id}");

                        QuerySnapshot subDocuments = await subcollection.GetSnapshotAsync();
                        foreach (DocumentSnapshot document1 in subDocuments.Documents)
                        {
                            //Console.WriteLine($" - Document ID: {document1.Id}");
                            
                            if (document1.Exists)
                            {
                                Dictionary<string, object> data = document1.ToDictionary();
                                foreach (var kvp in data)
                                {
                                    if (kvp.Key == "active")
                                    {
                                        if (kvp.Value.ToString().ToLower() == "true")
                                        {
                                            //Console.WriteLine("This is the active game");
                                            activeGame = data;
                                            //Getting the drives in the game
                                            IReadOnlyCollection<CollectionReference> GameSubCollections = await document1.Reference.ListCollectionsAsync().ToListAsync();
                                            foreach (CollectionReference gameCollections in GameSubCollections)
                                            {
                                                if (gameCollections.Id.ToString() == "Drives")
                                                {
                                                    QuerySnapshot DrivesSnapshop = await gameCollections.GetSnapshotAsync();
                                                    foreach (DocumentSnapshot drive in DrivesSnapshop.Documents)
                                                    {
                                                        if (drive.Exists)
                                                        {
                                                            Dictionary<string, object> DriveData = drive.ToDictionary();
                                                            foreach (var driveField in DriveData)
                                                            {
                                                                if (driveField.Key == "reasonForDriveEnd" && driveField.Value == "")
                                                                {
                                                                    //Console.WriteLine("This is the drive we want");
                                                                    activeDrive = DriveData;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Making a separate database call to get the scroll message
        collection = firestoreDb.Collection("StreamData");
        // Query the collection and get documents
        snapshot = await collection.GetSnapshotAsync();

        // Iterate through the documents
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                if (document.Id == "" + "StreamDataDocument")
                {
                    Dictionary<string, object> data = document.ToDictionary();
                    foreach (var kvp in data)
                    {
                        if (kvp.Key == "ScrollMessage")
                        {
                            prevScrollMessage = kvp.Value.ToString();
                        }
                        else if (kvp.Key == "minutes")
                        {
                            prevClockMinutes = Int32.Parse(kvp.Value.ToString());
                            updateClock = true;
                        }
                        else if (kvp.Key == "seconds")
                        {
                            prevClockSeconds = Int32.Parse(kvp.Value.ToString());
                            updateClock = true;
                        }
                    }
                }
            }
        }
        updateAllData = true;
    }

    private async void setupFirestoreListeners()
    {
        List<FirestoreChangeListener> listOfListeners = new List<FirestoreChangeListener>();

        CollectionReference collection = firestoreDb.Collection("tournaments");
        QuerySnapshot snapshot = await collection.GetSnapshotAsync();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                if (document.Id == "" + DateTime.Now.Year)
                {
                    DocumentReference currentDocument = document.Reference;
                    IReadOnlyCollection<CollectionReference> subcollections = await currentDocument.ListCollectionsAsync().ToListAsync();
                    foreach (CollectionReference subcollection in subcollections)
                    {
                        collectionsList.Add(subcollection.Id);
                        QuerySnapshot subSnapshot = await subcollection.GetSnapshotAsync();
                        //Create a listener for each (These should be quarterfinals, Semifinals, Finals)
                        FirestoreChangeListener listener = subcollection.Listen(snapshot =>
                        {
                            foreach (DocumentChange change in snapshot.Changes)
                            {
                                //Console.WriteLine("Change");
                                var newDoc = change.Document;
                                //Console.WriteLine("Document that was changed: " + newDoc.Id);

                                //This set of listeners listens for a change in active game
                                Dictionary<string, object> data = newDoc.ToDictionary();
                                foreach (var kvp in data)
                                {
                                    if (kvp.Key == "active")
                                    {
                                        if (kvp.Value.ToString().ToLower() == "true")
                                        {
                                            //Console.WriteLine("This is the active game: " + document1.Id);
                                            //Check if the active game is different
                                            if (newDoc.Id != activeGameID)
                                            {
                                                //The active game has changed
                                                //Console.WriteLine("Active Game Has Changed to: " + newDoc.Id);
                                                activeGameID = newDoc.Id;
                                                getActiveGameInformation();
                                            }
                                        }
                                    } else if (kvp.Key == "currentQuarter")
                                    {
                                        //Console.WriteLine("Current quarter Changed: " + Int32.Parse(kvp.Value.ToString()));
                                        if (Int32.Parse(kvp.Value.ToString()) != currentQuarter && newDoc.Id == activeGameID)
                                        {
                                            //Console.WriteLine("Current Quarter of Current Game Changed");
                                            //The quarter has changed so update that
                                            currentQuarter = Int32.Parse(kvp.Value.ToString());
                                            updateQuarter = true;
                                        }
                                    }
                                    else if (kvp.Key == "team1Points")
                                    {
                                        //Console.WriteLine("Current quarter Changed: " + Int32.Parse(kvp.Value.ToString()));
                                        if (Int32.Parse(kvp.Value.ToString()) != team1PrevScore && newDoc.Id == activeGameID)
                                        {
                                            //Console.WriteLine("Current Quarter of Current Game Changed");
                                            //The quarter has changed so update that
                                            team1PrevScore = Int32.Parse(kvp.Value.ToString());
                                            team1ScoreChange = true;
                                        }
                                    }
                                    else if (kvp.Key == "team2Points")
                                    {
                                        //Console.WriteLine("Current quarter Changed: " + Int32.Parse(kvp.Value.ToString()));
                                        if (Int32.Parse(kvp.Value.ToString()) != team2PrevScore && newDoc.Id == activeGameID)
                                        {
                                            //Console.WriteLine("Current Quarter of Current Game Changed");
                                            //The quarter has changed so update that
                                            team2PrevScore = Int32.Parse(kvp.Value.ToString());
                                            team2ScoreChange = true;
                                        }
                                    }
                                }


                            }
                        });
                        //Console.WriteLine("Created Listener For Collection Name: " + subcollection.Id);
                        listOfListeners.Add(listener);

                        QuerySnapshot subDocuments = await subcollection.GetSnapshotAsync();
                        foreach (DocumentSnapshot document1 in subDocuments.Documents)
                        {
                            if (document1.Exists)
                            {
                                Dictionary<string, object> data = document1.ToDictionary();
                                foreach (var kvp in data)
                                {
                                    if (kvp.Key == "active")
                                    {
                                        if (kvp.Value.ToString().ToLower() == "true")
                                        {
                                            //Console.WriteLine("This is the active game: " + document1.Id);
                                            activeGameID = document1.Id;
                                        }
                                    }
                                }
                            }
                        }

                        foreach (DocumentSnapshot subDocument in subSnapshot.Documents)
                        {
                            //Creating a listener on the drives collection for each game
                            DocumentReference subReference = subDocument.Reference;
                            IReadOnlyCollection<CollectionReference> subDrives = await subReference.ListCollectionsAsync().ToListAsync();
                            foreach (CollectionReference drive in subDrives)
                            {
                                if (drive.Id == "Drives")
                                {
                                    //Create a listener on this item

                                    FirestoreChangeListener subListener = drive.Listen(snapshot =>
                                    {
                                        foreach (DocumentChange change in snapshot.Changes)
                                        {
                                            //Console.WriteLine("Change: " + change.Document.Id);
                                            var newDoc = change.Document;
                                            //Console.WriteLine("Document that was changed: " + newDoc.Id);

                                            //This set of listeners listens for a change in the drives collection
                                            Dictionary<string, object> data = newDoc.ToDictionary();
                                            foreach (var kvp in data)
                                            {
                                                if (kvp.Key == "reasonForDriveEnd" && kvp.Value.ToString() == "" && activeDriveID != change.Document.Id)
                                                {
                                                    //Console.WriteLine("Current Active Drive changed to: " + change.Document.Id);
                                                    activeDriveID = change.Document.Id;
                                                    foreach(var kvp1 in data)
                                                    {
                                                        if (kvp1.Key == "currentDown")
                                                        {
                                                            prevDown = Int32.Parse(kvp1.Value.ToString());
                                                        } else if (kvp1.Key == "feetToFirstDown")
                                                        {
                                                            ftToFirstDown = Int32.Parse(kvp1.Value.ToString());
                                                        } else if (kvp1.Key == "goalNext")
                                                        {
                                                            prevGoalNext = bool.Parse(kvp1.Value.ToString().ToLower());
                                                        }
                                                    }
                                                    updateDown = true;
                                                    updateFt = true;
                                                    updateGoalNext = true;
                                                }
                                                else if (kvp.Key == "currentDown" && prevDown != Int32.Parse(kvp.Value.ToString()))
                                                {
                                                    //The current down number changed
                                                    prevDown = Int32.Parse(kvp.Value.ToString());
                                                    updateDown = true;
                                                    //Console.WriteLine("Current Down Changed: " + updateDown);
                                                }
                                                else if (kvp.Key == "feetToFirstDown" && ftToFirstDown != Int32.Parse(kvp.Value.ToString()))
                                                {
                                                    //Console.WriteLine("Current Ft Changed");
                                                    ftToFirstDown = Int32.Parse(kvp.Value.ToString());
                                                    updateFt = true;
                                                }
                                                else if (kvp.Key == "goalNext" && prevGoalNext != bool.Parse(kvp.Value.ToString().ToLower()))
                                                {
                                                    prevGoalNext = bool.Parse(kvp.Value.ToString().ToLower());
                                                    updateGoalNext = true;
                                                }
                                            }
                                        }
                                    });
                                    listOfListeners.Add(subListener);
                                }
                            }
                        }
                    }
                }
            }
        }

        //Listening for new tournament category to be created and dynamically creating more listeners
        FirestoreChangeListener creationListener = collection.Listen(snapshot =>
        {
            foreach (DocumentChange change in snapshot.Changes)
            {
                //Console.WriteLine("Change");
                var newDoc = change.Document;
                //Console.WriteLine("Document that was changed: " + newDoc.Id);
                if (newDoc.Id == DateTime.Now.Year.ToString())
                {
                    //This set of listeners listens for a new collection to be created in the active year
                    Dictionary<string, object> data = newDoc.ToDictionary();
                    foreach (var kvp in data)
                    {
                        if (kvp.Key == "UpdateCollections" && bool.Parse(kvp.Value.ToString().ToLower()) != prevTournamentCollectionsValue)
                        {
                            //updateTournamentCollections = true;
                            prevTournamentCollectionsValue = bool.Parse(kvp.Value.ToString().ToLower());
                            //Console.WriteLine("Triggered an update Message");
                            getNewTournamentCollections(newDoc);

                            //Wait 2 seconds before changing the value back to give it a minute to catch up
                            System.Threading.Thread.Sleep(2000);
                            DocumentReference docRef = newDoc.Reference;
                            Dictionary<string, object> updates = new Dictionary<string, object>{{ "UpdateCollections", false }};
                            docRef.UpdateAsync(updates);
                        }
                    }                    
                }
            }
        });
        listOfListeners.Add(creationListener);



        //Creating the listener for scrolling text changes
        CollectionReference collection2 = firestoreDb.Collection("StreamData");
        QuerySnapshot snapshot2 = await collection.GetSnapshotAsync();
        FirestoreChangeListener scrollListener = collection2.Listen(snapshot =>
        {
            foreach (DocumentChange change in snapshot.Changes)
            {
                //Console.WriteLine("Change");
                var newDoc = change.Document;
                //Console.WriteLine("Document that was changed: " + newDoc.Id);
                if (newDoc.Id == "StreamDataDocument")
                {
                    //This set of listeners listens for a change in active game
                    Dictionary<string, object> data = newDoc.ToDictionary();
                    foreach (var kvp in data)
                    {
                        if (kvp.Key == "ScrollMessage" && kvp.Value.ToString() != prevScrollMessage) 
                        {
                            updateScrollMessage = true;
                            prevScrollMessage = kvp.Value.ToString();
                            //Console.WriteLine("Updated Message");
                        }
                        else if (kvp.Key == "minutes" && Int32.Parse(kvp.Value.ToString()) != prevClockMinutes)
                        {
                            updateClock = true;
                            prevClockMinutes = Int32.Parse(kvp.Value.ToString());
                            //Console.WriteLine("Minutes changed");
                        }
                        else if (kvp.Key == "seconds" && Int32.Parse(kvp.Value.ToString()) != prevClockSeconds)
                        {
                            updateClock = true;
                            prevClockSeconds = Int32.Parse(kvp.Value.ToString());
                            //Console.WriteLine("Seconds changed");
                        }
                        else if (kvp.Key == "pauseClock" && bool.Parse(kvp.Value.ToString().ToLower()) != prevClockPauseValue)
                        {
                            updatePauseClock = true;
                            prevClockPauseValue = bool.Parse(kvp.Value.ToString().ToLower());
                            //Console.WriteLine("Pause changed");
                        }
                    }
                }
            }
        });
        //Console.WriteLine("Created Listener For Collection Name: " + collection2.Id);
        listOfListeners.Add(scrollListener);

        //Log the listeners to the array
        listenersList = listOfListeners;
    }

    public async Task getNewTournamentCollections(DocumentSnapshot originalDocument)
    {
        DocumentReference currentDocument = originalDocument.Reference;
        IReadOnlyCollection<CollectionReference> subcollections = await currentDocument.ListCollectionsAsync().ToListAsync();
        foreach (CollectionReference subcollection in subcollections)
        {
            if (!collectionsList.Contains(subcollection.Id))
            {
                Console.WriteLine("Found a new Collection Named: " + subcollection.Id);
                //This collection is not in the current year's inventory as of last check so it was just created
                collectionsList.Add(subcollection.Id);

                QuerySnapshot subSnapshot = await subcollection.GetSnapshotAsync();
                //Create a listener for each (These should be quarterfinals, Semifinals, Finals)
                FirestoreChangeListener listener = subcollection.Listen(snapshot =>
                {
                    foreach (DocumentChange change in snapshot.Changes)
                    {
                        //Console.WriteLine("Change");
                        var newDoc = change.Document;
                        //Console.WriteLine("Document that was changed: " + newDoc.Id);

                        //This set of listeners listens for a change in active game
                        Dictionary<string, object> data = newDoc.ToDictionary();
                        foreach (var kvp in data)
                        {
                            if (kvp.Key == "active")
                            {
                                if (kvp.Value.ToString().ToLower() == "true")
                                {
                                    //Console.WriteLine("This is the active game: " + document1.Id);
                                    //Check if the active game is different
                                    if (newDoc.Id != activeGameID)
                                    {
                                        //The active game has changed
                                        //Console.WriteLine("Active Game Has Changed to: " + newDoc.Id);
                                        activeGameID = newDoc.Id;
                                        getActiveGameInformation();
                                    }
                                }
                            }
                            else if (kvp.Key == "currentQuarter")
                            {
                                //Console.WriteLine("Current quarter Changed: " + Int32.Parse(kvp.Value.ToString()));
                                if (Int32.Parse(kvp.Value.ToString()) != currentQuarter && newDoc.Id == activeGameID)
                                {
                                    //Console.WriteLine("Current Quarter of Current Game Changed");
                                    //The quarter has changed so update that
                                    currentQuarter = Int32.Parse(kvp.Value.ToString());
                                    updateQuarter = true;
                                }
                            }
                            else if (kvp.Key == "team1Points")
                            {
                                //Console.WriteLine("Current quarter Changed: " + Int32.Parse(kvp.Value.ToString()));
                                if (Int32.Parse(kvp.Value.ToString()) != team1PrevScore && newDoc.Id == activeGameID)
                                {
                                    //Console.WriteLine("Current Quarter of Current Game Changed");
                                    //The quarter has changed so update that
                                    team1PrevScore = Int32.Parse(kvp.Value.ToString());
                                    team1ScoreChange = true;
                                }
                            }
                            else if (kvp.Key == "team2Points")
                            {
                                //Console.WriteLine("Current quarter Changed: " + Int32.Parse(kvp.Value.ToString()));
                                if (Int32.Parse(kvp.Value.ToString()) != team2PrevScore && newDoc.Id == activeGameID)
                                {
                                    //Console.WriteLine("Current Quarter of Current Game Changed");
                                    //The quarter has changed so update that
                                    team2PrevScore = Int32.Parse(kvp.Value.ToString());
                                    team2ScoreChange = true;
                                }
                            }
                        }


                    }
                });
                //Console.WriteLine("Created Listener For Collection Name: " + subcollection.Id);
                listenersList.Add(listener);

                QuerySnapshot subDocuments = await subcollection.GetSnapshotAsync();
                foreach (DocumentSnapshot document1 in subDocuments.Documents)
                {
                    if (document1.Exists)
                    {
                        Dictionary<string, object> data = document1.ToDictionary();
                        foreach (var kvp in data)
                        {
                            if (kvp.Key == "active")
                            {
                                if (kvp.Value.ToString().ToLower() == "true")
                                {
                                    //Console.WriteLine("This is the active game: " + document1.Id);
                                    activeGameID = document1.Id;
                                }
                            }
                        }
                    }
                }

                foreach (DocumentSnapshot subDocument in subSnapshot.Documents)
                {
                    //Creating a listener on the drives collection for each game
                    DocumentReference subReference = subDocument.Reference;
                    IReadOnlyCollection<CollectionReference> subDrives = await subReference.ListCollectionsAsync().ToListAsync();
                    foreach (CollectionReference drive in subDrives)
                    {
                        if (drive.Id == "Drives")
                        {
                            //Create a listener on this item

                            FirestoreChangeListener subListener = drive.Listen(snapshot =>
                            {
                                foreach (DocumentChange change in snapshot.Changes)
                                {
                                    //Console.WriteLine("Change: " + change.Document.Id);
                                    var newDoc = change.Document;
                                    //Console.WriteLine("Document that was changed: " + newDoc.Id);

                                    //This set of listeners listens for a change in the drives collection
                                    Dictionary<string, object> data = newDoc.ToDictionary();
                                    foreach (var kvp in data)
                                    {
                                        if (kvp.Key == "reasonForDriveEnd" && kvp.Value.ToString() == "" && activeDriveID != change.Document.Id)
                                        {
                                            //Console.WriteLine("Current Active Drive changed to: " + change.Document.Id);
                                            activeDriveID = change.Document.Id;
                                            foreach (var kvp1 in data)
                                            {
                                                if (kvp1.Key == "currentDown")
                                                {
                                                    prevDown = Int32.Parse(kvp1.Value.ToString());
                                                }
                                                else if (kvp1.Key == "feetToFirstDown")
                                                {
                                                    ftToFirstDown = Int32.Parse(kvp1.Value.ToString());
                                                }
                                                else if (kvp1.Key == "goalNext")
                                                {
                                                    prevGoalNext = bool.Parse(kvp1.Value.ToString().ToLower());
                                                }
                                            }
                                            updateDown = true;
                                            updateFt = true;
                                            updateGoalNext = true;
                                        }
                                        else if (kvp.Key == "currentDown" && prevDown != Int32.Parse(kvp.Value.ToString()))
                                        {
                                            //The current down number changed
                                            prevDown = Int32.Parse(kvp.Value.ToString());
                                            updateDown = true;
                                            //Console.WriteLine("Current Down Changed: " + updateDown);
                                        }
                                        else if (kvp.Key == "feetToFirstDown" && ftToFirstDown != Int32.Parse(kvp.Value.ToString()))
                                        {
                                            //Console.WriteLine("Current Ft Changed");
                                            ftToFirstDown = Int32.Parse(kvp.Value.ToString());
                                            updateFt = true;
                                        }
                                        else if (kvp.Key == "goalNext" && prevGoalNext != bool.Parse(kvp.Value.ToString().ToLower()))
                                        {
                                            prevGoalNext = bool.Parse(kvp.Value.ToString().ToLower());
                                            updateGoalNext = true;
                                        }
                                    }
                                }
                            });
                            listenersList.Add(subListener);
                        }
                    }
                }
                getActiveGameInformation();
            }
        }

        //We made it to the end so update the variable back to false

    }
}

