/*
Navicat MySQL Data Transfer

Source Server         : aaa
Source Server Version : 50550
Source Host           : localhost:3306
Source Database       : arpg

Target Server Type    : MYSQL
Target Server Version : 50550
File Encoding         : 65001

Date: 2017-06-24 10:11:04
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
) ENGINE=InnoDB AUTO_INCREMENT=239 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of account
-- ----------------------------
INSERT INTO `account` VALUES ('2', 'aaa', '1234');
INSERT INTO `account` VALUES ('3', 'bbb', '1234');
INSERT INTO `account` VALUES ('25', 'ccc', '1234');
INSERT INTO `account` VALUES ('26', '8577', '0000');
INSERT INTO `account` VALUES ('27', '444', '444');
INSERT INTO `account` VALUES ('28', 'asdf', '12345');
INSERT INTO `account` VALUES ('29', 'eeeddds', '1234');
INSERT INTO `account` VALUES ('30', 'fff', 'aaa');
INSERT INTO `account` VALUES ('31', 'aaaaaaa', '123456');
INSERT INTO `account` VALUES ('32', 'aaaa', '1234');
INSERT INTO `account` VALUES ('33', 'max', '1234');
INSERT INTO `account` VALUES ('34', 'kkk', '111');
INSERT INTO `account` VALUES ('35', '888', '666');
INSERT INTO `account` VALUES ('36', 'ouonion', '1213');
INSERT INTO `account` VALUES ('37', 'oaboaix', '123456');
INSERT INTO `account` VALUES ('38', 'zhaoChaoYang', 'zhaoChaoYang');
INSERT INTO `account` VALUES ('39', 'jnn', '1234');
INSERT INTO `account` VALUES ('40', 'asd', 'dsa');
INSERT INTO `account` VALUES ('41', 'adasdaeqwe', '12345');
INSERT INTO `account` VALUES ('42', 'zxc', 'zxc');
INSERT INTO `account` VALUES ('43', 'kkkk', '1234');
INSERT INTO `account` VALUES ('44', 'liu', '987');
INSERT INTO `account` VALUES ('45', 'ddd', '1234');
INSERT INTO `account` VALUES ('46', 'dddd', '1234');
INSERT INTO `account` VALUES ('47', 'qwer', 'qwea');
INSERT INTO `account` VALUES ('48', 'w123', 'w123');
INSERT INTO `account` VALUES ('49', 'cui', '321');
INSERT INTO `account` VALUES ('50', '123456', '123456');
INSERT INTO `account` VALUES ('51', 'qwe', '1234');
INSERT INTO `account` VALUES ('52', 'mei', '1234');
INSERT INTO `account` VALUES ('53', 'qqqq', '12345');
INSERT INTO `account` VALUES ('54', 'weiwei', '111');
INSERT INTO `account` VALUES ('55', '037911', 'asasd');
INSERT INTO `account` VALUES ('56', 'liuyichang', '123');
INSERT INTO `account` VALUES ('57', '111', '222');
INSERT INTO `account` VALUES ('58', '@@@@@@@@', '@@@@@@@@');
INSERT INTO `account` VALUES ('59', 'rgdfg', 'dfgafsdafs');
INSERT INTO `account` VALUES ('60', 'jxm', '1234');
INSERT INTO `account` VALUES ('61', 'fdsfdsfsd', 'sdfasd');
INSERT INTO `account` VALUES ('62', 'a', '1');
INSERT INTO `account` VALUES ('63', '123123', '543534');
INSERT INTO `account` VALUES ('64', 'fdsafsd', 'sdfasd');
INSERT INTO `account` VALUES ('65', 'fdasfsda', 'sdfasdf');
INSERT INTO `account` VALUES ('66', 'fdsaf', 'sdfasd');
INSERT INTO `account` VALUES ('67', 'qweqee', 'ewsasd');
INSERT INTO `account` VALUES ('68', 'dsfasda', 'asdf');
INSERT INTO `account` VALUES ('69', 'sdafsdafsadfsadfsad', 'sdfasd');
INSERT INTO `account` VALUES ('70', 'f', 'df');
INSERT INTO `account` VALUES ('71', 'zxczxc', 'zxc');
INSERT INTO `account` VALUES ('72', 'dsfaf', 'asdf');
INSERT INTO `account` VALUES ('73', 'zxxzc', 'zczxc');
INSERT INTO `account` VALUES ('74', '@@@', '@@@');
INSERT INTO `account` VALUES ('75', 'sdfsadfsdfsdafsadfdfsad', 'sdfasd');
INSERT INTO `account` VALUES ('76', 'ff', 'df');
INSERT INTO `account` VALUES ('77', 'dfsafdsg ', 'asdf');
INSERT INTO `account` VALUES ('78', 'q', '2');
INSERT INTO `account` VALUES ('79', 'fsadfsdfsadfsad', 'sdfasd');
INSERT INTO `account` VALUES ('80', 'dfasd', 'asdf');
INSERT INTO `account` VALUES ('81', '你是谁', '123');
INSERT INTO `account` VALUES ('82', 'qqqqq', 'qqqq');
INSERT INTO `account` VALUES ('83', 'fsdfsdafsadfsda', 'sdfasd');
INSERT INTO `account` VALUES ('84', 'd', '2');
INSERT INTO `account` VALUES ('85', 'dsafsdxcv', 'asdf');
INSERT INTO `account` VALUES ('86', 'ere', '5623');
INSERT INTO `account` VALUES ('87', 'wwww', 'qqqq');
INSERT INTO `account` VALUES ('88', 'fffffff', 'df');
INSERT INTO `account` VALUES ('89', 'fsdfsdaf', 'sdfasd');
INSERT INTO `account` VALUES ('90', 'fads', 'asdf');
INSERT INTO `account` VALUES ('91', 'ee', 'qqqq');
INSERT INTO `account` VALUES ('92', 'g', '2');
INSERT INTO `account` VALUES ('93', 'sadfsdafsda', 'sdfasd');
INSERT INTO `account` VALUES ('94', 'ffffffffffff', 'df');
INSERT INTO `account` VALUES ('95', '0000', '0000');
INSERT INTO `account` VALUES ('96', 'dfsafdsa', 'asdf');
INSERT INTO `account` VALUES ('97', 'fsdfsdfsda', 'sdfasd');
INSERT INTO `account` VALUES ('98', '###', '###');
INSERT INTO `account` VALUES ('99', 'jh', '2');
INSERT INTO `account` VALUES ('100', 'ffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('101', 'yty', 'qqqq');
INSERT INTO `account` VALUES ('102', 'sadfsdafsadfsad', 'sdfasd');
INSERT INTO `account` VALUES ('103', '123', '123');
INSERT INTO `account` VALUES ('104', 'sdafdsafZXCv', 'asdf');
INSERT INTO `account` VALUES ('105', 'qweq', 'qweeeeeeee');
INSERT INTO `account` VALUES ('106', 'jj', '2');
INSERT INTO `account` VALUES ('107', 'dsad', '的撒旦');
INSERT INTO `account` VALUES ('108', 'fffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('109', 'iuiokhj', 'qqqq');
INSERT INTO `account` VALUES ('110', '21313', '4214214');
INSERT INTO `account` VALUES ('111', 'qwwww', 'wwwww');
INSERT INTO `account` VALUES ('112', 'xcveawdsf', 'asdf');
INSERT INTO `account` VALUES ('113', 'tryhgf', '14551');
INSERT INTO `account` VALUES ('114', 'fsdfsdafsadfsdafsdfsdafsadfsda', 'sdfasd');
INSERT INTO `account` VALUES ('115', 'fffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('116', 'jjkk', '2');
INSERT INTO `account` VALUES ('117', 'iuiokhjdfg', 'qqqq');
INSERT INTO `account` VALUES ('118', 'xcfasfsd', 'asdf');
INSERT INTO `account` VALUES ('119', '21421', '4214214');
INSERT INTO `account` VALUES ('120', 'qq', 'qweeeeeeee');
INSERT INTO `account` VALUES ('121', 'fsdafsdafsadfsadfsadfsadf', 'sdfasd');
INSERT INTO `account` VALUES ('122', 'ffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('123', 'fh', '2');
INSERT INTO `account` VALUES ('124', 'afdsaf', 'asdf');
INSERT INTO `account` VALUES ('125', 'sssssssss', 'sss');
INSERT INTO `account` VALUES ('126', '3243', '4214214');
INSERT INTO `account` VALUES ('127', '#############', '###############');
INSERT INTO `account` VALUES ('128', 'dsa', 'd dsa');
INSERT INTO `account` VALUES ('129', 'fsdafdfsadfsadf', 'sdfasd');
INSERT INTO `account` VALUES ('130', 'qqq', 'qweeeeeeee');
INSERT INTO `account` VALUES ('131', 'fffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('132', '8156198', '15616');
INSERT INTO `account` VALUES ('133', '523', '4214214');
INSERT INTO `account` VALUES ('134', 'yuty', '2');
INSERT INTO `account` VALUES ('135', 'zxcvxcva', 'asdf');
INSERT INTO `account` VALUES ('136', 'sdasdas', 'dasdasd');
INSERT INTO `account` VALUES ('137', 'fsadfsdafgsdagaeryherwy ', 'sdfasd');
INSERT INTO `account` VALUES ('138', '5464', '4214214');
INSERT INTO `account` VALUES ('139', 'ffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('140', 'asfdsaf', 'asdf');
INSERT INTO `account` VALUES ('141', '12', 'sds');
INSERT INTO `account` VALUES ('142', '456436', '4214214');
INSERT INTO `account` VALUES ('143', 'fffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('144', 'fgsadfsdafgstghae', 'sdfasd');
INSERT INTO `account` VALUES ('145', 'try', '4547');
INSERT INTO `account` VALUES ('146', 'dfasg', 'asdf');
INSERT INTO `account` VALUES ('147', '45547', '4214214');
INSERT INTO `account` VALUES ('148', 'www', 'www');
INSERT INTO `account` VALUES ('149', 'ffffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('150', 'ada', '2');
INSERT INTO `account` VALUES ('151', 'afsadgasge', 'sdfasd');
INSERT INTO `account` VALUES ('152', 'saddd', 'asd');
INSERT INTO `account` VALUES ('153', 'qqqqqq', 'qweeeeeeee');
INSERT INTO `account` VALUES ('154', '45547475347', '4214214');
INSERT INTO `account` VALUES ('155', '12sds', 'sds');
INSERT INTO `account` VALUES ('156', '12dsa', '654dsa');
INSERT INTO `account` VALUES ('157', 'fffffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('158', 'dsfasd', 'asdf');
INSERT INTO `account` VALUES ('159', '423', '4214214');
INSERT INTO `account` VALUES ('160', 'agweetgwebhqagt', 'sdfasd');
INSERT INTO `account` VALUES ('161', 'w456w', 'w456w');
INSERT INTO `account` VALUES ('162', 'y5ry5', 'y54y');
INSERT INTO `account` VALUES ('163', '4325', '4214214');
INSERT INTO `account` VALUES ('164', 'ffffffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('165', 'kk', '2');
INSERT INTO `account` VALUES ('166', 'q1r2qtq3yhq3', 'sdfasd');
INSERT INTO `account` VALUES ('167', 'dsfasdasdf', 'asdf');
INSERT INTO `account` VALUES ('168', 'sadasd', 'ddasdas');
INSERT INTO `account` VALUES ('169', '54326', '4214214');
INSERT INTO `account` VALUES ('170', 'fffffffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('171', '%%%%%%%%%%%%%%%%%%%%%%%%%%%%5', '%%%%%%%%%%%%%%%%%%%%%%%%5555555');
INSERT INTO `account` VALUES ('172', '4y54y', 'yrty45');
INSERT INTO `account` VALUES ('173', '525', '4214214');
INSERT INTO `account` VALUES ('174', 'q1rwet', 'sdfasd');
INSERT INTO `account` VALUES ('175', 'ffffffffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('176', '52566243', '4214214');
INSERT INTO `account` VALUES ('177', 'ag3qytq', 'sdfasd');
INSERT INTO `account` VALUES ('178', 'dsadasd', 'dsadas');
INSERT INTO `account` VALUES ('179', '12321', '4214214');
INSERT INTO `account` VALUES ('180', '11111', '123');
INSERT INTO `account` VALUES ('181', 'fffffffffffffffffffffffffffffff', 'df');
INSERT INTO `account` VALUES ('182', 'r23yqa', 'sdfasd');
INSERT INTO `account` VALUES ('183', 'as', 'qweeeeeeee');
INSERT INTO `account` VALUES ('184', 'gg45y', 'hgdfh');
INSERT INTO `account` VALUES ('185', '64326', '4214214');
INSERT INTO `account` VALUES ('186', 'w111', 'w111');
INSERT INTO `account` VALUES ('187', 'kkfs', '2');
INSERT INTO `account` VALUES ('188', '643265756', '4214214');
INSERT INTO `account` VALUES ('189', 's', 'qweeeeeeee');
INSERT INTO `account` VALUES ('190', '734', '4214214');
INSERT INTO `account` VALUES ('191', 'kkfsjkk', '2');
INSERT INTO `account` VALUES ('192', 'mumu', '123');
INSERT INTO `account` VALUES ('193', 'zhao', '11');
INSERT INTO `account` VALUES ('194', 'lu', '1234');
INSERT INTO `account` VALUES ('195', 'mmm', '123');
INSERT INTO `account` VALUES ('196', '习近平', 'sss');
INSERT INTO `account` VALUES ('197', 'orz', '321');
INSERT INTO `account` VALUES ('198', 'aa', '6666');
INSERT INTO `account` VALUES ('199', 'ch', '123');
INSERT INTO `account` VALUES ('200', 'sasa', '1111');
INSERT INTO `account` VALUES ('201', 'ouon', '1213');
INSERT INTO `account` VALUES ('202', 'qwe123', '1234');
INSERT INTO `account` VALUES ('203', '666', '1234');
INSERT INTO `account` VALUES ('204', 'eeeeee', '33');
INSERT INTO `account` VALUES ('205', '857q', '8577');
INSERT INTO `account` VALUES ('206', 'qwertyyuyiy', 'qwer123');
INSERT INTO `account` VALUES ('207', 'qsxs', '1234');
INSERT INTO `account` VALUES ('208', 'jkluio', '1');
INSERT INTO `account` VALUES ('209', 'zyb', '1234');
INSERT INTO `account` VALUES ('210', '8888', '1234');
INSERT INTO `account` VALUES ('211', '5678', '1234');
INSERT INTO `account` VALUES ('212', '李达康', '1');
INSERT INTO `account` VALUES ('213', 'wwwww', '1441');
INSERT INTO `account` VALUES ('214', 'zzzzz', '1234');
INSERT INTO `account` VALUES ('215', '999', '1234');
INSERT INTO `account` VALUES ('216', 'eeee', '22');
INSERT INTO `account` VALUES ('217', 'SSSS', '123456');
INSERT INTO `account` VALUES ('218', '8577qwe', '1234');
INSERT INTO `account` VALUES ('219', 'ggll', '1234');
INSERT INTO `account` VALUES ('220', 'mmn', '1234');
INSERT INTO `account` VALUES ('221', 'magua', '111');
INSERT INTO `account` VALUES ('222', 'mmmmm', 'mmmmm');
INSERT INTO `account` VALUES ('223', '456', '123');
INSERT INTO `account` VALUES ('224', '212213', '123');
INSERT INTO `account` VALUES ('225', 'zzz', '1234');
INSERT INTO `account` VALUES ('226', 'dddddddd', '12345');
INSERT INTO `account` VALUES ('227', 'iii', '1234');
INSERT INTO `account` VALUES ('228', 'ABc', '123');
INSERT INTO `account` VALUES ('229', 'sssss', '1234');
INSERT INTO `account` VALUES ('230', 'ligeng', '1234');
INSERT INTO `account` VALUES ('231', '1101', '1234');
INSERT INTO `account` VALUES ('232', '159', '786');
INSERT INTO `account` VALUES ('233', 'bbq', 'bbq');
INSERT INTO `account` VALUES ('234', '严厉的尚老老师', '1234');
INSERT INTO `account` VALUES ('235', 'qi', '123');
INSERT INTO `account` VALUES ('236', 'yyyu', '1011');
INSERT INTO `account` VALUES ('237', 'aac', 'aac');
INSERT INTO `account` VALUES ('238', 'aas', '1456');

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
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=127 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of characters
-- ----------------------------
INSERT INTO `characters` VALUES ('3', '2', '赵四', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('4', '2', '刘能', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('5', '2', '王老七', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1004');
INSERT INTO `characters` VALUES ('6', '2', '塞恩', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1006');
INSERT INTO `characters` VALUES ('95', '25', '艾迪', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('96', '193', '找', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('97', '193', 'gg', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1004');
INSERT INTO `characters` VALUES ('98', '197', 'wew', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1004');
INSERT INTO `characters` VALUES ('99', '198', '666', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('100', '195', 'LoveZRP', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1001');
INSERT INTO `characters` VALUES ('101', '200', '路西法', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1006');
INSERT INTO `characters` VALUES ('102', '203', '123', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('103', '199', 'xxx', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1001');
INSERT INTO `characters` VALUES ('104', '204', '盖伦', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1001');
INSERT INTO `characters` VALUES ('105', '200', 'hhhs', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('106', '207', 'kk', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('107', '206', '哇咔咔', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1006');
INSERT INTO `characters` VALUES ('108', '209', '妖姬', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('109', '210', 'fchg', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1006');
INSERT INTO `characters` VALUES ('110', '212', '李大康', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1005');
INSERT INTO `characters` VALUES ('111', '216', 'ff', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('112', '214', '罗莉', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1005');
INSERT INTO `characters` VALUES ('113', '217', '刘异常', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1006');
INSERT INTO `characters` VALUES ('114', '224', 'asdsad', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1003');
INSERT INTO `characters` VALUES ('115', '222', '大佬来了', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1004');
INSERT INTO `characters` VALUES ('116', '223', '翟志饴', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('117', '194', 'gailun', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1001');
INSERT INTO `characters` VALUES ('118', '224', 'cvxv', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('119', '208', '6666', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1005');
INSERT INTO `characters` VALUES ('120', '226', '习近平', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1001');
INSERT INTO `characters` VALUES ('121', '228', 'LAJI', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('122', '230', 'lllllllll', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('123', '234', '尚老师', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1002');
INSERT INTO `characters` VALUES ('124', '235', '222', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1005');
INSERT INTO `characters` VALUES ('126', '2', '达康书记', '0', '1', '1', '1', '0', '0', '0', '-14.65', '0.5', '-5.54', '1005');

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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of equip
-- ----------------------------
INSERT INTO `equip` VALUES ('1', '1001', '1', '1003');
INSERT INTO `equip` VALUES ('2', '1001', '2', '1004');
INSERT INTO `equip` VALUES ('3', '1001', '3', '1005');

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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of inventory
-- ----------------------------
INSERT INTO `inventory` VALUES ('3', '1', '1', '2', '50');

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
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mail
-- ----------------------------
INSERT INTO `mail` VALUES ('14', '0', '126', '开服大礼包1', '您收到极品装备倚天剑的碎片1', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('15', '0', '126', '开服大礼包2', '您收到极品装备倚天剑的碎片2', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('16', '0', '126', '开服大礼包3', '您收到极品装备倚天剑的碎片3', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('17', '0', '126', '开服大礼包4', '您收到极品装备倚天剑的碎片4', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('18', '0', '126', '开服大礼包5', '您收到极品装备倚天剑的碎片5', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('19', '0', '126', '开服大礼包6', '您收到极品装备倚天剑的碎片6', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('20', '0', '126', '开服大礼包7', '您收到极品装备倚天剑的碎片7', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('21', '0', '126', '开服大礼包8', '您收到极品装备倚天剑的碎片8', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('22', '0', '126', '开服大礼包9', '您收到极品装备倚天剑的碎片9', '2017-06-23', '0', '0');
INSERT INTO `mail` VALUES ('23', '0', '126', '开服大礼包10', '您收到极品装备倚天剑的碎片10', '2017-06-23', '0', '0');

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
INSERT INTO `mail_items` VALUES ('1', '1', '1003', '4');
INSERT INTO `mail_items` VALUES ('2', '1', '1004', '1');

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
