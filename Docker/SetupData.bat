set PGPASSWORD=changeme

createdb -p 5433 -U postgres -T template0 Foam_auto
psql -p 5433 -U postgres Foam_auto < Foam_auto5.sql
pause 