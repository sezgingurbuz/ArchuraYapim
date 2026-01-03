CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `SeatingPlans` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `SalonAdi` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `PlanAdi` varchar(255) CHARACTER SET utf8mb4 NULL,
    `PlanJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-08 13:50:07.996061',
    `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-08 13:50:07.996121',
    CONSTRAINT `PK_SeatingPlans` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251208135009_InitialCreate', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 13:34:46.418671';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 13:34:46.418607';

ALTER TABLE `SeatingPlans` ADD `Kapasite` int NOT NULL DEFAULT 0;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251209133447_AddKapasiteColumn', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 13:52:55.280849';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 13:52:55.280798';

CREATE TABLE `Salonlar` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `SalonAdi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Sehir` longtext CHARACTER SET utf8mb4 NOT NULL,
    `KoltukDuzeni` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SalonKapasitesi` int NOT NULL,
    CONSTRAINT `PK_Salonlar` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251209135255_AddSalonTable', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 14:27:17.284917';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 14:27:17.284859';

ALTER TABLE `Salonlar` ADD `Durum` longtext CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251209142717_AddSalonDurum', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 19:20:11.668581';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-09 19:20:11.66853';

ALTER TABLE `SeatingPlans` ADD `Durum` longtext CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251209192013_AddSeatingPlanStatus', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-10 13:12:24.019695';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-10 13:12:24.019646';

CREATE TABLE `Etkinlikler` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EtkinlikAdi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SalonId` int NOT NULL,
    `TarihSaat` datetime(6) NOT NULL,
    `Tur` longtext CHARACTER SET utf8mb4 NOT NULL,
    `BiletFiyati` decimal(65,30) NOT NULL,
    `SatisAktifMi` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Etkinlikler` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Etkinlikler_Salonlar_SalonId` FOREIGN KEY (`SalonId`) REFERENCES `Salonlar` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `EtkinlikKoltuklari` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EtkinlikId` int NOT NULL,
    `KoltukNo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Blok` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Sira` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Numara` int NOT NULL,
    `DoluMu` tinyint(1) NOT NULL,
    `UserId` int NULL,
    `Fiyat` decimal(65,30) NOT NULL,
    CONSTRAINT `PK_EtkinlikKoltuklari` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EtkinlikKoltuklari_Etkinlikler_EtkinlikId` FOREIGN KEY (`EtkinlikId`) REFERENCES `Etkinlikler` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_EtkinlikKoltuklari_EtkinlikId` ON `EtkinlikKoltuklari` (`EtkinlikId`);

CREATE INDEX `IX_Etkinlikler_SalonId` ON `Etkinlikler` (`SalonId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251210131225_AddEtkinlikSistemi', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-10 14:48:07.263879';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-10 14:48:07.263831';

ALTER TABLE `Etkinlikler` MODIFY COLUMN `BiletFiyati` decimal(18,2) NOT NULL;

ALTER TABLE `EtkinlikKoltuklari` MODIFY COLUMN `Fiyat` decimal(18,2) NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251210144807_FixDecimalPrecision', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-10 15:13:10.690489';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-10 15:13:10.690438';

ALTER TABLE `Salonlar` ADD `SeatingPlanId` int NULL;

UPDATE Salonlar SET SeatingPlanId = NULL WHERE SeatingPlanId = 0

