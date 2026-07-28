USE [master];
GO

DECLARE @ReadOnlyPassword SYSNAME =
    N'$(ReadOnlyPassword)';

DECLARE @LoginCommand NVARCHAR(MAX);

IF SUSER_ID(N'midis_ai_reader') IS NULL
BEGIN
    SET @LoginCommand =
        N'CREATE LOGIN [midis_ai_reader] ' +
        N'WITH PASSWORD = ' +
        QUOTENAME(@ReadOnlyPassword, '''') +
        N', DEFAULT_DATABASE = [MidisSqlAiDb], ' +
        N'CHECK_POLICY = ON, ' +
        N'CHECK_EXPIRATION = OFF;';

    EXEC sys.sp_executesql @LoginCommand;
END
ELSE
BEGIN
    SET @LoginCommand =
        N'ALTER LOGIN [midis_ai_reader] ' +
        N'WITH PASSWORD = ' +
        QUOTENAME(@ReadOnlyPassword, '''') +
        N', DEFAULT_DATABASE = [MidisSqlAiDb], ' +
        N'CHECK_POLICY = ON, ' +
        N'CHECK_EXPIRATION = OFF;';

    EXEC sys.sp_executesql @LoginCommand;
END
GO

USE [MidisSqlAiDb];
GO

IF DATABASE_PRINCIPAL_ID(N'midis_ai_reader') IS NULL
BEGIN
    CREATE USER [midis_ai_reader]
    FOR LOGIN [midis_ai_reader];
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.database_role_members AS membership
    INNER JOIN sys.database_principals AS rolePrincipal
        ON rolePrincipal.principal_id =
           membership.role_principal_id
    INNER JOIN sys.database_principals AS memberPrincipal
        ON memberPrincipal.principal_id =
           membership.member_principal_id
    WHERE rolePrincipal.name = N'db_datareader'
      AND memberPrincipal.name = N'midis_ai_reader'
)
BEGIN
    ALTER ROLE [db_datareader]
    ADD MEMBER [midis_ai_reader];
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.database_role_members AS membership
    INNER JOIN sys.database_principals AS rolePrincipal
        ON rolePrincipal.principal_id =
           membership.role_principal_id
    INNER JOIN sys.database_principals AS memberPrincipal
        ON memberPrincipal.principal_id =
           membership.member_principal_id
    WHERE rolePrincipal.name = N'db_denydatawriter'
      AND memberPrincipal.name = N'midis_ai_reader'
)
BEGIN
    ALTER ROLE [db_denydatawriter]
    ADD MEMBER [midis_ai_reader];
END
GO