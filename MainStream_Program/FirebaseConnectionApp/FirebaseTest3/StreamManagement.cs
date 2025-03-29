using System.Data;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Drawing;

public class StreamManagement
{
    //Add any public values to control here
    Bitmap team1Image;
    Bitmap team2Image;
    string streamWorkingPath = "";

    //Constructor
    public StreamManagement(Bitmap team1Image, Bitmap team2Image, string streamWorkingPath)
    {
        this.team1Image = team1Image;
        this.team2Image = team2Image;
        this.streamWorkingPath = streamWorkingPath;
    }

    public bool updateTeamImages(Bitmap team1Image, Bitmap team2Image)
    {
        //Now the team names are set from the database
        this.team1Image = team1Image;
        this.team2Image = team2Image;

        //Now save these to the folder so that we can update the stream display
        updateStreamImageDisplay();

        //Save the images of the teams to 
        return true;
    }

    private void updateStreamImageDisplay()
    {

    }
}

