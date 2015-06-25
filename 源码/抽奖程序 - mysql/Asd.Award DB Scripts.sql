/*
Navicat MySQL Data Transfer

Source Server         : TMOA_MySql
Source Server Version : 50621
Source Host           : localhost:3306
Source Database       : TMAward

Target Server Type    : MYSQL
Target Server Version : 50621
File Encoding         : 65001

Date: 2015-02-09 12:41:27
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for TmpAward
-- ----------------------------
DROP TABLE IF EXISTS `TmpAward`;
CREATE TABLE `TmpAward` (
  `Level` varchar(50) DEFAULT NULL,
  `TicketNO` varchar(50) NOT NULL,
  `Remark` varchar(50) DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`TicketNO`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for TmpOption
-- ----------------------------
DROP TABLE IF EXISTS `TmpOption`;
CREATE TABLE `TmpOption` (
  `BelongTo` varchar(2) NOT NULL,
  `Level` char(1) NOT NULL,
  `HitSequence` int(11) NOT NULL,
  `AwardQty` int(11) NOT NULL,
  `Remark` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`BelongTo`,`Level`,`HitSequence`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for TmpSetting
-- ----------------------------
DROP TABLE IF EXISTS `TmpSetting`;
CREATE TABLE `TmpSetting` (
  `SettingName` varchar(50) NOT NULL,
  `SettingValue` varchar(50) DEFAULT NULL,
  `Remark` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`SettingName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for TmpTicket
-- ----------------------------
DROP TABLE IF EXISTS `TmpTicket`;
CREATE TABLE `TmpTicket` (
  `TicketNO` varchar(50) NOT NULL,
  `TicketCount` varchar(50) DEFAULT NULL,
  `TicketStart` varchar(50) DEFAULT NULL,
  `TicketEnd` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`TicketNO`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
