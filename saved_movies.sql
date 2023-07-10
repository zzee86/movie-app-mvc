-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Generation Time: Jul 10, 2023 at 07:21 PM
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
-- RELATIONSHIPS FOR TABLE `loginDetails`:
--

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
-- RELATIONSHIPS FOR TABLE `savedMovies`:
--   `userID`
--       `loginDetails` -> `userID`
--

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
-- RELATIONSHIPS FOR TABLE `tmpMoviePoster`:
--

--
-- Indexes for dumped tables
--

--
-- Indexes for table `loginDetails`
--
ALTER TABLE `loginDetails`
  ADD PRIMARY KEY (`userID`);

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
  MODIFY `userID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `savedMovies`
--
ALTER TABLE `savedMovies`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `tmpMoviePoster`
--
ALTER TABLE `tmpMoviePoster`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

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
