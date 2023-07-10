-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Generation Time: Jul 10, 2023 at 11:25 AM
-- Server version: 10.4.28-MariaDB
-- PHP Version: 8.2.4

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `saved_movies`
--

-- --------------------------------------------------------

--
-- Table structure for table `loginDetails`
--

CREATE TABLE `loginDetails` (
  `userID` int(11) NOT NULL,
  `email` varchar(200) NOT NULL,
  `username` varchar(200) NOT NULL,
  `password` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `loginDetails`
--

INSERT INTO `loginDetails` (`userID`, `email`, `username`, `password`) VALUES
(1, 'dsfkB@gmail.com', 'dsfk', 'dsfjkb'),
(2, 'sdkfjb@gmail.com', 'dfkj', 'kdsfjh'),
(3, 'dsfjkb@gmail.com', 'dkjb', 'eksfb'),
(4, 'testing@gmail.com', 'tester', 'testing'),
(5, 'testing2@gmail.com', 'testing2', 'testing'),
(6, 'timmy@gmail.com', 'tim', 'testing');

-- --------------------------------------------------------

--
-- Table structure for table `movies`
--

CREATE TABLE `movies` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `price` double NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `movies`
--

INSERT INTO `movies` (`id`, `name`, `price`) VALUES
(23, 'apple', 1000),
(25, 'biscuit ', 2),
(26, 'another', 20),
(27, 'apple', 20);

-- --------------------------------------------------------

--
-- Table structure for table `savedMovies`
--

CREATE TABLE `savedMovies` (
  `id` int(11) NOT NULL,
  `title` varchar(300) NOT NULL,
  `overview` varchar(1000) NOT NULL,
  `poster` varchar(900) NOT NULL DEFAULT '',
  `dateTimeInsertion` datetime DEFAULT NULL,
  `userID` int(11) DEFAULT NULL,
  `Rating` double NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `savedMovies`
--

INSERT INTO `savedMovies` (`id`, `title`, `overview`, `poster`, `dateTimeInsertion`, `userID`, `Rating`) VALUES
(169, 'Nimona', 'A knight framed for a tragic crime teams with a scrappy, shape-shifting teen to prove his innocence. But what if she\'s the monster he\'s sworn to destroy?', 'https://image.tmdb.org/t/p/w185/2NQljeavtfl22207D1kxLpa4LS3.jpg', '2023-07-03 21:56:27', 4, 7.9),
(170, 'The Witcher', 'Geralt of Rivia, a mutated monster-hunter for hire, journeys toward his destiny in a turbulent world where people often prove more wicked than beasts.', 'https://image.tmdb.org/t/p/w185/cZ0d3rtvXPVvuiX22sP79K3Hmjz.jpg', '2023-07-03 21:57:55', 4, 8.2);

-- --------------------------------------------------------

--
-- Table structure for table `tmpMoviePoster`
--

CREATE TABLE `tmpMoviePoster` (
  `id` int(11) NOT NULL,
  `poster` varchar(255) NOT NULL,
  `dominantColor` varchar(7) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `tmpMoviePoster`
--

INSERT INTO `tmpMoviePoster` (`id`, `poster`, `dominantColor`) VALUES
(132, 'https://image.tmdb.org/t/p/original/2EewmxXe72ogD0EaWM8gqa0ccIw.jpg', '#E61A19');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `loginDetails`
--
ALTER TABLE `loginDetails`
  ADD PRIMARY KEY (`userID`);

--
-- Indexes for table `movies`
--
ALTER TABLE `movies`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `savedMovies`
--
ALTER TABLE `savedMovies`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_savedMovies_loginDetails` (`userID`);

--
-- Indexes for table `tmpMoviePoster`
--
ALTER TABLE `tmpMoviePoster`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `loginDetails`
--
ALTER TABLE `loginDetails`
  MODIFY `userID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `movies`
--
ALTER TABLE `movies`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=28;

--
-- AUTO_INCREMENT for table `savedMovies`
--
ALTER TABLE `savedMovies`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=172;

--
-- AUTO_INCREMENT for table `tmpMoviePoster`
--
ALTER TABLE `tmpMoviePoster`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=133;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `savedMovies`
--
ALTER TABLE `savedMovies`
  ADD CONSTRAINT `fk_savedMovies_loginDetails` FOREIGN KEY (`userID`) REFERENCES `loginDetails` (`userID`) ON DELETE CASCADE ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
