import javafx.application.Application;
import javafx.embed.swing.SwingFXUtils;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.image.Image;
import javafx.stage.Stage;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.util.Objects;

public class Main extends Application {

    public Stage stage;

    @Override
    public void start(Stage stage) {
        try {
            this.stage = stage;
            Parent root = FXMLLoader.load(Objects.requireNonNull(getClass().getResource("Main.fxml")));
            stage.setTitle("CRFC Overlay");
            //BufferedImage Image = ImageIO.read(Objects.requireNonNull(getClass().getResource("/Resources/Logos/SEAM 32x32.png")));
            //Image test = SwingFXUtils.toFXImage(Image, null);
            //stage.getIcons().add(test);
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