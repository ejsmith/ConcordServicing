#wait for the SQL Server to come up
sleep 30s

echo "running set up script"
#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@55w0rd -d master -i db-init.sql
