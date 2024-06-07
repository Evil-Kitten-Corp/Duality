<?php
	include_once("db.php");

	if (isset($_POST["username"]) && !empty($_POST["username"]) &&
		isset($_POST["password"]) && !empty($_POST["password"])){

		Signup($_POST["username"], $_POST["password"]);
	}

	function Signup($username, $password){
		GLOBAL $con;

		$sql = "SELECT username FROM users WHERE username=?";
		$st = $con->prepare($sql);
		$st->execute(array($username));
		$all = $st->fetchAll();

		if (count($all) > 0){
			echo "SERVER: error, username already exists";
			exit();
		}

		$sql = "INSERT INTO users (username, password) VALUES (?, ?)";
		$st = $con->prepare($sql);
		$st->execute(array($username, sha1($password)));

		echo "SERVER: user created successfully";
		exit();
	}

	echo "SERVER: error, invalid username or password";
?>
