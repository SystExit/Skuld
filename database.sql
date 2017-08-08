SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `skuld`
--

-- --------------------------------------------------------

--
-- Table structure for table `accounts`
--

CREATE TABLE `accounts` (
  `ID` bigint(20) NOT NULL,
  `Username` text NOT NULL,
  `Money` bigint(20) DEFAULT '0',
  `Description` text,
  `Daily` text,
  `LuckFactor` double NOT NULL DEFAULT '1',
  `DMEnabled` tinyint(1) NOT NULL DEFAULT '1',
  `Petted` bigint(20) UNSIGNED NOT NULL DEFAULT '0',
  `Pets` bigint(20) UNSIGNED NOT NULL DEFAULT '0',
  `HP` int(10) UNSIGNED NOT NULL DEFAULT '10000',
  `GlaredAt` bigint(20) UNSIGNED NOT NULL DEFAULT '0',
  `Glares` bigint(20) UNSIGNED NOT NULL DEFAULT '0',
  `PrevCmd` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `commandusage`
--

CREATE TABLE `commandusage` (
  `ID` bigint(20) UNSIGNED NOT NULL,
  `UserID` bigint(20) UNSIGNED NOT NULL,
  `UserUsage` bigint(20) UNSIGNED NOT NULL,
  `Command` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `guild`
--

CREATE TABLE `guild` (
  `ID` bigint(20) UNSIGNED NOT NULL,
  `Name` text NOT NULL,
  `JoinMessage` text,
  `LeaveMessage` text,
  `EventChannel` bigint(20) UNSIGNED DEFAULT NULL,
  `AutoJoinRole` bigint(20) UNSIGNED DEFAULT NULL,
  `Prefix` text,
  `Users` int(11) NOT NULL,
  `LogEnabled` tinyint(1) NOT NULL,
  `JoinableRoles` longtext,
  `TwitchNotifChannel` bigint(20) UNSIGNED DEFAULT NULL,
  `TwitterLogChannel` bigint(20) UNSIGNED DEFAULT NULL,
  `MutedRole` bigint(20) UNSIGNED DEFAULT NULL,
  `AuditChannel` bigint(20) UNSIGNED DEFAULT NULL,
  `UserJoinChan` bigint(20) UNSIGNED DEFAULT NULL,
  `UserLeaveChan` bigint(20) UNSIGNED DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `pasta`
--

CREATE TABLE `pasta` (
  `ID` bigint(20) UNSIGNED NOT NULL,
  `OwnerID` bigint(20) UNSIGNED NOT NULL,
  `Upvotes` bigint(20) UNSIGNED NOT NULL DEFAULT '0',
  `Downvotes` bigint(20) UNSIGNED NOT NULL DEFAULT '0',
  `PastaName` text NOT NULL,
  `Username` text NOT NULL,
  `Created` text NOT NULL,
  `Content` longtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`ID`),
  ADD KEY `ID` (`ID`);

--
-- Indexes for table `commandusage`
--
ALTER TABLE `commandusage`
  ADD PRIMARY KEY (`ID`);

--
-- Indexes for table `guild`
--
ALTER TABLE `guild`
  ADD PRIMARY KEY (`ID`);

--
-- Indexes for table `pasta`
--
ALTER TABLE `pasta`
  ADD PRIMARY KEY (`ID`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `commandusage`
--
ALTER TABLE `commandusage`
  MODIFY `ID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=248;
--
-- AUTO_INCREMENT for table `pasta`
--
ALTER TABLE `pasta`
  MODIFY `ID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=41;COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
