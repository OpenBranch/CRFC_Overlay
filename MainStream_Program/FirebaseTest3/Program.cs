using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using Google.Cloud.Firestore;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Runtime.Intrinsics.X86;
using System.Numerics;
using System.Threading.Tasks.Dataflow;

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
    /*EXTRACTING TEAM LOGO IMAGES FOR STREAM*/
    static string resourceName = "MainStream_Control_Project.TeamLogos.";
    static Bitmap ONULogo = LoadEmbeddedImage(resourceName + "ONULogo" + ".png");
    static Bitmap NDLogo = LoadEmbeddedImage(resourceName + "NDLogo" + ".png");
    static Bitmap ValpoLogo = LoadEmbeddedImage(resourceName + "ValpoLogo" + ".png");
    static Bitmap NavyLogo = LoadEmbeddedImage(resourceName + "NavyLogo" + ".png");
    static Bitmap CalvinLogo = LoadEmbeddedImage(resourceName + "CalvinLogo" + ".png");
    static Bitmap CatholicLogo = LoadEmbeddedImage(resourceName + "CatholicLogo" + ".png");
    static Bitmap EvansvilleLogo = LoadEmbeddedImage(resourceName + "EvansvilleLogo" + ".png");
    static Bitmap HowardLogo = LoadEmbeddedImage(resourceName + "HowardLogo" + ".png");
    static Bitmap IndianaTechLogo = LoadEmbeddedImage(resourceName + "IndianaTechLogo" + ".png");
    static Bitmap MiamiLogo = LoadEmbeddedImage(resourceName + "MiamiLogo" + ".png");
    static Bitmap MtUnionLogo = LoadEmbeddedImage(resourceName + "MtUnionLogo" + ".png");
    static Bitmap PennStateLogo = LoadEmbeddedImage(resourceName + "PennStateLogo" + ".png");
    static Bitmap PurdueLogo = LoadEmbeddedImage(resourceName + "PurdueLogo" + ".png");
    static Bitmap TrineLogo = LoadEmbeddedImage(resourceName + "TrineLogo" + ".png");
    static Bitmap WriteStateLogo = LoadEmbeddedImage(resourceName + "WriteStateLogo" + ".png");

    /*EXTRACTING HELPER IMAGES FOR STREAM*/
    static Bitmap HedermanTrophy = LoadEmbeddedImage("MainStream_Control_Project.MSCResources." + "Hederman" + ".png");
    static Bitmap CRFCLogo = LoadEmbeddedImage("MainStream_Control_Project.MSCResources." + "CRFCLogo" + ".png");
    static Bitmap BlackSquare = LoadEmbeddedImage("MainStream_Control_Project.MSCResources." + "BlackSquare" + ".png");
    static Bitmap BackgroundOverlay = LoadEmbeddedImage("MainStream_Control_Project.MSCResources." + "BackgroundRender" + ".png");


    static async Task Main()
    {
        /*GET PROGRAM LEVEL INFORMATION*/
        string jsonContent = GetEmbeddedResource("MainStream_Control_Project.CRFC_FirebaseSDK_Credentials.json");
        string tempFilePath = Path.Combine(Path.GetTempPath(), "CRFC_FirebaseSDK_Credentials.json");
        File.WriteAllText(tempFilePath, jsonContent);
        Console.WriteLine("Firebase credentials extracted successfully!");
        var firestoreService = new FirestoreService("robotic-football-game-stats", tempFilePath);
        int port = 1025;
        //Log the IP and port to the database
        await firestoreService.updateSocketServerIP(GetLocalIPv4(), port);

        /*EXTRACTING OBS LUA SCRIPT FOR CLOCK CONTROL*/
        resourceName = "MainStream_Control_Project.OBSScripts.GameClockTimerScript.lua";
        var assembly = Assembly.GetExecutingAssembly();
        string OBSFileContent = "";
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                Console.WriteLine("Resource not found.");
                return;
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                OBSFileContent = reader.ReadToEnd();
            }
        }

        /*STREAM CONTROL VARIABLES*/
        bool pauseClock = true;
        int minutes = 0;
        int seconds = 0;


        /*MAIN PROGRAM START*/

        //Get the location of the streaming folders (Using a file folder chooser dialog)
        string streamWorkingPath = getStreamWorkingPath();
        bool generateFiles = getGenerateFiles();
        Thread.Sleep(2000);
        setupStreamFolderStructure(streamWorkingPath, ONULogo, NDLogo, HedermanTrophy, CRFCLogo, BackgroundOverlay, BlackSquare, OBSFileContent, CalvinLogo, CatholicLogo, EvansvilleLogo, HowardLogo, IndianaTechLogo, MiamiLogo, MtUnionLogo, PennStateLogo, PurdueLogo, TrineLogo, WriteStateLogo, generateFiles);
        
        /*  SETTING UP FIREBASE LISTENERS
         * */
        var firebase = new FirebaseV3("robotic-football-game-stats", tempFilePath);
        await firebase.ListenToCollectionPathAsync("tournaments/2025-2026/Finals");
        Console.WriteLine("Listening for changes. Press Enter to stop.");


        string activeGamePath = await firebase.GetActiveGamePathAsync();
        if (activeGamePath != null)
        {
            Console.WriteLine("Currently active game: " + activeGamePath);
        }

        while (true)
        {
            System.Threading.Thread.Sleep(200);
        }


        await firestoreService.getActiveGameInformation();
        int previousSecond = 0;
        Console.WriteLine("All Setup Complete: Starting Stream Management Loop");
        while (true)
        {
            //Running the timer
            /* SERVER SOCKET OLD TIMER CODE
            if (serverSocket.timeUpdate)
            {
                serverSocket.timeUpdate = false;
                minutes = serverSocket.minute;
                seconds = serverSocket.second;
            }
            */

            //New Clock Code
            if (firestoreService.updateClock)
            {
                //IF the clock needs updated we just set the minutes and seconds value to the values from the database
                minutes = firestoreService.prevClockMinutes;
                seconds = firestoreService.prevClockSeconds;
                firestoreService.updateClock = false;
                if (seconds < 0)
                {
                    minutes--;
                    seconds = 59;
                }
                if (minutes < 0)
                {
                    minutes = 0;
                    seconds = 0;
                }
                writeTimeToFile(minutes, seconds, streamWorkingPath);
            }

            if (firestoreService.updatePauseClock)
            {
                pauseClock = firestoreService.prevClockPauseValue;
                firestoreService.updatePauseClock = false;
            }

            //Every second run the code to change the time in the file
            if (DateTime.Now.Second != previousSecond && !pauseClock)
            {
                previousSecond = DateTime.Now.Second;
                seconds--;
                if (seconds < 0)
                {
                    minutes--;
                    seconds = 59;
                }
                if (minutes < 0)
                {
                    minutes = 0;
                    seconds = 0;
                }
                writeTimeToFile(minutes, seconds, streamWorkingPath);
            }

            //Controlling the quarter displayed
            if (firestoreService.updateQuarter)
            {
                firestoreService.updateQuarter = false;

                string quarter = "1st";
                if (firestoreService.currentQuarter == 2)
                {
                    quarter = "2nd";
                }
                else if (firestoreService.currentQuarter == 3)
                {
                    quarter = "3rd";
                }
                else if (firestoreService.currentQuarter == 4)
                {
                    quarter = "4th";
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentGameInfo\\Quarter.txt", quarter);
            }

            //Controlling team #1 score
            if (firestoreService.team1ScoreChange)
            {
                firestoreService.team1ScoreChange = false;
                string points = "";
                if (firestoreService.team1PrevScore < 10)
                {
                    points = "    " + firestoreService.team1PrevScore;
                }
                else if (firestoreService.team1PrevScore < 100)
                {
                    points = "  " + firestoreService.team1PrevScore;
                } else
                {
                    points = "" + firestoreService.team1PrevScore;
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentTeamInfo\\LeftTeamScore.txt", points);
            }

            //Controlling team #2 score
            if (firestoreService.team2ScoreChange)
            {
                firestoreService.team2ScoreChange = false;
                string points = "";
                if (firestoreService.team2PrevScore < 10)
                {
                    points = "    " + firestoreService.team2PrevScore;
                }
                else if (firestoreService.team2PrevScore < 100)
                {
                    points = "  " + firestoreService.team2PrevScore;
                } else
                {
                    points = "" + firestoreService.team2PrevScore;
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentTeamInfo\\RightTeamScore.txt", points);
            }

            //Controlling the downs and yardage display
            if (firestoreService.updateDown || firestoreService.updateFt || firestoreService.updateGoalNext)
            {
                //Console.WriteLine("Triggered update (curent Down) " + firestoreService.prevDown);
                firestoreService.updateFt = false;
                firestoreService.updateDown = false;
                firestoreService.updateGoalNext = false;

                //Getting the current drive data if it exists for the yards until 1st down remaining
                int currentDown = firestoreService.prevDown;
                int yardsRemaining = firestoreService.ftToFirstDown;
                bool goalNext = firestoreService.prevGoalNext;
                string yardage = "1st & 10";

                if (currentDown == 0 || yardsRemaining == 0)
                {
                    yardage = "1st & 10";
                }
                else if (currentDown == 1 && yardsRemaining != 0)
                {
                    yardage = "1st & " + yardsRemaining;
                }
                else if (currentDown == 2 && yardsRemaining != 0)
                {
                    yardage = "2nd & " + yardsRemaining;
                }
                else if (currentDown == 3 && yardsRemaining != 0)
                {
                    yardage = "3rd & " + yardsRemaining;
                }
                else if (currentDown == 2 && yardsRemaining != 0)
                {
                    yardage = "4th & " + yardsRemaining;
                }

                if (goalNext)
                {
                    if (currentDown == 1)
                    {
                        yardage = "1st & Goal";
                    }
                    else if (currentDown == 2)
                    {
                        yardage = "2nd & Goal";
                    }
                    else if (currentDown == 3)
                    {
                        yardage = "3rd & Goal";
                    }
                    else if (currentDown == 4)
                    {
                        yardage = "4th & Goal";
                    }
                }

                //Making sure there are enough characters to center everything up nicely
                while (yardage.Length < 14)
                {
                    yardage = " " + yardage + " ";
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentGameInfo\\DownAndYard.txt", yardage);
            }

            //Controlling the scrolling message
            if (firestoreService.updateScrollMessage)
            {
                //Updating the scrolling message
                string message = firestoreService.prevScrollMessage;
                for (int i = 0; i < 15; i++)
                {
                    message += " ";
                }
                writeValueToTextFile(streamWorkingPath, "\\MSCResources\\ScrollText.txt", message);
                firestoreService.updateScrollMessage = false;
            }

            //The following function just triggers all updates to occur at once
            if (firestoreService.updateAllData)
            {
                //Start by checking if we have the right team names up on stream
                string leftTeamName = getCurrentTextFileValue(streamWorkingPath, "\\CurrentTeamInfo\\LeftTeamName.txt");
                string rightTeamName = getCurrentTextFileValue(streamWorkingPath, "\\CurrentTeamInfo\\RightTeamName.txt");
                string LTeamName = "";
                string RTeamName = "";
                Bitmap rightTeamLogo = ONULogo;
                Bitmap leftTeamLogo = ONULogo;
                int LTeamPoints = 0;
                int RTeamPoints = 0;
                int currentQuarter = 0;
                foreach (var field in firestoreService.activeGame)
                {
                    //Check left team name
                    if (field.Key == "team1Name")
                    {
                        LTeamName =field.Value.ToString();
                    } else if (field.Key == "team1Points")
                    {
                        //Number of points scored so far by team #1
                        LTeamPoints = Int32.Parse(field.Value.ToString());
                    }

                    //Check right team name
                    if (field.Key == "team2Name")
                    {
                        RTeamName = field.Value.ToString();
                    }
                    else if (field.Key == "team2Points")
                    {
                        //Number of points scored so far by team #1
                        RTeamPoints = Int32.Parse(field.Value.ToString());
                    }

                    //Getting the current quarter
                    if (field.Key == "currentQuarter")
                    {
                        currentQuarter = Int32.Parse(field.Value.ToString());
                    }
                }

                //Handling leftTeam
                //Console.WriteLine("Left Team: " + leftTeamName);
                //Console.WriteLine("Right Team: " + rightTeamName);
                leftTeamLogo = getTeamLogo(LTeamName);
                LTeamName = getTeamAbbrev(LTeamName);
                rightTeamLogo = getTeamLogo(RTeamName);
                RTeamName = getTeamAbbrev(RTeamName);

                //Make the changes
                saveCurrentImageToLocation(streamWorkingPath, "\\CurrentTeamInfo\\TeamLeft.png", leftTeamLogo);
                saveCurrentImageToLocation(streamWorkingPath, "\\CurrentTeamInfo\\TeamRight.png", rightTeamLogo);
                writeValueToTextFile(streamWorkingPath, "\\CurrentTeamInfo\\LeftTeamName.txt", LTeamName);
                writeValueToTextFile(streamWorkingPath, "\\CurrentTeamInfo\\RightTeamName.txt", RTeamName);
                string points = LTeamPoints.ToString();
                if (LTeamPoints < 10)
                {
                    points = "    " + LTeamPoints;
                } else if (LTeamPoints < 100)
                {
                    points = "  " + LTeamPoints;
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentTeamInfo\\LeftTeamScore.txt", points);

                points = RTeamPoints.ToString();
                if (RTeamPoints < 10)
                {
                    points = "    " + RTeamPoints;
                }
                else if (RTeamPoints < 100)
                {
                    points = "  " + RTeamPoints;
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentTeamInfo\\RightTeamScore.txt", points);

                string quarter = "1st";
                if (currentQuarter == 2)
                {
                    quarter = "2nd";
                } else if (currentQuarter == 3)
                {
                    quarter = "3rd";
                } else if (currentQuarter == 4)
                {
                    quarter = "4th";
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentGameInfo\\Quarter.txt", quarter);

                //Getting the current drive data if it exists for the yards until 1st down remaining
                int currentDown = 0;
                int yardsRemaining = 0;
                bool goalNext = false;
                string yardage = "1st & 10";
                if (firestoreService.activeDrive != null)
                {
                    foreach (var field in firestoreService.activeDrive)
                    {
                        //Check left team name
                        if (field.Key == "currentDown")
                        {
                            currentDown = Int32.Parse(field.Value.ToString());
                        } else if (field.Key == "yardsRemaining")
                        {
                            yardsRemaining = Int32.Parse(field.Value.ToString());
                        }
                        else if (field.Key == "goalNext")
                        {
                            if (field.Value.ToString().ToLower() == "true")
                            {
                                goalNext = true;
                            }
                        }
                    }
                }

                if (currentDown == 0 || yardsRemaining == 0)
                {
                    yardage = "1st & 10";
                }
                else if (currentDown == 1 && yardsRemaining != 0)
                {
                    yardage = "1st & " + yardsRemaining;
                }
                else if (currentDown == 2 && yardsRemaining != 0)
                {
                    yardage = "2nd & " + yardsRemaining;
                }
                else if (currentDown == 3 && yardsRemaining != 0)
                {
                    yardage = "3rd & " + yardsRemaining;
                }
                else if (currentDown == 2 && yardsRemaining != 0)
                {
                    yardage = "4th & " + yardsRemaining;
                }

                if (goalNext)
                {
                    if (currentDown == 1)
                    {
                        yardage = "1st & Goal";
                    } else if (currentDown == 2)
                    {
                        yardage = "2nd & Goal";
                    } else if (currentDown == 3)
                    {
                        yardage = "3rd & Goal";
                    } else if (currentDown == 4)
                    {
                        yardage = "4th & Goal";
                    }
                }

                //Making sure there are enough characters to center everything up nicely
                while (yardage.Length < 14)
                {
                    yardage = " " + yardage + " ";
                }
                writeValueToTextFile(streamWorkingPath, "\\CurrentGameInfo\\DownAndYard.txt", yardage);


                //Updating the scrolling message
                string message = firestoreService.prevScrollMessage;
                for (int i = 0; i < 15; i++)
                {
                    message += " ";
                }
                writeValueToTextFile(streamWorkingPath, "\\MSCResources\\ScrollText.txt", message);

                firestoreService.updateAllData = false;
                //Console.WriteLine("Data Updated From Database on Stream");
            }

            //Making sure the program doesn't take all of the processing power
            System.Threading.Thread.Sleep(25);
        }
    }

    static bool writeTimeToFile(int minutes, int seconds, string streamWorkingPath)
    {
        bool returnValue = true;

        string timeValue = minutes + ":";
        if (seconds < 10)
        {
            timeValue += "0";
        }
        timeValue += seconds + "";

        string newFolderPath = streamWorkingPath + "\\CurrentGameInfo\\GameClock.txt";
        
        File.WriteAllText(newFolderPath, timeValue);
        
        return returnValue;
    }

    static string getCurrentTextFileValue(string streamWorkingPath, string Extension)
    {
        using (StreamReader reader = new StreamReader(streamWorkingPath + Extension))
        {
            return reader.ReadLine();
        }

    }

    static bool saveCurrentImageToLocation(string streamWorkingPath, string Extension, Bitmap currentImage)
    {
        currentImage.Save(streamWorkingPath + Extension, ImageFormat.Png);
        return true;

    }

    static bool writeValueToTextFile(string streamWorkingPath, string Extension, string valueToWrite)
    {
        bool success = false;
        try
        {
            File.WriteAllText(streamWorkingPath + Extension, valueToWrite);
            success = true;
        }
        catch (Exception ex)
        {
            success = false;
        }
        return success;
    }

    static string getStreamWorkingPath()
    {
        bool validPath = false;
        string selectedFolder = "";
        while (!validPath)
        {
            Console.WriteLine("Enter the path of the stream working folder: ");
            selectedFolder = Console.ReadLine();

            // Validate input
            if (string.IsNullOrWhiteSpace(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                Console.WriteLine("Invalid folder path. Please make sure the directory exists.");
            } else
            {
                validPath = true;
            }
        }
        return selectedFolder;        
    }

    static bool getGenerateFiles()
    {
        bool result = true;
        bool validAnswer = false;
        string response = "";
        while (!validAnswer)
        {
            Console.WriteLine("Do you wish to Generate all files? (Y/N)");
            response = Console.ReadLine();

            //Validate input
            if (!string.IsNullOrWhiteSpace(response))
            {
                if (response.ToLower() == "y")
                {
                    result = true;
                    validAnswer = true;
                } else if (response.ToLower() == "n")
                {
                    result = false;
                    validAnswer = true;
                }
            }
        }
        return result;
    }

    static bool setupStreamFolderStructure(string streamWorkingPath, Bitmap team1Placeholder, Bitmap team2Placeholder, Bitmap hederman, Bitmap CRFCLogo, Bitmap BackgroundOverlay, Bitmap BlackSquare, string OBSFileContent, Bitmap CalvinLogo, Bitmap CatholicLogo, Bitmap EvansvilleLogo, Bitmap HowardLogo, Bitmap IndianaTechLogo, Bitmap MiamiLogo, Bitmap MtUnionLogo, Bitmap PennStateLogo, Bitmap PurdueLogo, Bitmap TrineLogo, Bitmap WriteStateLogo, bool generateFiles)
    {
        bool result = false;
        try
        {
            if (generateFiles)
            {
                //Clear anything in the folder of the working path
                DeleteFolderContents(streamWorkingPath);
                //Console.WriteLine("Folder contents deleted successfully.");
            }
            else
            {
                try
                {
                    //Clear anything in the folder of the working path
                    DeleteFolderContents(streamWorkingPath);
                    //Console.WriteLine("Folder contents deleted successfully.");
                } catch (Exception exc)
                {

                }
            }

            //Create the folder for images and populate it with two images
            string newFolderPath = streamWorkingPath + "\\CurrentTeamInfo";
            Directory.CreateDirectory(newFolderPath);
            //Console.WriteLine($"New folder created: {newFolderPath}");

            // Save the Bitmap as a PNG
            string imagePath = newFolderPath + "\\TeamLeft.png";
            team1Placeholder.Save(imagePath, ImageFormat.Png);
            //Console.WriteLine($"Image saved successfully: {imagePath}");
            imagePath = newFolderPath + "\\TeamRight.png";
            team2Placeholder.Save(imagePath, ImageFormat.Png);
            //Console.WriteLine($"Image saved successfully: {imagePath}");

            //Creating the text files that contain the names of the teams facing each other
            string textFilePath = newFolderPath + "\\RightTeamName.txt";
            string textContent = "ND";
            File.WriteAllText(textFilePath, textContent);

            textFilePath = newFolderPath + "\\LeftTeamName.txt";
            textContent = "ONU";
            File.WriteAllText(textFilePath, textContent);

            //Createing the text files for the scores
            textFilePath = newFolderPath + "\\RightTeamScore.txt";
            textContent = "0";
            File.WriteAllText(textFilePath, textContent);

            textFilePath = newFolderPath + "\\LeftTeamScore.txt";
            textContent = "0";
            File.WriteAllText(textFilePath, textContent);

            //Creating the folder for the current game information
            newFolderPath = streamWorkingPath + "\\CurrentGameInfo";
            Directory.CreateDirectory(newFolderPath);
            //Console.WriteLine($"New folder created: {newFolderPath}");

            //Create the text file for the current quarter
            textFilePath = newFolderPath + "\\Quarter.txt";
            textContent = "1st";
            File.WriteAllText(textFilePath, textContent);

            //Creating the text file for the game clock
            textFilePath = newFolderPath + "\\GameClock.txt";
            textContent = "15:00";
            File.WriteAllText(textFilePath, textContent);

            //Creating the down and yardage file
            textFilePath = newFolderPath + "\\DownAndYard.txt";
            textContent = "1st & 10";
            File.WriteAllText(textFilePath, textContent);

            //Creaeting the files for the msc resources
            newFolderPath = streamWorkingPath + "\\MSCResources";
            Directory.CreateDirectory (newFolderPath);
            //Console.WriteLine($"New folder created: {newFolderPath}");

            imagePath = newFolderPath + "\\Hederman.png";
            hederman.Save(imagePath, ImageFormat.Png);
            //Console.WriteLine($"Image saved successfully: {imagePath}");

            imagePath = newFolderPath + "\\CRFCLogo.png";
            CRFCLogo.Save(imagePath, ImageFormat.Png);
            //Console.WriteLine($"Image saved successfully: {imagePath}");

            imagePath = newFolderPath + "\\BackgroundOverlay.png";
            BackgroundOverlay.Save(imagePath, ImageFormat.Png);
            //Console.WriteLine($"Image saved successfully: {imagePath}");

            imagePath = newFolderPath + "\\BlackSquare.png";
            BlackSquare.Save(imagePath, ImageFormat.Png);
            //Console.WriteLine($"Image saved successfully: {imagePath}");

            textFilePath = newFolderPath + "\\ScrollText.txt";
            textContent = "";
            File.WriteAllText(textFilePath, textContent);

            //Creating the OBS Text File
            textFilePath = streamWorkingPath + "\\GameClockTimerScript.lua";
            textContent = OBSFileContent;
            File.WriteAllText(textFilePath, textContent);

            if (generateFiles)
            {
                //Creating the advertisements
                newFolderPath = streamWorkingPath + "\\Advertisements";
                Directory.CreateDirectory(newFolderPath);
                SaveEmbeddedVideo("MainStream_Control_Project.Advertisements.MISUMI_1.mp4", streamWorkingPath + "\\Advertisements\\MISUMI_1.mp4");
                SaveEmbeddedVideo("MainStream_Control_Project.Advertisements.MISUMI_2.mp4", streamWorkingPath + "\\Advertisements\\MISUMI_2.mp4");

                //Creating the background Music
                newFolderPath = streamWorkingPath + "\\Music";
                Directory.CreateDirectory(newFolderPath);
                SaveEmbeddedVideo("MainStream_Control_Project.BackgroundMusic.BackgroundMusic.mp4", streamWorkingPath + "\\Music\\BackgroundMusic.mp4");
            }
            result = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in setupStreamFolderStructure: {ex.Message}");
        }
        return result;
    }

    static void DeleteFolderContents(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Folder does not exist.");
            return;
        }

        // Delete all files
        foreach (string file in Directory.GetFiles(folderPath))
        {
            File.Delete(file);
        }

        // Delete all subdirectories
        foreach (string subDirectory in Directory.GetDirectories(folderPath))
        {
            Directory.Delete(subDirectory, true);
        }
    }

    static Bitmap LoadEmbeddedImage(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new Exception($"Resource {resourceName} not found! Check namespace and file name.");
            }
            return new Bitmap(stream);
        }
    }

    static string GetEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    static string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "No IPv4 Address Found";
    }

    static async Task<string> GetPublicIPv4Async()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                return await client.GetStringAsync("https://api64.ipify.org");
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }

    static string getTeamAbbrev(string inputString)
    {
        if (inputString.Contains("Notre Dame"))
        {
            inputString = "ND";
        }
        else if (inputString.Contains("Nav"))
        {
            inputString = "NAV";
        }
        else if (inputString.Contains("Ohio Northern"))
        {
            inputString = "ONU";
        }
        else if (inputString.Contains("Valp"))
        {
            inputString = "VAL";
        }
        else if (inputString.Contains("Calvin"))
        {
            inputString = "CAL";
        }
        else if (inputString.Contains("Catholic"))
        {
            inputString = "CTH";
        }
        else if (inputString.Contains("Evans"))
        {
            inputString = "EVA";
        }
        else if (inputString.Contains("Howard"))
        {
            inputString = "HWD";
        }
        else if (inputString.Contains("Indiana"))
        {
            inputString = "IND";
        }
        else if (inputString.Contains("Miami"))
        {
            inputString = "MIA";
        }
        else if (inputString.Contains("Union"))
        {
            inputString = "MTU";
        }
        else if (inputString.Contains("Penn"))
        {
            inputString = "PNS";
        }
        else if (inputString.Contains("Purdue"))
        {
            inputString = "PDU";
        }
        else if (inputString.Contains("Trine"))
        {
            inputString = "TRN";
        }
        else if (inputString.Contains("Wright"))
        {
            inputString = "WRT";
        }
        return inputString;
    }

    static Bitmap getTeamLogo(string inputstring)
    {
        Bitmap teamLogo = ONULogo;
        if (inputstring.Contains("Notre Dame"))
        {
            teamLogo = NDLogo;
        }
        else if (inputstring.Contains("Nav"))
        {
            teamLogo = NavyLogo;
        }
        else if (inputstring.Contains("Ohio Northern"))
        {
            teamLogo = ONULogo;
        }
        else if (inputstring.Contains("Valp"))
        {
            teamLogo = ValpoLogo;
        }
        else if (inputstring.Contains("Calvin"))
        {
            teamLogo = CalvinLogo;
        }
        else if (inputstring.Contains("Catholic"))
        {
            teamLogo = CatholicLogo;
        }
        else if (inputstring.Contains("Evans"))
        {
            teamLogo = EvansvilleLogo;
        }
        else if (inputstring.Contains("Howard"))
        {
            teamLogo = HowardLogo;
        }
        else if (inputstring.Contains("Indiana"))
        {
            teamLogo = IndianaTechLogo;
        }
        else if (inputstring.Contains("Miami"))
        {
            teamLogo = MiamiLogo;
        }
        else if (inputstring.Contains("Union"))
        {
            teamLogo = MtUnionLogo;
        }
        else if (inputstring.Contains("Penn"))
        {
            teamLogo = PennStateLogo;
        }
        else if (inputstring.Contains("Purdue"))
        {
            teamLogo = PennStateLogo;
        }
        else if (inputstring.Contains("Trine"))
        {
            teamLogo = TrineLogo;
        }
        else if (inputstring.Contains("Wright"))
        {
            teamLogo = WriteStateLogo;
        }
        return teamLogo;
    }

    static void SaveEmbeddedVideo(string resourceName, string outputFilePath)
    {
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new Exception("Embedded video not found: " + resourceName);

            using (var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }

}

