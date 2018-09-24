### Physical setup
picture of the bits that make up the system

Mongoose > tablets > pi > database(1)>data pipe>rabbitMq(internet)> data receiver> new database.
![](images\overview.png)
### Layout of the new database

Basic elements are captured in individual tables in the database, these tables can be 'joined' to retrieve the data relevant to a question. 

#### Core tables
![](images\basic.png)
##### pack
name
date formed

##### litter
name

##### individual
name sex litter etc

##### pack_history
where the membership of an individual in a pack is recorded. Many things relate to an individual whilst in a pack and this is where that is recorded.


##### pup focal
![](images\pup_focal.png)

#### oestrus focal
![](images\oestrus_event.png)

#### group composition
![](images\GroupComp.png)

## Events sent by the DataPipe

GroupAlarmEvent  
GroupComposition  
GroupCompositionMateGuard  
GroupMoveEvent  
GroupWeightMeasure  
IndividualCreated  
IndividualDied  
IndividualUpdate  
InterGroupInteractionEvent  
LifeHistoryEvent  
LitterCreated 
OestrusEvent  
OestrusAffiliationEvent  
OestrusAggressionEvent  
OestrusMaleAggression  
OestrusMateEvent  
OestrusNearest  
PackCreated  
PackMove  
PregnancyAffiliation  
PregnancyAggression  
PregnancyFocal  
PregnancyNearest  
PupAggressionEvent  
PupAssociation  
PupCare  
PupFeed  
PupFind  
PupFocal  
PupNearest  
WeightMeasure  
