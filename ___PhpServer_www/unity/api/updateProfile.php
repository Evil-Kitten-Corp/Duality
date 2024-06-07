<?php
    include_once("db.php");

    if (isset($_POST["username"]) && !empty($_POST["username"]))
    {
        $username = $_POST["username"];

        // Increment values based on what is sent
        $matchesPlayedIncrement = isset($_POST["matches_played_increment"]) ? (int)$_POST["matches_played_increment"] : 0;
        $winsIncrement = isset($_POST["wins_increment"]) ? (int)$_POST["wins_increment"] : 0;
        $lossesIncrement = isset($_POST["losses_increment"]) ? (int)$_POST["losses_increment"] : 0;
        $totalScoreIncrement = isset($_POST["total_score_increment"]) ? (int)$_POST["total_score_increment"] : 0;
    
        // Prepare the SQL statement
        $sql = "UPDATE users SET
                    matches_played = matches_played + ?,
                    wins = wins + ?,
                    losses = losses + ?,
                    total_score = total_score + ?
                WHERE username = ?";
        $st = $con->prepare($sql);
        $st->execute(array($matchesPlayedIncrement, $winsIncrement, $lossesIncrement, $totalScoreIncrement, $username));

        echo "SERVER: profile updated successfully";
        exit();
    }

    echo "SERVER: error, invalid data";
?>
