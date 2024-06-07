<?php   
    $host = "mysql:host=localhost;dbname=unity";
    $user = "root";
    $pass = "fairytale1";
    try {
        $con = new PDO($host, $user, $pass);
        $con->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        echo "Connection successful!";
    } catch (PDOException $e) {
        echo "Connection failed: " . $e->getMessage();
    }
?>
