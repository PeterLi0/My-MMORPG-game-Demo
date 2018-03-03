/*
Navicat MySQL Data Transfer

Source Server         : aaa
Source Server Version : 50550
Source Host           : localhost:3306
Source Database       : wow

Target Server Type    : MYSQL
Target Server Version : 50550
File Encoding         : 65001

Date: 2017-12-08 17:34:54
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `account`
-- ----------------------------
DROP TABLE IF EXISTS `account`;
CREATE TABLE `account` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `account` varchar(255) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=241 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of account
-- ----------------------------
INSERT INTO `account` VALUES ('240', 'a001', '1234');

-- ----------------------------
-- Table structure for `characters`
-- ----------------------------
DROP TABLE IF EXISTS `characters`;
CREATE TABLE `characters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `accountid` int(11) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `race` int(4) DEFAULT NULL COMMENT '种族',
  `job` int(4) DEFAULT NULL COMMENT '职业',
  `gender` int(4) DEFAULT NULL COMMENT '性别',
  `level` int(11) DEFAULT NULL COMMENT '等级',
  `exp` int(11) DEFAULT NULL COMMENT '经验',
  `diamond` int(11) DEFAULT NULL,
  `gold` int(11) DEFAULT NULL,
  `pos_x` float DEFAULT NULL,
  `pos_y` float DEFAULT NULL,
  `pos_z` float DEFAULT NULL,
  `cfgid` int(11) DEFAULT NULL,
  `mapid` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=129 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of characters
-- ----------------------------
INSERT INTO `characters` VALUES ('128', '240', '盖伦1', '0', '1', '1', '1', '0', '200', '1000', '0', '0.5', '0', '1002', '1001');

-- ----------------------------
-- Table structure for `equip`
-- ----------------------------
DROP TABLE IF EXISTS `equip`;
CREATE TABLE `equip` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `characterid` int(11) DEFAULT NULL,
  `slot` int(11) DEFAULT NULL,
  `itemid` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=76 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of equip
-- ----------------------------
INSERT INTO `equip` VALUES ('70', '128', '1', '1101');
INSERT INTO `equip` VALUES ('71', '128', '2', '1201');
INSERT INTO `equip` VALUES ('72', '128', '3', '1301');
INSERT INTO `equip` VALUES ('73', '128', '4', '-1');
INSERT INTO `equip` VALUES ('74', '128', '5', '-1');
INSERT INTO `equip` VALUES ('75', '128', '6', '-1');

-- ----------------------------
-- Table structure for `inventory`
-- ----------------------------
DROP TABLE IF EXISTS `inventory`;
CREATE TABLE `inventory` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `characterid` int(11) DEFAULT NULL,
  `slot` int(11) DEFAULT NULL,
  `itemid` int(11) DEFAULT NULL,
  `num` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=514 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of inventory
-- ----------------------------
INSERT INTO `inventory` VALUES ('494', '128', '1', '1101', '1');
INSERT INTO `inventory` VALUES ('495', '128', '2', '1102', '1');
INSERT INTO `inventory` VALUES ('496', '128', '3', '1103', '1');
INSERT INTO `inventory` VALUES ('497', '128', '4', '1201', '1');
INSERT INTO `inventory` VALUES ('498', '128', '5', '1202', '1');
INSERT INTO `inventory` VALUES ('499', '128', '6', '1203', '1');
INSERT INTO `inventory` VALUES ('500', '128', '7', '1301', '1');
INSERT INTO `inventory` VALUES ('501', '128', '8', '1302', '1');
INSERT INTO `inventory` VALUES ('502', '128', '9', '1303', '1');
INSERT INTO `inventory` VALUES ('503', '128', '10', '1304', '1');
INSERT INTO `inventory` VALUES ('504', '128', '11', '-1', '1');
INSERT INTO `inventory` VALUES ('505', '128', '12', '-1', '1');
INSERT INTO `inventory` VALUES ('506', '128', '13', '-1', '1');
INSERT INTO `inventory` VALUES ('507', '128', '14', '-1', '1');
INSERT INTO `inventory` VALUES ('508', '128', '15', '-1', '1');
INSERT INTO `inventory` VALUES ('509', '128', '16', '-1', '1');
INSERT INTO `inventory` VALUES ('510', '128', '17', '-1', '1');
INSERT INTO `inventory` VALUES ('511', '128', '18', '-1', '1');
INSERT INTO `inventory` VALUES ('512', '128', '19', '-1', '1');
INSERT INTO `inventory` VALUES ('513', '128', '20', '-1', '1');

-- ----------------------------
-- Table structure for `mail`
-- ----------------------------
DROP TABLE IF EXISTS `mail`;
CREATE TABLE `mail` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `sender_id` int(11) DEFAULT NULL,
  `receiver_id` int(11) DEFAULT NULL,
  `subject` varchar(255) DEFAULT NULL,
  `body` varchar(255) DEFAULT NULL,
  `deliver_time` varchar(255) DEFAULT NULL,
  `money` int(11) DEFAULT NULL,
  `has_items` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=44 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mail
-- ----------------------------
INSERT INTO `mail` VALUES ('34', '0', '128', '开服大礼包1', '您收到极品装备倚天剑的碎片1', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('35', '0', '128', '开服大礼包2', '您收到极品装备倚天剑的碎片2', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('36', '0', '128', '开服大礼包3', '您收到极品装备倚天剑的碎片3', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('37', '0', '128', '开服大礼包4', '您收到极品装备倚天剑的碎片4', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('38', '0', '128', '开服大礼包5', '您收到极品装备倚天剑的碎片5', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('39', '0', '128', '开服大礼包6', '您收到极品装备倚天剑的碎片6', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('40', '0', '128', '开服大礼包7', '您收到极品装备倚天剑的碎片7', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('41', '0', '128', '开服大礼包8', '您收到极品装备倚天剑的碎片8', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('42', '0', '128', '开服大礼包9', '您收到极品装备倚天剑的碎片9', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('43', '0', '128', '开服大礼包10', '您收到极品装备倚天剑的碎片10', '2017-06-23', '0', '0');

-- ----------------------------
-- Table structure for `mail_items`
-- ----------------------------
DROP TABLE IF EXISTS `mail_items`;
CREATE TABLE `mail_items` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `mail_id` int(11) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  `item_num` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mail_items
-- ----------------------------

-- ----------------------------
-- Table structure for `queststats`
-- ----------------------------
DROP TABLE IF EXISTS `queststats`;
CREATE TABLE `queststats` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `characterid` int(11) DEFAULT NULL,
  `questid` int(11) DEFAULT NULL,
  `status` int(4) DEFAULT NULL,
  `explored` int(4) DEFAULT NULL,
  `timer` int(11) DEFAULT NULL,
  `mobcount1` int(6) DEFAULT NULL,
  `mobcount2` int(6) DEFAULT NULL,
  `mobcount3` int(6) DEFAULT NULL,
  `mobcount4` int(6) DEFAULT NULL,
  `itemcount1` int(6) DEFAULT NULL,
  `itemcount2` int(6) DEFAULT NULL,
  `itemcount3` int(6) DEFAULT NULL,
  `itemcount4` int(6) DEFAULT NULL,
  `playercount` int(6) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of queststats
-- ----------------------------
