set PGPASSWORD=changeme
set SERVER=localhost
set PORT=5432
set DATABASE=Foam_auto
set IMPORTFILE=Foam_auto5.sql

docker exec postgres createdb  -U postgres -T template0 %DATABASE%

docker cp %IMPORTFILE% postgres:%IMPORTFILE%

docker exec postgres psql -U postgres --dbname=%DATABASE% --file=%IMPORTFILE% 

pause 