import javafx.event.ActionEvent;
import javafx.scene.control.TextField;

import java.io.FileWriter;
import java.io.IOException;

public class Controller {
    public TextField LTeamId;
    public TextField RTeamId;
    public TextField LScoreId;
    public TextField RScoreId;
    public TextField NewsId;

    int LeftScore;
    int RightScore;

    String LeftTeamName;
    String RightTeamName;
    String News;

    //applies typed team names into field
    public void Submit(ActionEvent actionEvent) throws IOException {

        LeftTeamName = LTeamId.getText();

        FileWriter LeftWriter = new FileWriter("TextFiles/LeftName.txt");
        LeftWriter.write(String.valueOf(LeftTeamName));
        LeftWriter.close();

        RightTeamName = RTeamId.getText();

        FileWriter RightWriter = new FileWriter("TextFiles/RightName.txt");
        RightWriter.write(String.valueOf(RightTeamName));
        RightWriter.close();
    }

    //
    public void LScore(ActionEvent actionEvent) throws IOException {
        LeftScore = Integer.parseInt(LScoreId.getText());
        LScoreId.setText(String.valueOf(LeftScore));

        FileWriter LScore = new FileWriter("TextFiles/LeftScore.txt");
        LScore.write(String.valueOf(LeftScore));
        LScore.close();
    }

    public void RScore(ActionEvent actionEvent) throws IOException {
        RightScore = Integer.parseInt(RScoreId.getText());
        RScoreId.setText(String.valueOf(RightScore));

        FileWriter RScore = new FileWriter("TextFiles/RightScore.txt");
        RScore.write(String.valueOf(RightScore));
        RScore.close();
    }

    public void L6(ActionEvent actionEvent) throws IOException {
        LeftScore = LeftScore + 6;
        LScoreId.setText(String.valueOf(LeftScore));

        FileWriter LeftScoreWriter = new FileWriter("TextFiles/LeftScore.txt");
        LeftScoreWriter.write(String.valueOf(LeftScore));
        LeftScoreWriter.close();
    }

    public void L3(ActionEvent actionEvent) throws IOException {
        LeftScore = LeftScore + 3;
        LScoreId.setText(String.valueOf(LeftScore));

        FileWriter LeftScoreWriter = new FileWriter("TextFiles/LeftScore.txt");
        LeftScoreWriter.write(String.valueOf(LeftScore));
        LeftScoreWriter.close();
    }

    public void L2(ActionEvent actionEvent) throws IOException {
        LeftScore = LeftScore + 2;
        LScoreId.setText(String.valueOf(LeftScore));

        FileWriter LeftScoreWriter = new FileWriter("TextFiles/LeftScore.txt");
        LeftScoreWriter.write(String.valueOf(LeftScore));
        LeftScoreWriter.close();
    }

    public void L1(ActionEvent actionEvent) throws IOException {
        LeftScore = LeftScore + 1;
        LScoreId.setText(String.valueOf(LeftScore));

        FileWriter LeftScoreWriter = new FileWriter("TextFiles/LeftScore.txt");
        LeftScoreWriter.write(String.valueOf(LeftScore));
        LeftScoreWriter.close();
    }

    public void R6(ActionEvent actionEvent) throws IOException {
        RightScore = RightScore + 6;
        RScoreId.setText(String.valueOf(RightScore));

        FileWriter RightScoreWriter = new FileWriter("TextFiles/RightScore.txt");
        RightScoreWriter.write(String.valueOf(RightScore));
        RightScoreWriter.close();
    }

    public void R3(ActionEvent actionEvent) throws IOException {
        RightScore = RightScore + 3;
        RScoreId.setText(String.valueOf(RightScore));

        FileWriter RightScoreWriter = new FileWriter("TextFiles/RightScore.txt");
        RightScoreWriter.write(String.valueOf(RightScore));
        RightScoreWriter.close();
    }

    public void R2(ActionEvent actionEvent) throws IOException {
        RightScore = RightScore + 2;
        RScoreId.setText(String.valueOf(RightScore));

        FileWriter RightScoreWriter = new FileWriter("TextFiles/RightScore.txt");
        RightScoreWriter.write(String.valueOf(RightScore));
        RightScoreWriter.close();
    }

    public void R1(ActionEvent actionEvent) throws IOException {
        RightScore = RightScore + 1;
        RScoreId.setText(String.valueOf(RightScore));

        FileWriter RightScoreWriter = new FileWriter("TextFiles/RightScore.txt");
        RightScoreWriter.write(String.valueOf(RightScore));
        RightScoreWriter.close();
    }

    //sets the half to 1
    public void H1(ActionEvent actionEvent) throws IOException {
        FileWriter H1 = new FileWriter("TextFiles/Half.txt");
        H1.write("1st Half");
        H1.close();
    }

    //sets the half to 2
    public void H2(ActionEvent actionEvent) throws IOException {
        FileWriter H2 = new FileWriter("TextFiles/Half.txt");
        H2.write("2nd Half");
        H2.close();
    }

    public void TimeOut(ActionEvent actionEvent) throws IOException {
        FileWriter MatchETA = new FileWriter("TextFiles/ETA.txt");
        MatchETA.write("Time Out");
        MatchETA.close();
    }

    public void InProgress(ActionEvent actionEvent) throws IOException {
        FileWriter MatchETA = new FileWriter("TextFiles/ETA.txt");
        MatchETA.write("In Progress");
        MatchETA.close();
    }

    public void HalfTime(ActionEvent actionEvent) throws IOException {
        FileWriter MatchETA = new FileWriter("TextFiles/ETA.txt");
        MatchETA.write("Half Time");
        MatchETA.close();
    }

    public void EndGame(ActionEvent actionEvent) throws IOException {
        FileWriter MatchETA = new FileWriter("TextFiles/ETA.txt");
        MatchETA.write("End Game");
        MatchETA.close();
    }

    public void News(ActionEvent actionEvent) throws IOException {
        News = "          NEWS: " +NewsId.getText();
        FileWriter NewsWriter = new FileWriter("TextFiles/News.txt");
        NewsWriter.write(String.valueOf(News));
        NewsWriter.close();
    }

    public void Reset(ActionEvent actionEvent) throws IOException {
        //resets half
        FileWriter H2 = new FileWriter("TextFiles/Half.txt");
        H2.write("");
        H2.close();

        //resets right score
        RScoreId.setText("");
        FileWriter RightScoreWriter = new FileWriter("TextFiles/RightScore.txt");
        RightScoreWriter.write(" ");
        RightScoreWriter.close();
        RightScore=0;

        //resets left score
        LScoreId.setText("");
        FileWriter LeftScoreWriter = new FileWriter("TextFiles/LeftScore.txt");
        LeftScoreWriter.write(" ");
        LeftScoreWriter.close();
        LeftScore=0;

        //resets left team name
        LeftTeamName = LTeamId.getText();
        LTeamId.setText("");
        FileWriter LeftWriter = new FileWriter("TextFiles/LeftName.txt");
        LeftWriter.write("");
        LeftWriter.close();

        //resets right team name
        RightTeamName = RTeamId.getText();
        RTeamId.setText("");
        FileWriter RightWriter = new FileWriter("TextFiles/RightName.txt");
        RightWriter.write("");
        RightWriter.close();

        FileWriter MatchETA = new FileWriter("TextFiles/ETA.txt");
        MatchETA.write("");
        MatchETA.close();
    }


}
