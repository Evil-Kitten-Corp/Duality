<?php
$dbFile = 'D:/UnityProjects/Duality/___PhpServer_www/unity/api/unity.db';

try {
    $con = new PDO("sqlite:" . $dbFile);
    $con->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    echo "Connection successful!";
} catch (PDOException $e) {
    echo "Connection failed: " . $e->getMessage();
}
?>

