CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `users` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `email` varchar(320) CHARACTER SET utf8mb4 NOT NULL,
    `password_hash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `profile` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_users` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_users_email` ON `users` (`email`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251009211757_InitialCreate', '8.0.8');

COMMIT;

