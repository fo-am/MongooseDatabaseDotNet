#!/bin/bash

##############################
## POSTGRESQL BACKUP CONFIG ##
##############################
 
# Optional hostname to adhere to pg_hba policies.  Will default to "localhost" if none specified.
# HOSTNAME=127.0.0.1
 
# Optional username to connect to database as.  Will default to "postgres" if none specified.
#USERNAME=pgusername
 
# This dir will be created if it doesn't exist.  This must be writable by the user the script is
# running as.
BACKUP_DIR=/var/backups/postgres

# Database name to backup
DATABASE=Foam_auto
 
# Will produce a custom-format backup if set to "yes"
ENABLE_CUSTOM_BACKUPS=no
 
# Will produce a gzipped plain-format backup if set to "yes"
ENABLE_PLAIN_BACKUPS=yes

#### SETTINGS FOR ROTATED BACKUPS ####

# Number of days to keep daily backups
#DAYS_TO_KEEP=14

 
###########################
### INITIALISE DEFAULTS ###
###########################
 
if [ ! $HOSTNAME ]; then
  HOSTNAME="localhost"
fi;
 
if [ ! $USERNAME ]; then
  USERNAME="postgres"
fi;
 
 
###########################
#### START THE BACKUPS ####
###########################
 
function perform_backups()
{
  SUFFIX=$1
  FINAL_BACKUP_DIR=$BACKUP_DIR"`date +\%Y-\%m-\%d`$SUFFIX/"
 
  echo "Making backup directory in $FINAL_BACKUP_DIR"
 
  if ! mkdir -p $FINAL_BACKUP_DIR; then
    echo "Cannot create backup directory in $FINAL_BACKUP_DIR. Go and fix it!" 1>&2
    exit 1;
  fi;
 
  echo -e "\n\nPerforming full backup of $DATABASE"
  echo -e "--------------------------------------------\n"
 
    if [ $ENABLE_PLAIN_BACKUPS = "yes" ]
    then
      echo "Plain backup of $DATABASE"
 
      if ! pg_dump -Fp -h "$HOSTNAME" -U "$USERNAME" "$DATABASE" | gzip > $FINAL_BACKUP_DIR"$DATABASE".sql.gz.in_progress; then
        echo "[!!ERROR!!] Failed to produce plain backup database $DATABASE" 1>&2
      else
        mv $FINAL_BACKUP_DIR"$DATABASE".sql.gz.in_progress $FINAL_BACKUP_DIR"$DATABASE".sql.gz
      fi
    fi
 
    if [ $ENABLE_CUSTOM_BACKUPS = "yes" ]
    then
      echo "Custom backup of $DATABASE"
 
      if ! pg_dump -Fc -h "$HOSTNAME" -U "$USERNAME" "$DATABASE" -f $FINAL_BACKUP_DIR"$DATABASE".custom.in_progress; then
        echo "[!!ERROR!!] Failed to produce custom backup database $DATABASE"
      else
        mv $FINAL_BACKUP_DIR"$DATABASE".custom.in_progress $FINAL_BACKUP_DIR"$DATABASE".custom
      fi
    fi
 
  echo -e "\nDatabase backup complete!"
}
 
# DAILY BACKUPS
 
# Delete daily backups older than DAYS_TO_KEEP
#find $BACKUP_DIR -maxdepth 1 -mtime +$DAYS_TO_KEEP -name "*-daily" -exec rm -rf '{}' ';'

perform_backups "-daily"