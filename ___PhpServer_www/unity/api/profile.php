<?php
	include_once("db.php");

	if (isset($_POST["username"]) && !empty($_POST["username"])) {
		GetProfile($_POST["username"]);
	}

	function GetProfile($username) {
		GLOBAL $con;

		$sql = "SELECT matches_played, wins, losses, total_score FROM users WHERE username=?";
		$st = $con->prepare($sql);
		$st->execute(array($username));
		$profile = $st->fetch(PDO::FETCH_ASSOC);

		if ($profile) {
			echo json_encode($profile);
			exit();
		}

		echo "SERVER: error, user not found";
		exit();
	}
?>
