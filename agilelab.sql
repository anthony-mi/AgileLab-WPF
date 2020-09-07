-- phpMyAdmin SQL Dump
-- version 4.8.3
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1:3306
-- Generation Time: Feb 02, 2020 at 05:41 PM
-- Server version: 5.6.41
-- PHP Version: 5.6.38

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `agilelab`
--

-- --------------------------------------------------------

--
-- Table structure for table `backlogs`
--

CREATE TABLE `backlogs` (
  `Id` int(11) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `backlogs`
--

INSERT INTO `backlogs` (`Id`) VALUES
(1),
(2),
(3),
(4),
(5),
(6),
(7),
(8),
(9),
(10),
(11),
(12),
(13),
(14),
(15),
(16),
(17),
(18),
(19),
(20),
(21),
(22),
(23),
(24),
(25),
(26),
(27),
(28),
(29),
(30),
(31),
(32),
(33),
(34),
(35),
(36),
(37),
(38),
(39),
(40),
(41),
(42),
(43),
(44),
(45),
(46),
(47),
(48),
(49),
(50),
(51);

-- --------------------------------------------------------

--
-- Table structure for table `projects`
--

CREATE TABLE `projects` (
  `Id` int(11) UNSIGNED NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Manager` int(10) UNSIGNED NOT NULL,
  `DevelopmentTeamId` int(10) UNSIGNED NOT NULL,
  `ProductBacklogId` int(11) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `projects`
--

INSERT INTO `projects` (`Id`, `Name`, `Manager`, `DevelopmentTeamId`, `ProductBacklogId`) VALUES
(1, 'System R', 7, 2, 1),
(2, 'Ingres', 7, 2, 2),
(3, 'Actian X', 7, 2, 3),
(4, 'AgileLab', 7, 2, 4),
(5, 'Project A', 1, 3, 5),
(6, 'Project B', 1, 3, 6),
(10, 'Project A', 2, 6, 10),
(11, 'Project B', 2, 6, 11),
(12, 'Project C', 2, 6, 12),
(13, 'Some Testers Project', 2, 4, 13),
(14, 'Brain Out', 2, 7, 14),
(15, 'Project A', 3, 1, 15),
(16, 'Project B', 3, 1, 16),
(17, 'Project C', 3, 1, 17),
(18, 'Project C', 3, 8, 18),
(19, 'Project A', 3, 9, 19),
(20, 'Project A', 3, 10, 20),
(21, 'Project A', 3, 11, 21),
(22, 'Project A', 3, 12, 22),
(23, 'Project B', 3, 12, 23),
(24, 'Project C', 3, 12, 24),
(25, 'Project A', 3, 15, 25),
(26, 'Project B', 3, 15, 26),
(27, 'Project A', 3, 16, 27),
(28, 'Project B', 3, 16, 28),
(29, 'Project C', 3, 16, 29),
(30, 'Project D', 3, 16, 30),
(31, 'Project E', 3, 16, 31),
(32, 'Project F', 3, 16, 32),
(33, 'Project G', 3, 16, 33),
(34, 'Project H', 3, 16, 34),
(35, 'Project I', 3, 16, 35),
(36, 'Project J', 3, 16, 36),
(37, 'Project A', 3, 17, 37),
(38, 'Project B', 3, 17, 38),
(39, 'Project A', 3, 18, 39),
(41, 'Project X', 15, 2, 50);

-- --------------------------------------------------------

--
-- Table structure for table `roles`
--

CREATE TABLE `roles` (
  `Id` int(10) UNSIGNED NOT NULL,
  `Name` varchar(128) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `sprint`
--

CREATE TABLE `sprint` (
  `Id` int(10) UNSIGNED NOT NULL,
  `MainGoal` text,
  `StartDate` date NOT NULL,
  `FinishDate` date NOT NULL,
  `ProjectId` int(11) UNSIGNED NOT NULL,
  `BacklogId` int(11) UNSIGNED NOT NULL,
  `FinishedBeforeTheApointedDate` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `sprint`
--

INSERT INTO `sprint` (`Id`, `MainGoal`, `StartDate`, `FinishDate`, `ProjectId`, `BacklogId`, `FinishedBeforeTheApointedDate`) VALUES
(1, 'Sprint #1 main goal', '2020-01-02', '2020-01-31', 4, 40, 1),
(2, 'Sprint_2 main goal', '2020-01-22', '2020-01-31', 4, 41, 1),
(3, 'Sprint_3 main goal', '2020-01-30', '2020-01-31', 4, 42, 1),
(4, 'Extend application functionality', '2020-01-27', '2020-02-07', 4, 43, 1),
(5, 'Building a graphical shell', '2020-01-30', '2020-02-07', 2, 44, 1),
(6, 'Building a graphical shell', '2020-01-30', '2020-02-14', 2, 45, 0),
(10, 'Bugs fixing', '2020-02-02', '2020-02-14', 4, 51, 0);

-- --------------------------------------------------------

--
-- Table structure for table `stories`
--

CREATE TABLE `stories` (
  `Id` int(10) UNSIGNED NOT NULL,
  `Name` text NOT NULL,
  `Importance` int(10) UNSIGNED NOT NULL,
  `InitialEstimate` int(10) UNSIGNED DEFAULT NULL,
  `HowToDemo` text,
  `Notes` text,
  `Status` int(10) UNSIGNED NOT NULL,
  `ExecutorId` int(10) UNSIGNED DEFAULT NULL,
  `BacklogId` int(11) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `stories`
--

INSERT INTO `stories` (`Id`, `Name`, `Importance`, `InitialEstimate`, `HowToDemo`, `Notes`, `Status`, `ExecutorId`, `BacklogId`) VALUES
(2, 'Users authorization', 49, NULL, '', 'Authorization data: username and password. App should be able to save authorization data.', 3, NULL, 4),
(3, 'Teams', 48, NULL, '', 'User should be able to create teams, join other teams and leave them. Implement searching for teams by name.', 3, NULL, 4),
(4, 'Projects', 47, NULL, '', 'User should be able to create new project and select it as current.', 3, NULL, 4),
(5, 'Product backlog', 46, NULL, '', 'Implement creation of tasks for project development team. Task data: name, imortance, status (waiting for execution, in progress and completed), notes.', 3, NULL, 4),
(6, 'Sprint', 45, NULL, '', 'User should be able to create sprints. The Sprint should include the main goal, start date, and end date. Visualize the start and end dates of the sprint as a progress. Add manual sprint finishing functionality.', 3, NULL, 4),
(7, 'All user tasks', 44, NULL, '', 'Show to user all tasks that he must complete to deadline date.', 3, NULL, 4),
(8, 'Project review', 43, NULL, '', 'Show to user statistical data about selected project (count of sprints, tasks in progress, completed tasks, completed story points etc).', 3, NULL, 4),
(9, 'Update project view', 42, NULL, '', 'Display project review in projects view (text inside tiles).', 2, NULL, 4),
(10, '[User story for statistic] #1', 1, 1, '', '', 1, NULL, 40),
(11, '[User story for statistic] #2', 1, 1, '', '', 3, 12, 40),
(12, '[User story for statistic] #3', 1, 1, '', '', 2, 12, 40),
(13, '[User story for statistic] #4', 1, 1, '', '', 1, NULL, 40),
(14, '[User story for statistic] #5', 1, 1, '', '', 3, 12, 40),
(15, '[User story for statistic] #6', 1, 1, '', '', 1, NULL, 40),
(16, '[User story for statistic] #7', 1, 8, '', '', 3, 12, 40),
(17, '[User story for statistic] #8', 3, 12, '', '', 2, 12, 40),
(18, '[User story for statistic] #9', 4, 7, '', '', 3, 12, 40),
(19, '[User story for statistic] #1', 1, 1, '', '', 3, 12, 41),
(20, '[User story for statistic] #2', 1, 2, '', '', 1, NULL, 41),
(21, '[User story for statistic] #3', 1, 3, '', '', 2, 12, 41),
(22, '[User story for statistic] #4', 1, 4, '', '', 3, 12, 41),
(23, '[User story for statistic] #5', 1, 5, '', '', 3, 12, 41),
(24, '[User story for statistic] #6', 1, 6, '', '', 1, NULL, 41),
(25, '[User story for statistic] #1', 1, 1, '', '', 3, 12, 42),
(26, '[User story for statistic] #2', 1, 2, '', '', 1, NULL, 42),
(27, '[User story for statistic] #3', 1, 3, '', '', 3, 12, 42),
(28, '[User story for statistic] #4', 1, 4, '', '', 1, NULL, 42),
(29, '[User story for statistic] #5', 1, 5, '', '', 2, 12, 42),
(30, '[User story for statistic] #6', 1, 6, '', '', 3, 12, 42),
(31, 'Add project search by name', 24, 1, 'Type part of the name of the target project', 'Search by lower case', 3, 15, 43),
(32, 'Show current team/project in title bar', 23, 1, 'Select team and project than it must be shown in title bar', '', 3, 11, 43),
(35, 'Refactor dialogs', 1, 3, '', 'Cut dialogs code from view source code to new user controls.', 2, 12, 43),
(36, 'Save current team and project to registry', 29, 1, 'Check \"Remember me\" before logout. Select team and project. Close app. Check out app registry keys. Run app and look at selected project and team.', '*if user checked \'Remember me\'', 3, 15, 43),
(37, 'Add the ability to prematurely finish the sprint', 30, 4, '', 'Create button \'Finish\' in Sprint View', 3, 12, 43),
(38, 'Display project review in projects user control', 25, 6, '', 'Change layout of tile', 1, NULL, 43),
(39, 'Convert literals associated with the OnPropertyChanged event to reflection', 1, 1, '', '[Refactoring]', 2, 12, 43),
(40, 'Create project documentation view', 28, 40, '', 'Add new property to project entity (documentation).  Implement simple text editor with text formating. Add it as new view.', 1, NULL, 43),
(41, 'Add user story creation datetime', 26, 2, '', '', 2, 11, 43),
(42, 'Add sprints statistics in project review', 27, 18, 'Create few sprints and fill them by some statistical data. Go to project review and select sprints.', 'Visualize in graphs', 2, 12, 43),
(46, 'Stories hashtagging', 41, NULL, '', 'Simplify searching tasks using hashtagging system. Every user can add hashtags to user story to mark which modules it affects.', 2, NULL, 4),
(48, 'Display team statistical data in \"Your team\" user control', 20, 4, '', 'Change layout of tile', 1, NULL, 43),
(49, 'Lorem ipsum', 1, 1, '', '', 1, 15, 45);

-- --------------------------------------------------------

--
-- Table structure for table `storystatuses`
--

CREATE TABLE `storystatuses` (
  `Id` int(10) UNSIGNED NOT NULL,
  `Name` varchar(128) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `storystatuses`
--

INSERT INTO `storystatuses` (`Id`, `Name`) VALUES
(1, 'Waiting for executor'),
(2, 'In progress'),
(3, 'Completed');

-- --------------------------------------------------------

--
-- Table structure for table `teammembers`
--

CREATE TABLE `teammembers` (
  `Id` int(10) UNSIGNED NOT NULL,
  `TeamId` int(10) UNSIGNED NOT NULL,
  `UserId` int(10) UNSIGNED NOT NULL,
  `RoleId` int(10) UNSIGNED DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `teammembers`
--

INSERT INTO `teammembers` (`Id`, `TeamId`, `UserId`, `RoleId`) VALUES
(1, 1, 7, NULL),
(2, 2, 7, NULL),
(3, 2, 1, NULL),
(4, 3, 1, NULL),
(5, 4, 1, NULL),
(6, 5, 1, NULL),
(7, 6, 1, NULL),
(8, 2, 2, NULL),
(9, 3, 2, NULL),
(10, 4, 2, NULL),
(11, 6, 2, NULL),
(12, 7, 2, NULL),
(13, 2, 3, NULL),
(14, 1, 3, NULL),
(15, 3, 3, NULL),
(16, 4, 3, NULL),
(17, 8, 3, NULL),
(18, 9, 3, NULL),
(19, 10, 3, NULL),
(20, 11, 3, NULL),
(21, 12, 3, NULL),
(22, 13, 3, NULL),
(23, 14, 3, NULL),
(24, 15, 3, NULL),
(25, 16, 3, NULL),
(26, 17, 3, NULL),
(27, 18, 3, NULL),
(28, 19, 3, NULL),
(29, 16, 4, NULL),
(30, 17, 4, NULL),
(31, 12, 4, NULL),
(32, 18, 4, NULL),
(33, 2, 4, NULL),
(34, 3, 4, NULL),
(35, 7, 4, NULL),
(36, 8, 4, NULL),
(37, 9, 4, NULL),
(38, 6, 4, NULL),
(39, 7, 5, NULL),
(40, 4, 5, NULL),
(41, 9, 5, NULL),
(42, 10, 5, NULL),
(43, 2, 5, NULL),
(44, 6, 5, NULL),
(45, 1, 5, NULL),
(46, 16, 5, NULL),
(47, 15, 5, NULL),
(48, 11, 5, NULL),
(49, 12, 5, NULL),
(50, 18, 5, NULL),
(51, 3, 6, NULL),
(52, 8, 6, NULL),
(53, 9, 6, NULL),
(54, 5, 6, NULL),
(55, 2, 6, NULL),
(56, 6, 6, NULL),
(57, 1, 6, NULL),
(58, 16, 6, NULL),
(59, 15, 6, NULL),
(60, 11, 6, NULL),
(61, 12, 6, NULL),
(62, 17, 6, NULL),
(63, 3, 7, NULL),
(64, 8, 7, NULL),
(65, 9, 7, NULL),
(66, 16, 7, NULL),
(67, 12, 7, NULL),
(68, 7, 8, NULL),
(69, 3, 8, NULL),
(70, 10, 8, NULL),
(71, 6, 8, NULL),
(72, 1, 8, NULL),
(73, 15, 8, NULL),
(74, 16, 8, NULL),
(75, 17, 8, NULL),
(76, 18, 8, NULL),
(77, 12, 8, NULL),
(78, 7, 9, NULL),
(79, 3, 9, NULL),
(80, 10, 9, NULL),
(81, 2, 9, NULL),
(82, 6, 9, NULL),
(83, 11, 9, NULL),
(84, 15, 9, NULL),
(85, 16, 9, NULL),
(86, 17, 9, NULL),
(87, 12, 9, NULL),
(88, 7, 10, NULL),
(89, 3, 10, NULL),
(90, 10, 10, NULL),
(91, 2, 10, NULL),
(92, 1, 10, NULL),
(93, 15, 10, NULL),
(94, 16, 10, NULL),
(95, 17, 10, NULL),
(96, 7, 11, NULL),
(97, 3, 11, NULL),
(98, 8, 11, NULL),
(99, 9, 11, NULL),
(100, 10, 11, NULL),
(101, 2, 11, NULL),
(102, 6, 11, NULL),
(103, 1, 11, NULL),
(104, 11, 11, NULL),
(105, 15, 11, NULL),
(106, 16, 11, NULL),
(107, 17, 11, NULL),
(108, 18, 11, NULL),
(109, 12, 11, NULL),
(110, 19, 11, NULL),
(111, 7, 12, NULL),
(112, 3, 12, NULL),
(113, 4, 12, NULL),
(114, 8, 12, NULL),
(115, 9, 12, NULL),
(116, 10, 12, NULL),
(117, 2, 12, NULL),
(118, 6, 12, NULL),
(119, 1, 12, NULL),
(120, 11, 12, NULL),
(121, 15, 12, NULL),
(122, 12, 12, NULL),
(126, 21, 15, NULL),
(127, 7, 15, NULL),
(128, 2, 15, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `teams`
--

CREATE TABLE `teams` (
  `Id` int(10) UNSIGNED NOT NULL,
  `Name` varchar(128) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `teams`
--

INSERT INTO `teams` (`Id`, `Name`) VALUES
(7, '404 Brain Not Found'),
(4, 'Black Box Testers'),
(3, 'Breakfast Buddies'),
(8, 'BugSquashers'),
(21, 'Dream Team'),
(13, 'Dynamic Developers'),
(9, 'Easy Scrum Easy Go'),
(5, 'Empty Coffee Cups'),
(10, 'Essentials'),
(2, 'IBM R Team'),
(14, 'Ideas R Us'),
(6, 'Int Elligence;'),
(1, 'IT.Pro'),
(11, 'Keep Calm and Sprint On'),
(15, 'Mr Manager and the Rainmakers'),
(16, 'Must Have Caffeine'),
(17, 'Net Surfers'),
(18, 'Nouveau Riche'),
(19, 'Orange Dots'),
(12, 'The A-Team');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `Id` int(10) UNSIGNED NOT NULL,
  `FirstName` varchar(256) NOT NULL,
  `LastName` varchar(256) NOT NULL,
  `UserName` varchar(128) NOT NULL,
  `PasswordHash` varchar(128) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`Id`, `FirstName`, `LastName`, `UserName`, `PasswordHash`) VALUES
(1, 'Michael', 'Abrash', 'mabrash', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OXF6G705asl5ZA6bXeW/ufHwwm.0VP7S'),
(2, 'Andrei', 'Alexandrescu', 'andrescu', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OU4FihHULrBU.MNrRsbqYuKhJ9wfPzA2'),
(3, 'Paul', 'Allen', 'paul53', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OQGjHknFh2KhlJ9iyd7jlRaX0oZyflj6'),
(4, 'Kent', 'Beck', 'kbeck', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OdBdE7mHSNAymD82v1r19sOQeYHc.YLG'),
(5, 'Donald', 'Becker', 'dbecker', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OtI3GeiTVgxKTXkaOkeL86j4qMVwCRy.'),
(6, 'Brian', 'Behlendorf', 'bdorf73', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OS2Fk4zir2NhWqVtVtbIyS0OWBkOSEfC'),
(7, 'Edgar', 'Codd', 'ecodd23', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OsVORRqLJL50VhYKEvvSkfh1PW.jtan.'),
(8, 'Walter', 'Bright', 'wright', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OiZ09ZYLOKNpB0ArtRrOaKPYTVDL78RK'),
(9, 'Alan', 'Cooper', 'alan52', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OseKGwlj5xFjO4bgKIiL4/D8S7/mw212'),
(10, 'Roberta', 'Williams', 'rsierra', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OngYpH/DVMGlPT3o8VipGyMyPZce21fu'),
(11, 'Sophie', 'Wilson', 'sopharm', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2Ormolkhqmx6tGpTFT1c8A/H90M2GHYEW'),
(12, 'Richard', 'Hipp', 'hippard', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OsZg5Gw3GZt5GLPqDgHf2Twf7ac9YQse'),
(15, 'Scott', 'Guthrie', 'scottgu', '$2a$12$VvDRKYKGt4Zd2Ux35LeG2OYKJg65jZafFwseQNkwhTKA7F6FkhZb.');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `backlogs`
--
ALTER TABLE `backlogs`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `projects`
--
ALTER TABLE `projects`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `ProductBacklogId_2` (`ProductBacklogId`),
  ADD KEY `Manager` (`Manager`),
  ADD KEY `DevelopmentTeamId` (`DevelopmentTeamId`),
  ADD KEY `ProductBacklogId` (`ProductBacklogId`),
  ADD KEY `Name` (`Name`) USING BTREE;

--
-- Indexes for table `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Name` (`Name`);

--
-- Indexes for table `sprint`
--
ALTER TABLE `sprint`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `BacklogId` (`BacklogId`),
  ADD KEY `ProjectId` (`ProjectId`);

--
-- Indexes for table `stories`
--
ALTER TABLE `stories`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `Status` (`Status`),
  ADD KEY `ExecutorId` (`ExecutorId`),
  ADD KEY `BacklogId` (`BacklogId`) USING BTREE;

--
-- Indexes for table `storystatuses`
--
ALTER TABLE `storystatuses`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `teammembers`
--
ALTER TABLE `teammembers`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `TeamId` (`TeamId`),
  ADD KEY `teammembers_ibfk_2` (`UserId`),
  ADD KEY `RoleId` (`RoleId`);

--
-- Indexes for table `teams`
--
ALTER TABLE `teams`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `Name` (`Name`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UserName` (`UserName`),
  ADD UNIQUE KEY `UserName_2` (`UserName`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `backlogs`
--
ALTER TABLE `backlogs`
  MODIFY `Id` int(11) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=52;

--
-- AUTO_INCREMENT for table `projects`
--
ALTER TABLE `projects`
  MODIFY `Id` int(11) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=42;

--
-- AUTO_INCREMENT for table `roles`
--
ALTER TABLE `roles`
  MODIFY `Id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `sprint`
--
ALTER TABLE `sprint`
  MODIFY `Id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `stories`
--
ALTER TABLE `stories`
  MODIFY `Id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=50;

--
-- AUTO_INCREMENT for table `storystatuses`
--
ALTER TABLE `storystatuses`
  MODIFY `Id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `teammembers`
--
ALTER TABLE `teammembers`
  MODIFY `Id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=129;

--
-- AUTO_INCREMENT for table `teams`
--
ALTER TABLE `teams`
  MODIFY `Id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=22;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `Id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=16;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `projects`
--
ALTER TABLE `projects`
  ADD CONSTRAINT `projects_ibfk_1` FOREIGN KEY (`Manager`) REFERENCES `users` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `projects_ibfk_3` FOREIGN KEY (`DevelopmentTeamId`) REFERENCES `teams` (`Id`),
  ADD CONSTRAINT `projects_ibfk_4` FOREIGN KEY (`ProductBacklogId`) REFERENCES `backlogs` (`Id`);

--
-- Constraints for table `sprint`
--
ALTER TABLE `sprint`
  ADD CONSTRAINT `sprint_ibfk_1` FOREIGN KEY (`BacklogId`) REFERENCES `backlogs` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `sprint_ibfk_2` FOREIGN KEY (`ProjectId`) REFERENCES `projects` (`Id`);

--
-- Constraints for table `stories`
--
ALTER TABLE `stories`
  ADD CONSTRAINT `stories_ibfk_1` FOREIGN KEY (`Status`) REFERENCES `storystatuses` (`Id`),
  ADD CONSTRAINT `stories_ibfk_2` FOREIGN KEY (`ExecutorId`) REFERENCES `users` (`Id`),
  ADD CONSTRAINT `stories_ibfk_3` FOREIGN KEY (`BacklogId`) REFERENCES `backlogs` (`Id`);

--
-- Constraints for table `teammembers`
--
ALTER TABLE `teammembers`
  ADD CONSTRAINT `teammembers_ibfk_1` FOREIGN KEY (`TeamId`) REFERENCES `teams` (`Id`),
  ADD CONSTRAINT `teammembers_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `teammembers_ibfk_3` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
