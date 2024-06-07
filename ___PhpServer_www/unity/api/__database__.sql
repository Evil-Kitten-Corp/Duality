-- Table structure for table `users`
DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password` varchar(200) NOT NULL,
  `matches_played` int(11) NOT NULL DEFAULT 0,
  `wins` int(11) NOT NULL DEFAULT 0,
  `losses` int(11) NOT NULL DEFAULT 0,
  `total_score` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`)
);

-- Dumping data for table `users`
INSERT INTO `users` (`username`, `password`, `matches_played`, `wins`, `losses`, `total_score`) VALUES
( 'johndoe', sha1('johndoe'), 0, 0, 0, 0 ),
( 'johnsmith', sha1('johnsmith'), 0, 0, 0, 0 );
COMMIT;

