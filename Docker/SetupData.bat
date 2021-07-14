set PGPASSWORD=changeme
set SERVER=localhost
set PORT=5433
set DATABASE=Foam_auto
set IMPORTFILE=Foam_auto2.sql

createdb -h %SERVER% -p %PORT%  -U postgres -T template0 %DATABASE%
psql -h %SERVER% -p %PORT%  -U postgres %DATABASE% < %IMPORTFILE%

pause 