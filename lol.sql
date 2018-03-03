/*
Navicat MySQL Data Transfer

Source Server         : aaa
Source Server Version : 50550
Source Host           : localhost:3306
Source Database       : lol

Target Server Type    : MYSQL
Target Server Version : 50550
File Encoding         : 65001

Date: 2017-11-27 09:29:49
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `account`
-- ----------------------------
DROP TABLE IF EXISTS `account`;
CREATE TABLE `account` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `account` varchar(255) DEFAULT NULL,
  `pwd` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1021 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of account
-- ----------------------------
INSERT INTO `account` VALUES ('1001', 'Tom', '1234');
INSERT INTO `account` VALUES ('1002', 'Jack', '1234');
INSERT INTO `account` VALUES ('1003', 'a001', '1234');
INSERT INTO `account` VALUES ('1004', 'a003', '1234');
INSERT INTO `account` VALUES ('1005', 'peter_li', '123');
INSERT INTO `account` VALUES ('1006', 'zdd', '1234');
INSERT INTO `account` VALUES ('1007', 'QAQA', '123456');
INSERT INTO `account` VALUES ('1008', 'qwe', 'qwe');
INSERT INTO `account` VALUES ('1009', 'nima', '123');
INSERT INTO `account` VALUES ('1010', 'nini', '1');
INSERT INTO `account` VALUES ('1011', 'caixuan', '123');
INSERT INTO `account` VALUES ('1012', '123', '123');
INSERT INTO `account` VALUES ('1013', 'wcs', '123');
INSERT INTO `account` VALUES ('1014', 'Jason', 'Jason');
INSERT INTO `account` VALUES ('1015', 'Hello', '123456');
INSERT INTO `account` VALUES ('1016', '666', '999999');
INSERT INTO `account` VALUES ('1017', 'qwq', '111');
INSERT INTO `account` VALUES ('1018', '奇酷胖子郭', '123');
INSERT INTO `account` VALUES ('1019', 'aaaa', '1234');
INSERT INTO `account` VALUES ('1020', 'QI', '123');
