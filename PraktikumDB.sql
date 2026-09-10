-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: travelmanager
-- ------------------------------------------------------
-- Server version	9.4.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `booking_persons`
--

DROP TABLE IF EXISTS `booking_persons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `booking_persons` (
  `person_id` int NOT NULL AUTO_INCREMENT,
  `booking_id` int NOT NULL,
  `name` varchar(100) NOT NULL,
  `passport_data` varchar(100) DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `is_child` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`person_id`),
  KEY `booking_id` (`booking_id`),
  CONSTRAINT `booking_persons_ibfk_1` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`booking_id`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `booking_persons`
--

LOCK TABLES `booking_persons` WRITE;
/*!40000 ALTER TABLE `booking_persons` DISABLE KEYS */;
INSERT INTO `booking_persons` VALUES (1,1,'Коваленко Олена Іванівна','АА123456','1990-03-15',0),(2,1,'Коваленко Ігор Васильович','АА789012','1988-06-20',0),(3,2,'Бондар Микола Петрович','АВ234567','1985-07-22',0),(4,3,'Савченко Ірина Олегівна','АС345678','1992-11-08',0),(5,3,'Савченко Олексій Романович','АС901234','1990-04-13',0),(6,6,'Лисенко Андрій Сергійович','АЖ678901','1988-01-25',0),(7,6,'Лисенко Марина Дмитрівна','АЖ345678','1990-07-09',0),(8,10,'Романенко Юлія Борисівна','АМ123456','1996-02-19',0),(9,10,'Романенко Кирило Петрович','АМ456789','1993-11-05',0),(10,10,'Романенко Єва Кирилівна','АМ000001','2017-03-12',1),(11,11,'Шевченко Павло Олексійович','АН234567','1975-10-07',0),(12,11,'Шевченко Ольга Миколаївна','АН567890','1978-05-22',0),(13,14,'Ковальчук Роман Степанович','АС678901','1982-05-05',0),(14,14,'Ковальчук Ірина Олегівна','АС111222','1984-09-17',0),(15,19,'Петренко Тетяна Василівна','АЕ567890','1995-09-14',0),(16,19,'Петренко Дмитро Ігорович','АЕ111333','1993-12-01',0),(17,19,'Петренко Соломія Дмитрівна','АЕ000002','2016-07-25',1),(18,19,'Петренко Тарас Дмитрович','АЕ000003','2018-11-10',1),(19,21,'Дмитро','253789','2008-06-02',0),(20,21,'Петро','887689','2007-06-10',0),(27,25,'Дмитро','253789','2008-06-02',0),(28,25,'Петро','887689','2007-06-10',0);
/*!40000 ALTER TABLE `booking_persons` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `bookings`
--

DROP TABLE IF EXISTS `bookings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bookings` (
  `booking_id` int NOT NULL AUTO_INCREMENT,
  `credential_id` int NOT NULL,
  `tour_id` int NOT NULL,
  `start_date` date NOT NULL,
  `booking_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `status` enum('pending','confirmed','cancelled') NOT NULL,
  `number_of_people` int NOT NULL DEFAULT '1',
  `total_price` decimal(10,2) NOT NULL DEFAULT '0.00',
  `comment` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`booking_id`),
  KEY `credential_id` (`credential_id`),
  KEY `tour_id` (`tour_id`),
  CONSTRAINT `bookings_ibfk_1` FOREIGN KEY (`credential_id`) REFERENCES `credentials` (`credential_id`),
  CONSTRAINT `bookings_ibfk_2` FOREIGN KEY (`tour_id`) REFERENCES `tours` (`tour_id`)
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bookings`
--

LOCK TABLES `bookings` WRITE;
/*!40000 ALTER TABLE `bookings` DISABLE KEYS */;
INSERT INTO `bookings` VALUES (1,1,1,'2025-06-10','2025-04-05 10:20:00','confirmed',2,57000.00,'Просимо номер з видом на море'),(2,2,3,'2025-05-20','2025-03-12 14:35:00','confirmed',1,18900.00,NULL),(3,3,5,'2025-10-15','2025-07-01 09:00:00','pending',2,70000.00,'Медовий місяць, прохання прикрасити номер'),(4,4,2,'2025-07-04','2025-05-20 16:45:00','cancelled',3,12600.00,'Скасовано через хворобу'),(5,5,4,'2025-03-01','2025-01-15 11:10:00','pending',2,44000.00,NULL),(6,6,6,'2025-12-22','2025-10-03 13:00:00','confirmed',2,63000.00,'Потрібні лижі розміру 42 та 38'),(7,7,10,'2025-06-15','2025-04-18 08:30:00','confirmed',2,84000.00,'Ласощі без глютену для одного учасника'),(8,9,7,'2025-08-10','2025-06-01 15:20:00','pending',1,65000.00,NULL),(9,10,16,'2025-05-01','2025-03-25 10:00:00','pending',2,19600.00,'Хочемо екскурсію до Давіт Гареджа'),(10,11,13,'2025-07-18','2025-05-10 09:45:00','confirmed',3,114000.00,'Двоє дорослих та одна дитина 8 років'),(11,12,8,'2025-09-05','2025-07-14 12:30:00','confirmed',2,178000.00,'Бунгало над водою, сторона заходу сонця'),(12,14,18,'2025-11-10','2025-09-02 14:00:00','pending',1,47500.00,NULL),(13,15,9,'2025-04-10','2025-02-28 11:15:00','pending',2,29000.00,'Записати на масаж гарячим камінням'),(14,16,15,'2025-11-20','2025-09-10 16:30:00','confirmed',2,110000.00,'Номер з видом на фіорд'),(15,17,12,'2025-09-15','2025-07-20 10:10:00','confirmed',2,55000.00,NULL),(16,19,14,'2025-11-01','2025-08-30 13:45:00','pending',2,82000.00,'Веганське харчування для обох туристів'),(17,1,11,'2025-04-20','2025-03-01 09:30:00','pending',2,50000.00,NULL),(18,3,17,'2025-11-05','2025-09-15 14:20:00','confirmed',1,32000.00,NULL),(19,5,6,'2026-01-07','2025-11-01 10:00:00','pending',4,126000.00,'Сімейний відпочинок, 2 дорослих та 2 дітей'),(20,9,2,'2025-08-25','2025-06-15 11:00:00','confirmed',3,12600.00,NULL),(21,22,7,'2025-07-11','2026-06-16 19:38:21','pending',2,130000.00,''),(25,22,1,'2025-05-01','2026-06-16 19:59:18','confirmed',2,57000.00,'');
/*!40000 ALTER TABLE `bookings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `credentials`
--