ALTER TABLE `Salonlar` ADD CONSTRAINT `FK_Salonlar_SeatingPlans_SeatingPlanId` FOREIGN KEY (`SeatingPlanId`) REFERENCES `SeatingPlans` (`Id`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251210151311_AddSeatingPlanId', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Salonlar` DROP COLUMN `KoltukDuzeni`;

ALTER TABLE `Etkinlikler` DROP COLUMN `BiletFiyati`;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 12:48:27.090605';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 12:48:27.090553';

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251224124829_RemoveBiletFiyatiFromEtkinlik', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 13:19:24.18213';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 13:19:24.182078';

ALTER TABLE `EtkinlikKoltuklari` ADD `MusteriAdi` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `EtkinlikKoltuklari` ADD `MusteriSoyadi` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `EtkinlikKoltuklari` ADD `MusteriTelefon` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `EtkinlikKoltuklari` ADD `OdemeYontemi` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `EtkinlikKoltuklari` ADD `SatisTarihi` datetime(6) NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251224131924_AddCustomerInfoToEtkinlikKoltuk', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 13:32:06.785635';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 13:32:06.785581';

ALTER TABLE `EtkinlikKoltuklari` ADD `MusteriEmail` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `EtkinlikKoltuklari` ADD `SatisYapanKullanici` longtext CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251224133207_AddEmailAndSalesPerson', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 15:12:59.055388';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 15:12:59.055337';

CREATE TABLE `AdminUsers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `userName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `passwordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AdminUsers` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251224151259_AddAdminAuth', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 16:57:23.2215';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-24 16:57:23.221449';

ALTER TABLE `EtkinlikKoltuklari` ADD `SatisPlatformu` longtext CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251224165723_AddSatisPlatformuColumn', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-25 11:56:34.562472';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-25 11:56:34.562421';

CREATE TABLE `EtkinlikRaporlari` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EtkinlikAdi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Tur` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SalonAdi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Sehir` longtext CHARACTER SET utf8mb4 NOT NULL,
    `TarihSaat` datetime(6) NOT NULL,
    `ToplamKapasite` int NOT NULL,
    `SatilanBilet` int NOT NULL,
    `BosKoltuk` int NOT NULL,
    `ToplamHasilat` decimal(18,2) NOT NULL,
    `BubiletSatisAdedi` int NOT NULL,
    `BiletinialSatisAdedi` int NOT NULL,
    `NakitSatisAdedi` int NOT NULL,
    `NakitHasilat` decimal(18,2) NOT NULL,
    `KartSatisAdedi` int NOT NULL,
    `KartHasilat` decimal(18,2) NOT NULL,
    `EFTSatisAdedi` int NOT NULL,
    `EFTHasilat` decimal(18,2) NOT NULL,
    `RaporTarihi` datetime(6) NOT NULL,
    `RaporlayanKullanici` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_EtkinlikRaporlari` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251225115636_AddEtkinlikRaporTable', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-25 13:06:00.137444';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-25 13:06:00.137393';

ALTER TABLE `AdminUsers` ADD `Role` longtext CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251225130600_AddRoleToAdminUser', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-26 16:07:56.091713';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-26 16:07:56.091658';

CREATE TABLE `GalleryImages` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ImagePath` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Title` longtext CHARACTER SET utf8mb4 NULL,
    `UploadedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_GalleryImages` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251226160757_AddGalleryImage', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2025-12-27 11:36:09.667986';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-12-27 11:36:09.667936';

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FirstName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `LastName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PhoneNumber` varchar(20) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT UTC_TIMESTAMP(),
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251227113610_AddUsersTable', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Users` ADD `BirthDate` datetime(6) NULL;

ALTER TABLE `Users` ADD `City` varchar(100) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2026-01-01 11:22:57.507133';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2026-01-01 11:22:57.506844';

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260101112301_AddCityAndBirthDateToUser', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2026-01-01 12:59:32.623776';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2026-01-01 12:59:32.623699';

ALTER TABLE `EtkinlikKoltuklari` ADD `BiletKodu` char(36) COLLATE ascii_general_ci NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE `EtkinlikKoltuklari` ADD `GirisYapildiMi` tinyint(1) NOT NULL DEFAULT FALSE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260101125934_AddBiletKoduToEtkinlikKoltuk', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `SeatingPlans` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '2026-01-03 19:21:46.191438';

ALTER TABLE `SeatingPlans` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT '2026-01-03 19:21:46.191339';

ALTER TABLE `Etkinlikler` ADD `SatisTipi` longtext CHARACTER SET utf8mb4 NOT NULL;

CREATE TABLE `EtkinlikKategorileri` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EtkinlikId` int NOT NULL,
    `KategoriAdi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Fiyat` decimal(18,2) NOT NULL,
    `Kontenjan` int NULL,
    CONSTRAINT `PK_EtkinlikKategorileri` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EtkinlikKategorileri_Etkinlikler_EtkinlikId` FOREIGN KEY (`EtkinlikId`) REFERENCES `Etkinlikler` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `KategoriBiletler` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EtkinlikKategoriId` int NOT NULL,
    `RezervasyonKodu` longtext CHARACTER SET utf8mb4 NOT NULL,
    `UserId` int NULL,
    `MusteriAdi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `MusteriSoyadi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `MusteriTelefon` longtext CHARACTER SET utf8mb4 NOT NULL,
    `MusteriEmail` longtext CHARACTER SET utf8mb4 NULL,
    `SatisTarihi` datetime(6) NOT NULL,
    `OdemeYontemi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `OdenenFiyat` decimal(18,2) NOT NULL,
    `AtananKoltukId` int NULL,
    `KoltukAtandiMi` tinyint(1) NOT NULL,
    `KoltukAtamaTarihi` datetime(6) NULL,
    `BiletKodu` char(36) COLLATE ascii_general_ci NOT NULL,
    `GirisYapildiMi` tinyint(1) NOT NULL,
    CONSTRAINT `PK_KategoriBiletler` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_KategoriBiletler_EtkinlikKategorileri_EtkinlikKategoriId` FOREIGN KEY (`EtkinlikKategoriId`) REFERENCES `EtkinlikKategorileri` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_KategoriBiletler_EtkinlikKoltuklari_AtananKoltukId` FOREIGN KEY (`AtananKoltukId`) REFERENCES `EtkinlikKoltuklari` (`Id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_EtkinlikKategorileri_EtkinlikId` ON `EtkinlikKategorileri` (`EtkinlikId`);

CREATE INDEX `IX_KategoriBiletler_AtananKoltukId` ON `KategoriBiletler` (`AtananKoltukId`);

CREATE INDEX `IX_KategoriBiletler_EtkinlikKategoriId` ON `KategoriBiletler` (`EtkinlikKategoriId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260103192148_AddKategoriSatisSistemi', '8.0.0');

COMMIT;

