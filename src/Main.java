import javafx.application.Application;
import javafx.embed.swing.SwingFXUtils;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.image.Image;
import javafx.stage.Stage;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Objects;

public class Main extends Application {

    public Stage stage;

    @Override
    public void start(Stage stage) throws IOException {
        try {
            this.stage = stage;
            Parent root = FXMLLoader.load(Objects.requireNonNull(getClass().getResource("Main.fxml")));
            stage.setTitle("CRFC Overlay");
            //BufferedImage Image = ImageIO.read(Objects.requireNonNull(getClass().getResource("TextFiles/Hederman_64x64.jpg")));
            //Image test = SwingFXUtils.toFXImage(Image, null);
            //stage.getIcons().add(test);
            stage.getIcons().add(new Image("/TextFiles/Hederman_64x64.jpg"));
            Scene scene = new Scene(root);
            stage.setScene(scene);
            stage.setResizable(false);
            //stage.setHeight(650);
            stage.setWidth(532);
            stage.show();

        } catch(Exception e){
            e.printStackTrace();
        }

    }

    public static void main(String[] args) {
        launch(args);
    }
}