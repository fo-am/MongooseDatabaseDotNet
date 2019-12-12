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

## table names
"alarm"  
"alarm_cause"  
"anti_parasite"  
"babysitting"  
"blood_data"  
"capture"  
"dna_samples"  
"event_log"  
"group_composition"  
"hpa_sample"  
"individual"  
"individual_event"  
"individual_event_code"  
"individual_name_history"  
"inter_group_interaction"  
"interaction_outcome"  
"litter"  
"litter_event"  
"litter_event_code"  
"mate_guard"  
"maternal_conditioning_litter"  
"maternel_conditioning_females"  
"meterology"  
"oestrus"  
"oestrus_affiliation"  
"oestrus_aggression"  
"oestrus_event"  
"oestrus_male_aggression"  
"oestrus_mating"  
"oestrus_nearest"  
"ox_shielding_feeding"  
"ox_shielding_group"  
"ox_shielding_male"  
"pack"  
"pack_composition"  
"pack_event"  
"pack_event_code"  
"pack_history"  
"pack_move"  
"pack_move_destination"  
"pack_name_history"  
"poo_sample"  
"pregnancy"  
"pregnancy_affiliation"  
"pregnancy_aggression"  
"pregnancy_focal"  
"pregnancy_nearest"  
"provisioning_data"  
"pup_aggression"  
"pup_association"  
"pup_care"  
"pup_feed"  
"pup_find"  
"pup_focal"  
"pup_nearest"  
"radiocollar"  
"ultrasound"  
"weight"  


#### View names

"Antiparasite experiment"  
"Babysitting Records"  
"DNA Samples"  
"Group Composition"  
"HPA samples"  
"Indvidual Events"  
"Inter Group Interactions"  
"Jenni's blood data"  
"Maternal Condition Experiment: Females"  
"Maternal Condition Experiment: Litters"  
"Maternal Condition Experiment: provisioning data"  
"METEROLOGICAL DATA"  
"Ox shielding experiment - feeding record"  
"Ox Shielding Experiment - female treatment groups"  
"Ox Shielding Experiment - males being sampled"  
"Pack Alarms"  
"Pack events"  
"Pack Move"  
"Poo Database"  
"pup association"  
"Radiocollar records"  
"UltrasoundView"  
"WeightView"  


### ToDo


* connection of tables
* Example queues
* think about logging
* How to deploy the dataReceiver
* https://www.microsoft.com/net/download/linux-package-manager/debian9/sdk-2.1.4
*   https://www.microsoft.com/net/learn/get-started/linux/debian9
*   https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x
*   
* git clone https://github.com/fo-am/MongooseDatabaseDotNet.git
* dotnet restore

* users on the database
* 
* Install rabbit.
* https://www.rabbitmq.com/install-debian.html
* 
* log issues in github 
* https://github.com/fo-am/MongooseDatabaseDotNet