DROP TABLE IF EXISTS `credentials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `credentials` (
  `credential_id` int NOT NULL AUTO_INCREMENT,
  `email` varchar(100) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `role` enum('client','manager','admin') NOT NULL,
  `status` enum('active','deleted') DEFAULT 'active',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`credential_id`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `credentials`
--

LOCK TABLES `credentials` WRITE;
/*!40000 ALTER TABLE `credentials` DISABLE KEYS */;
INSERT INTO `credentials` VALUES (1,'olena.kovalenko@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-01-10 09:15:00'),(2,'mykola.bondar@ukr.net','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-01-15 11:30:00'),(3,'iryna.savchenko@yahoo.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-02-01 14:00:00'),(4,'vasyl.melnyk@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-02-10 08:45:00'),(5,'tetiana.petrenko@meta.ua','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-02-20 16:20:00'),(6,'andriy.lysenko@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-03-05 10:00:00'),(7,'natalia.kravets@ukr.net','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-03-12 13:15:00'),(8,'serhiy.moroz@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-03-18 09:30:00'),(9,'oksana.tkachenko@i.ua','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-04-01 15:45:00'),(10,'dmytro.nechyporuk@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-04-10 11:00:00'),(11,'yulia.romanenko@ukr.net','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-04-22 14:30:00'),(12,'pavlo.shevchenko@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-05-03 08:00:00'),(13,'larysa.hrytsenko@meta.ua','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-05-14 17:20:00'),(14,'bohdan.karpenko@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-05-25 12:10:00'),(15,'viktoriia.duda@ukr.net','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-06-01 09:45:00'),(16,'roman.kovalchuk@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-06-10 16:00:00'),(17,'svitlana.marchenko@i.ua','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-06-18 10:30:00'),(18,'ihor.zakharchenko@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-07-02 13:00:00'),(19,'kateryna.sydorenko@ukr.net','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','client','active','2026-07-11 11:15:00'),(20,'admin@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','admin','active','2026-12-01 08:00:00'),(21,'manager@gmail.com','$2a$11$aJjAUEOL84377gMPHwjpdujZsYro1c4w6rz4WyLG85z/nhAIAqSQS','manager','active','2026-12-01 08:00:00'),(22,'toderikodima@gmail.com','$2a$11$P1MprbwrJS4YN6v/V.zF8Obc8Xcrz4LjhVvljBK/O92FNqq36Eivi','manager','deleted','2026-06-16 19:36:48');
/*!40000 ALTER TABLE `credentials` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `penalties`
--

DROP TABLE IF EXISTS `penalties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `penalties` (
  `penalty_id` int NOT NULL AUTO_INCREMENT,
  `booking_id` int NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `is_paid` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`penalty_id`),
  KEY `booking_id` (`booking_id`),
  CONSTRAINT `penalties_ibfk_1` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`booking_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `penalties`
--

LOCK TABLES `penalties` WRITE;
/*!40000 ALTER TABLE `penalties` DISABLE KEYS */;
/*!40000 ALTER TABLE `penalties` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `saved_persons`
--

DROP TABLE IF EXISTS `saved_persons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `saved_persons` (
  `person_id` int NOT NULL AUTO_INCREMENT,
  `credential_id` int NOT NULL,
  `name` varchar(100) NOT NULL,
  `passport_data` varchar(100) DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `is_child` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`person_id`),
  KEY `credential_id` (`credential_id`),
  CONSTRAINT `saved_persons_ibfk_1` FOREIGN KEY (`credential_id`) REFERENCES `credentials` (`credential_id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `saved_persons`
--

LOCK TABLES `saved_persons` WRITE;
/*!40000 ALTER TABLE `saved_persons` DISABLE KEYS */;
INSERT INTO `saved_persons` VALUES (1,1,'Коваленко Ігор Васильович','АА789012','1988-06-20',0),(2,2,'Бондар Олена Петрівна','АВ555666','1987-03-10',0),(3,3,'Савченко Олексій Романович','АС901234','1990-04-13',0),(4,5,'Петренко Дмитро Ігорович','АЕ111333','1993-12-01',0),(5,5,'Петренко Соломія Дмитрівна','АЕ000002','2016-07-25',1),(6,5,'Петренко Тарас Дмитрович','АЕ000003','2018-11-10',1),(7,6,'Лисенко Марина Дмитрівна','АЖ345678','1990-07-09',0),(8,7,'Кравець Василь Олегович','АЗ111222','1991-05-03',0),(9,10,'Нечипорук Аліна Сергіївна','АЛ333444','1985-08-27',0),(10,11,'Романенко Кирило Петрович','АМ456789','1993-11-05',0),(11,11,'Романенко Єва Кирилівна','АМ000001','2017-03-12',1),(12,12,'Шевченко Ольга Миколаївна','АН567890','1978-05-22',0),(13,14,'Карпенко Вікторія Богданівна','АП000004','2005-09-19',1),(14,15,'Дуда Максим Олександрович','АР222333','1992-01-31',0),(15,16,'Ковальчук Ірина Олегівна','АС111222','1984-09-17',0),(16,17,'Марченко Олег Федорович','АТ444555','1986-12-08',0),(17,19,'Сидоренко Артем Олегович','АФ777888','1999-04-22',0),(18,22,'Петро','887689','2007-06-10',0);
/*!40000 ALTER TABLE `saved_persons` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tour_assets`
--

DROP TABLE IF EXISTS `tour_assets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tour_assets` (
  `asset_id` int NOT NULL AUTO_INCREMENT,
  `tour_id` int NOT NULL,
  `asset_type` enum('image','video') NOT NULL,
  `url` varchar(255) NOT NULL,
  PRIMARY KEY (`asset_id`),
  KEY `tour_id` (`tour_id`),
  CONSTRAINT `tour_assets_ibfk_1` FOREIGN KEY (`tour_id`) REFERENCES `tours` (`tour_id`)
) ENGINE=InnoDB AUTO_INCREMENT=54 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tour_assets`
--

LOCK TABLES `tour_assets` WRITE;
/*!40000 ALTER TABLE `tour_assets` DISABLE KEYS */;
INSERT INTO `tour_assets` VALUES (22,1,'image','https://localhost:7041/images/a7b0eb96-260a-4e64-93c0-5bdf56b2d242.jpg'),(23,1,'image','https://localhost:7041/images/3238b7a7-af54-44d3-ab31-65fa83f8d334.jpg'),(24,2,'image','https://localhost:7041/images/481a8a92-e252-466e-a369-721236fc98c3.jpg'),(27,4,'image','https://localhost:7041/images/da1c6d78-c9e2-463f-9f4b-3a438618d1eb.jpg'),(28,5,'image','https://localhost:7041/images/c1cac003-c1a6-4dca-8518-a1b5037b71d0.jpg'),(29,5,'image','https://localhost:7041/images/caa9a49f-6e61-4e3e-9dca-68fdf5b94159.jpg'),(30,6,'image','https://localhost:7041/images/5e3d16fd-ef25-4a3c-855a-26c5917232b7.jpg'),(31,7,'image','https://localhost:7041/images/ea992a7b-dfe2-49dc-99b9-9283f360725c.jpg'),(32,7,'video','https://localhost:7041/videos/d8d70f56-5fbe-4643-8e8a-5b6f2ceab3f3.mp4'),(33,8,'image','https://localhost:7041/images/a3518c64-eea3-4229-897e-98748f05b597.jpg'),(34,9,'image','https://localhost:7041/images/ae8f8eb7-e7e7-4963-9d4b-3411faa1c698.jpg'),(35,10,'image','https://localhost:7041/images/b6c35889-65a7-42c0-b408-4a4691f3eea8.jpg'),(36,12,'image','https://localhost:7041/images/248ca4d8-eee5-48cb-9f6e-758974a6decf.jpg'),(37,13,'image','https://localhost:7041/images/635bead8-9e7a-48f4-88d7-32b0b2a4f058.jpg'),(38,14,'image','https://localhost:7041/images/a1f38622-3aa4-4bd3-866e-fb8764fc4e45.jpg'),(39,15,'image','https://localhost:7041/images/46e41ebc-e2e8-43dd-aa05-408426764d71.jpg'),(40,16,'image','https://localhost:7041/images/9a845c4e-6c41-4076-a802-9c7c480a6215.jpg'),(41,17,'image','https://localhost:7041/images/24d52853-bd71-4a57-9215-d43dda299f24.jpg'),(42,18,'image','https://localhost:7041/images/539175d4-6119-4509-b60d-c74e4b8faa32.jpg'),(44,3,'image','https://localhost:7041/images/79b6aa3b-1394-4e97-9ab0-8266ba6002da.jpg'),(45,3,'video','https://localhost:7041/videos/82fe5ef3-1e58-4297-8915-30e62e4ad6ad.mp4'),(52,20,'image','https://localhost:7041/images/a58d50f7-8674-4237-a356-a4b519985772.jpg'),(53,20,'image','https://localhost:7041/images/88811f9c-7ff3-43d9-b04b-2db2d07982ab.jpg');
/*!40000 ALTER TABLE `tour_assets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tour_types`
--

DROP TABLE IF EXISTS `tour_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tour_types` (
  `type_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  PRIMARY KEY (`type_id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tour_types`
--

LOCK TABLES `tour_types` WRITE;
/*!40000 ALTER TABLE `tour_types` DISABLE KEYS */;
INSERT INTO `tour_types` VALUES (1,'Пляжний відпочинок'),(2,'Гірський туризм'),(3,'Культурно-пізнавальний'),(4,'Екотуризм'),(5,'Пригодницький'),(6,'Круїз'),(7,'Спа та оздоровлення'),(8,'Гастрономічний'),(9,'Дайвінг'),(10,'Лижний'),(11,'Сафарі'),(12,'Містичний'),(13,'Релігійний'),(14,'Сімейний'),(15,'Медичний туризм');
/*!40000 ALTER TABLE `tour_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tours`
--

DROP TABLE IF EXISTS `tours`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tours` (
  `tour_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `description` text,
  `price` decimal(10,2) NOT NULL,
  `duration_days` int NOT NULL,
  `available_from` date NOT NULL,
  `available_to` date NOT NULL,
  `type_id` int NOT NULL,
  PRIMARY KEY (`tour_id`),
  KEY `type_id` (`type_id`),
  CONSTRAINT `tours_ibfk_1` FOREIGN KEY (`type_id`) REFERENCES `tour_types` (`type_id`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tours`
--

LOCK TABLES `tours` WRITE;
/*!40000 ALTER TABLE `tours` DISABLE KEYS */;
INSERT INTO `tours` VALUES (1,'Сонячна Туреччина All Inclusive','Незабутній відпочинок на узбережжі Середземного моря з повним пансіоном. Готель 5*, аквапарк, анімація.',28500.00,10,'2025-05-01','2025-09-30',1),(2,'Карпатська казка','Пішохідні маршрути Карпатами, ночівля у затишних колибах, дегустація гуцульської кухні.',4200.00,7,'2025-06-01','2025-10-15',2),(3,'Перлини Європи: 5 країн','Автобусний тур: Відень, Прага, Будапешт, Краків, Варшава. Екскурсії, гід, проживання 3*.',18900.00,12,'2025-04-15','2025-11-30',3),(4,'Єгипет: Шарм-ель-Шейх','Кораловий риф, нескінченне сонце та гостинність Єгипту. Включно з перельотом та трансфером.',22000.00,9,'2025-01-01','2025-12-31',1),(5,'Таїланд: Острів Пхукет','Тропічні пляжі, тайський масаж, екскурсії до храмів та джунглів.',35000.00,14,'2025-10-01','2026-04-30',1),(6,'Альпійський лижний курорт','Австрійські Альпи, засніжені схили, ски-пас на 7 днів, проживання поряд із підйомником.',31500.00,8,'2025-12-01','2026-03-31',10),(7,'Сафарі у Кенії','Великі рівнини Масаї Мара, спостереження за «Великою пятіркою», проживання у тентових кемпах.',65000.00,10,'2025-07-01','2025-10-31',11),(8,'Мальдіви: Blue Lagoon Resort','Бунгало над водою, приватний пляж, снорклінг та дайвінг у кришталево чистій воді.',89000.00,11,'2025-01-01','2025-12-31',9),(9,'Спа-відпочинок у Карлових Варах','Лікувальні джерела, оздоровчі процедури, прогулянки мальовничим чеським містечком.',14500.00,7,'2025-03-01','2025-11-30',7),(10,'Круїз Середземномор\'ям','MSC Splendida: Барселона – Марсель – Генуя – Неаполь – Рим. Внутрішня каюта, повний пансіон.',42000.00,9,'2025-04-01','2025-10-31',6),(11,'Ізраїль: Святі місця','Єрусалим, Віфлеєм, Назарет, Мертве море. Духовна подорож з православним гідом.',25000.00,8,'2025-03-15','2025-05-31',13),(12,'Гастрономічна Італія','Тоскана, трюфелі, виноробні, кулінарні майстер-класи та дегустаційні вечері в автентичних ресторанах.',27500.00,8,'2025-09-01','2025-11-15',8),(13,'Сімейний Disney Paris','Діснейленд Париж, екскурсія до центру Парижа, проживання у Disney Hotel. Дітям — анімація цілодобово.',38000.00,6,'2025-06-01','2025-08-31',14),(14,'Балі: Острів богів','Рисові тераси Убуду, храм Тананлот, серфінг у Куті, традиційний балійський масаж.',41000.00,12,'2025-10-01','2026-03-31',4),(15,'Норвегія: Фіорди та Полярне сяйво','Бергенська залізниця, круїз фіордами, спостереження за північним сяйвом. Проживання 4*.',55000.00,9,'2025-11-01','2026-02-28',5),(16,'Грузія: Душа Кавказу','Тбілісі, Мцхета, Казбегі, Батумі. Дегустація вин Кахетії, монастирі та кавказька кухня.',9800.00,7,'2025-04-01','2025-11-30',3),(17,'Бангкок – Паттайя','Мегаполіс і море: храм Ват Пхо, тайська кухня на вуличних ринках, пляжний відпочинок у Паттаї.',32000.00,11,'2025-10-15','2026-04-15',8),(18,'Дубай: Місто майбутнього','Бурдж-Халіфа, сафарі в пустелі, шопінг у Dubai Mall, аквапарк Atlantis. Готель 5*.',47500.00,8,'2025-10-01','2026-04-30',3),(20,'Магічний Ісландський Експрес','Незабутня подорож до країни льоду та вогню: відвідування водоспадів Сельяландсфосс та Скогафосс, чорні пляжі Віка, купання у термальних джерелах Блакитної лагуни та полювання на північне сяйво',70000.00,6,'2026-06-15','2026-06-30',5);
/*!40000 ALTER TABLE `tours` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_details`
--

DROP TABLE IF EXISTS `user_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_details` (
  `user_id` int NOT NULL AUTO_INCREMENT,
  `credential_id` int NOT NULL,
  `name` varchar(100) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `passport_data` varchar(100) DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `credential_id` (`credential_id`),
  CONSTRAINT `user_details_ibfk_1` FOREIGN KEY (`credential_id`) REFERENCES `credentials` (`credential_id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_details`
--

LOCK TABLES `user_details` WRITE;
/*!40000 ALTER TABLE `user_details` DISABLE KEYS */;
INSERT INTO `user_details` VALUES (1,1,'Олена','+380501234567','АА123456','1990-03-15'),(2,2,'Микола','+380672345678','АВ234567','1985-07-22'),(3,3,'Ірина','+380933456789','АС345678','1992-11-08'),(4,4,'Василь','+380504567890','АД456789','1978-05-30'),(5,5,'Тетяна','+380675678901','АЕ567890','1995-09-14'),(6,6,'Андрій','+380936789012','АЖ678901','1988-01-25'),(7,7,'Наталія','+380507890123','АЗ789012','1993-06-17'),(8,8,'Сергій','+380678901234','АИ890123','1980-12-03'),(9,9,'Оксана','+380939012345','АК901234','1997-04-28'),(10,10,'Дмитро','+380500123456','АЛ012345','1983-08-11'),(11,11,'Юлія','+380671234567','АМ123456','1996-02-19'),(12,12,'Павло','+380932345678','АН234567','1975-10-07'),(13,13,'Лариса','+380503456789','АО345678','1991-03-23'),(14,14,'Богдан','+380674567890','АП456789','1987-07-16'),(15,15,'Вікторія','+380935678901','АР567890','1994-12-30'),(16,16,'Роман','+380506789012','АС678901','1982-05-05'),(17,17,'Світлана','+380677890123','АТ789012','1989-09-21'),(18,18,'Ігор','+380938901234','АУ890123','1976-01-14'),(19,19,'Катерина','+380509012345','АФ901234','1998-06-08'),(20,20,'Адмін Системний','+380800000000','АХ000000','1985-01-01'),(21,21,'Менеджер','+380895487232','АХ0101011','1988-02-01'),(22,22,'Дмитро','+380993742569','253789','2008-05-26');
/*!40000 ALTER TABLE `user_details` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-09-08 10:45:03
