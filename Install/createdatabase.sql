CREATE DATABASE [[niekoboarddb]]
GO

use [[niekoboarddb]]
IF NOT EXISTS 
    (SELECT name  
     FROM master.sys.server_principals
     WHERE name = '[[IIS APPPOOL\AppPoolName]]')
BEGIN
    CREATE LOGIN [[[IIS APPPOOL\AppPoolName]]] FROM WINDOWS;
END
GO
CREATE USER niekoboarduser FOR LOGIN [[[IIS APPPOOL\AppPoolName]]];
GO

ALTER ROLE db_datawriter ADD MEMBER niekoboarduser
ALTER ROLE db_datareader ADD MEMBER niekoboarduser
GO