-- Database generated with pgModeler (PostgreSQL Database Modeler).
-- pgModeler  version: 0.9.1
-- PostgreSQL version: 9.4
-- Project Site: pgmodeler.io
-- Model Author: ---


-- Database creation must be done outside a multicommand file.
-- These commands were put in this file only as a convenience.
-- -- object: "Foam_auto" | type: DATABASE --
-- -- DROP DATABASE IF EXISTS "Foam_auto";
-- CREATE DATABASE "Foam_auto";
-- -- ddl-end --
-- 

-- object: mongoose | type: SCHEMA --
-- DROP SCHEMA IF EXISTS mongoose CASCADE;
CREATE SCHEMA mongoose;
-- ddl-end --
ALTER SCHEMA mongoose OWNER TO postgres;
-- ddl-end --

SET search_path TO pg_catalog,public,mongoose;
-- ddl-end --

-- object: mongoose.pack | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack CASCADE;
CREATE TABLE mongoose.pack(
	pack_id serial NOT NULL,
	name text NOT NULL,
	pack_created_date timestamp,
	unique_id text,
	CONSTRAINT pack_pk PRIMARY KEY (pack_id),
	CONSTRAINT name_unique UNIQUE (name),
	CONSTRAINT unique_pack_unique_id UNIQUE (unique_id)

);
-- ddl-end --
ALTER TABLE mongoose.pack OWNER TO postgres;
-- ddl-end --

-- object: mongoose.litter | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.litter CASCADE;
CREATE TABLE mongoose.litter(
	litter_id serial NOT NULL,
	pack_id integer,
	name text NOT NULL,
	date_formed timestamp,
	CONSTRAINT litter_pk PRIMARY KEY (litter_id),
	CONSTRAINT "UQ_Litter_name_unique" UNIQUE (name)

);
-- ddl-end --
ALTER TABLE mongoose.litter OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.litter DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.litter ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.individual | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.individual CASCADE;
CREATE TABLE mongoose.individual(
	individual_id serial NOT NULL,
	litter_id integer,
	name text NOT NULL,
	sex text,
	date_of_birth timestamp,
	transponder_id text,
	unique_id text,
	collar_weight real,
	is_mongoose boolean NOT NULL DEFAULT TRUE,
	CONSTRAINT individual_pk PRIMARY KEY (individual_id),
	CONSTRAINT unique_name UNIQUE (name),
	CONSTRAINT unique_individual_unique_id UNIQUE (unique_id)

);
-- ddl-end --
ALTER TABLE mongoose.individual OWNER TO postgres;
-- ddl-end --

-- object: litter_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.individual DROP CONSTRAINT IF EXISTS litter_fk CASCADE;
ALTER TABLE mongoose.individual ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id)
REFERENCES mongoose.litter (litter_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pack_history | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack_history CASCADE;
CREATE TABLE mongoose.pack_history(
	pack_history_id serial NOT NULL,
	pack_id integer,
	individual_id integer,
	date_joined timestamp,
	CONSTRAINT packhistory_pk PRIMARY KEY (pack_history_id)

);
-- ddl-end --
ALTER TABLE mongoose.pack_history OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_history DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.pack_history ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_history DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pack_history ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: postgis | type: EXTENSION --
-- DROP EXTENSION IF EXISTS postgis CASCADE;
CREATE EXTENSION postgis
      WITH SCHEMA public;
-- ddl-end --

-- object: mongoose.pack_event | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack_event CASCADE;
CREATE TABLE mongoose.pack_event(
	pack_event_id serial NOT NULL,
	pack_id integer,
	pack_event_code_id integer,
	date date NOT NULL,
	exact text,
	status text,
	cause text,
	location geography,
	comment text,
	CONSTRAINT event_pk PRIMARY KEY (pack_event_id)

);
-- ddl-end --
ALTER TABLE mongoose.pack_event OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_event DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.pack_event ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pack_event_code | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack_event_code CASCADE;
CREATE TABLE mongoose.pack_event_code(
	pack_event_code_id serial NOT NULL,
	code text NOT NULL,
	detail text,
	CONSTRAINT packevnettypes_pk PRIMARY KEY (pack_event_code_id),
	CONSTRAINT uq_code_is_unique UNIQUE (code)

);
-- ddl-end --
ALTER TABLE mongoose.pack_event_code OWNER TO postgres;
-- ddl-end --

-- object: mongoose.individual_event | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.individual_event CASCADE;
CREATE TABLE mongoose.individual_event(
	individual_event_id serial NOT NULL,
	individual_event_code_id integer,
	pack_history_id integer,
	date date NOT NULL,
	exact text,
	start_end text,
	status text,
	cause text,
	affected_litter integer,
	location geography,
	comment text,
	CONSTRAINT individualevent_pk PRIMARY KEY (individual_event_id)

);
-- ddl-end --
ALTER TABLE mongoose.individual_event OWNER TO postgres;
-- ddl-end --

-- object: mongoose.individual_event_code | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.individual_event_code CASCADE;
CREATE TABLE mongoose.individual_event_code(
	individual_event_code_id serial NOT NULL,
	code text NOT NULL,
	CONSTRAINT individualeventcode_pk PRIMARY KEY (individual_event_code_id),
	CONSTRAINT individual_event_code_unique UNIQUE (code)

);
-- ddl-end --
ALTER TABLE mongoose.individual_event_code OWNER TO postgres;
-- ddl-end --

-- object: individual_event_code_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.individual_event DROP CONSTRAINT IF EXISTS individual_event_code_fk CASCADE;
ALTER TABLE mongoose.individual_event ADD CONSTRAINT individual_event_code_fk FOREIGN KEY (individual_event_code_id)
REFERENCES mongoose.individual_event_code (individual_event_code_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.weight | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.weight CASCADE;
CREATE TABLE mongoose.weight(
	weight_id serial NOT NULL,
	weight integer NOT NULL,
	"time" timestamp NOT NULL,
	accuracy smallint,
	session text,
	collar_weight integer,
	location geography(POINT, 4326),
	comment text,
	pack_history_id integer,
	pack_composition_id integer,
	CONSTRAINT weight_pk PRIMARY KEY (weight_id)

);
-- ddl-end --
ALTER TABLE mongoose.weight OWNER TO postgres;
-- ddl-end --

-- object: mongoose.ultrasound | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.ultrasound CASCADE;
CREATE TABLE mongoose.ultrasound(
	ultrasound_id serial NOT NULL,
	observation_date timestamp NOT NULL,
	foetus_number text,
	foetus_id integer NOT NULL,
	foetus_size text,
	cross_view_length decimal,
	cross_view_width decimal,
	long_view_length decimal,
	long_view_width decimal,
	observer text,
	comment text,
	pack_history_id integer,
	CONSTRAINT ultrasound_pk PRIMARY KEY (ultrasound_id)

);
-- ddl-end --
ALTER TABLE mongoose.ultrasound OWNER TO postgres;
-- ddl-end --

-- object: mongoose.radiocollar | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.radiocollar CASCADE;
CREATE TABLE mongoose.radiocollar(
	radiocollar_id serial NOT NULL,
	frequency smallint,
	weight smallint,
	fitted timestamp,
	turned_on timestamp,
	removed timestamp,
	comment text,
	date_entered timestamp,
	pack_history_id integer,
	CONSTRAINT radiocollar_pk PRIMARY KEY (radiocollar_id)

);
-- ddl-end --
ALTER TABLE mongoose.radiocollar OWNER TO postgres;
-- ddl-end --

-- object: pack_event_code_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_event DROP CONSTRAINT IF EXISTS pack_event_code_fk CASCADE;
ALTER TABLE mongoose.pack_event ADD CONSTRAINT pack_event_code_fk FOREIGN KEY (pack_event_code_id)
REFERENCES mongoose.pack_event_code (pack_event_code_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.weight DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.weight ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.ultrasound DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.ultrasound ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.radiocollar DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.radiocollar ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.individual_event DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.individual_event ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus CASCADE;
CREATE TABLE mongoose.oestrus(
	oestrus_id serial NOT NULL,
	pack_history_id integer,
	date date NOT NULL,
	"time" text,
	oestrus_code text,
	guard_id smallint,
	pesterer_id_1 smallint,
	pesterer_id_2 smallint,
	pesterer_id_3 smallint,
	pesterer_id_4 smallint,
	strength smallint,
	confidence smallint,
	location geography,
	comment text,
	CONSTRAINT oestrus_pk PRIMARY KEY (oestrus_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.oestrus ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.litter_event_code | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.litter_event_code CASCADE;
CREATE TABLE mongoose.litter_event_code(
	litter_event_code_id serial NOT NULL,
	code text NOT NULL,
	CONSTRAINT litter_code_pk PRIMARY KEY (litter_event_code_id),
	CONSTRAINT litter_code_unique UNIQUE (code)

);
-- ddl-end --
ALTER TABLE mongoose.litter_event_code OWNER TO postgres;
-- ddl-end --

-- object: mongoose.litter_event | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.litter_event CASCADE;
CREATE TABLE mongoose.litter_event(
	litter_event_id serial NOT NULL,
	litter_id integer,
	litter_event_code_id integer,
	date date NOT NULL,
	cause text,
	exact text,
	last_seen date,
	location geography,
	comment text,
	CONSTRAINT litter_event_pk PRIMARY KEY (litter_event_id)

);
-- ddl-end --
ALTER TABLE mongoose.litter_event OWNER TO postgres;
-- ddl-end --

-- object: litter_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.litter_event DROP CONSTRAINT IF EXISTS litter_fk CASCADE;
ALTER TABLE mongoose.litter_event ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id)
REFERENCES mongoose.litter (litter_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: litter_event_code_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.litter_event DROP CONSTRAINT IF EXISTS litter_event_code_fk CASCADE;
ALTER TABLE mongoose.litter_event ADD CONSTRAINT litter_event_code_fk FOREIGN KEY (litter_event_code_id)
REFERENCES mongoose.litter_event_code (litter_event_code_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.capture | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.capture CASCADE;
CREATE TABLE mongoose.capture(
	capture_id serial NOT NULL,
	pack_history_id integer,
	date date,
	trap_time time,
	process_time time,
	trap_location text,
	bleed_time time,
	release_time time,
	examiner text,
	age text,
	drugs text,
	reproductive_status text,
	teats_ext text,
	ultrasound text,
	foetuses integer,
	foetus_size text,
	weight integer,
	head_width real,
	head_length real,
	body_length integer,
	hind_foot_length real,
	tail_length integer,
	tail_circumference integer,
	ticks integer,
	fleas integer,
	wounds_and_scars text,
	plasma_sample_id text,
	plasma_freeze_time time,
	blood_sample_id text,
	blood_sample_freeze_time time,
	bucket integer,
	white_blood_count text,
	white_blood_freeze_time time,
	white_blood_cell_bucket integer,
	whisker_sample_id text,
	ear_clip bool,
	tail_tip bool,
	"2d4d_photo" bool,
	agd_photo bool,
	blood_sugar real,
	red_cell_percentage real,
	fat_neck_1 text,
	fat_neck_2 text,
	fat_armpit real,
	fat_thigh real,
	testes_length text,
	testes_width text,
	testes_depth real,
	tooth_wear text,
	comment text,
	CONSTRAINT capture_pk PRIMARY KEY (capture_id)

);
-- ddl-end --
ALTER TABLE mongoose.capture OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.capture DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.capture ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.event_log | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.event_log CASCADE;
CREATE TABLE mongoose.event_log(
	event_log_id serial NOT NULL,
	message_id text NOT NULL,
	delivered_count integer NOT NULL DEFAULT 1,
	type text NOT NULL,
	object json NOT NULL,
	success bool,
	error text,
	date_created timestamp NOT NULL DEFAULT current_timestamp,
	CONSTRAINT "EventLog_pk" PRIMARY KEY (event_log_id),
	CONSTRAINT "uniqueMessageId" UNIQUE (message_id)

);
-- ddl-end --
ALTER TABLE mongoose.event_log OWNER TO postgres;
-- ddl-end --

-- object: mongoose.pup_association | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pup_association CASCADE;
CREATE TABLE mongoose.pup_association(
	pup_association_id serial NOT NULL,
	pup_pack_history_id integer,
	escort_id integer,
	pack_composition_id integer,
	date date,
	strength text,
	confidence text,
	location geography,
	comment text,
	comment_editing text,
	CONSTRAINT pup_association_pk PRIMARY KEY (pup_association_id)

);
-- ddl-end --
ALTER TABLE mongoose.pup_association OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_association DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.pup_association ADD CONSTRAINT pack_history_fk FOREIGN KEY (pup_pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.provisioning_data | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.provisioning_data CASCADE;
CREATE TABLE mongoose.provisioning_data(
	provisioning_data_id serial NOT NULL,
	pack_history_id integer,
	litter_id integer,
	date date NOT NULL,
	visit_time text,
	egg_weight text NOT NULL,
	comments text,
	CONSTRAINT provisioning_data_pk PRIMARY KEY (provisioning_data_id)

);
-- ddl-end --
ALTER TABLE mongoose.provisioning_data OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.provisioning_data DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.provisioning_data ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.poo_sample | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.poo_sample CASCADE;
CREATE TABLE mongoose.poo_sample(
	poo_sample_id serial NOT NULL,
	pack_history_id integer,
	sample_number text NOT NULL,
	date date,
	pack_status text,
	emergence_time time,
	collection_time time,
	freezer_time time,
	parasite_sample boolean,
	comment text,
	CONSTRAINT poo_sample_pk PRIMARY KEY (poo_sample_id),
	CONSTRAINT sample_number_unique UNIQUE (sample_number)

);
-- ddl-end --
ALTER TABLE mongoose.poo_sample OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.poo_sample DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.poo_sample ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.meterology | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.meterology CASCADE;
CREATE TABLE mongoose.meterology(
	meterology_id serial NOT NULL,
	date date,
	rain real,
	temp_max real,
	temp_min real,
	humidity_max real,
	humidity_min real,
	temp real,
	observer text,
	CONSTRAINT meterology_pk PRIMARY KEY (meterology_id)

);
-- ddl-end --
ALTER TABLE mongoose.meterology OWNER TO postgres;
-- ddl-end --

-- object: mongoose.maternal_conditioning_litter | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.maternal_conditioning_litter CASCADE;
CREATE TABLE mongoose.maternal_conditioning_litter(
	maternal_conditioning_litter_id serial NOT NULL,
	litter_id integer,
	experiment_number integer NOT NULL,
	pregnancy_check_date date,
	date_started date,
	experiment_type text,
	foetus_age integer,
	"number_T_females" integer,
	"number_C_females" integer,
	notes text,
	CONSTRAINT maternal_conditioning_litter_pk PRIMARY KEY (maternal_conditioning_litter_id)

);
-- ddl-end --
ALTER TABLE mongoose.maternal_conditioning_litter OWNER TO postgres;
-- ddl-end --

-- object: litter_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.maternal_conditioning_litter DROP CONSTRAINT IF EXISTS litter_fk CASCADE;
ALTER TABLE mongoose.maternal_conditioning_litter ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id)
REFERENCES mongoose.litter (litter_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.maternel_conditioning_females | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.maternel_conditioning_females CASCADE;
CREATE TABLE mongoose.maternel_conditioning_females(
	maternel_conditioning_females_id serial NOT NULL,
	pack_history_id integer,
	experiment_type text,
	paired_female_id integer,
	litter_id integer,
	catagory text,
	notes text,
	CONSTRAINT maternel_conditioning_females_pk PRIMARY KEY (maternel_conditioning_females_id)

);
-- ddl-end --
ALTER TABLE mongoose.maternel_conditioning_females OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.maternel_conditioning_females DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.maternel_conditioning_females ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.blood_data | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.blood_data CASCADE;
CREATE TABLE mongoose.blood_data(
	blood_data_id serial NOT NULL,
	individual_id integer,
	date date NOT NULL,
	trap_time time NOT NULL,
	bleed_time interval,
	weight integer NOT NULL,
	release_time time,
	sample text,
	spinning_time time,
	freeze_time time,
	focal text,
	plasma_volume_ul integer,
	comment text,
	CONSTRAINT blood_data_pk PRIMARY KEY (blood_data_id)

);
-- ddl-end --
ALTER TABLE mongoose.blood_data OWNER TO postgres;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.blood_data DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.blood_data ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.hpa_sample | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.hpa_sample CASCADE;
CREATE TABLE mongoose.hpa_sample(
	hpa_sample_id serial NOT NULL,
	individual_id integer,
	date date,
	time_in_trap interval,
	capture_time time,
	first_blood_sample_taken_time interval,
	first_sample_id text,
	first_blood_sample_freezer_time time,
	second_blood_sample_taken_time interval,
	second_blood_sample_id text,
	second_blood_sample_freezer_time time,
	head_width integer,
	weight integer,
	ticks integer,
	CONSTRAINT hpa_sample_pk PRIMARY KEY (hpa_sample_id)

);
-- ddl-end --
ALTER TABLE mongoose.hpa_sample OWNER TO postgres;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.hpa_sample DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.hpa_sample ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.dna_samples | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.dna_samples CASCADE;
CREATE TABLE mongoose.dna_samples(
	dna_samples_id serial NOT NULL,
	pack_history_id integer,
	date date,
	litter_id integer,
	sample_type text,
	tissue text,
	storage text,
	tube_id text,
	age text,
	dispersal text,
	box_slot text,
	comment text,
	CONSTRAINT dna_samples_pk PRIMARY KEY (dna_samples_id)

);
-- ddl-end --
ALTER TABLE mongoose.dna_samples OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.dna_samples DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.dna_samples ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.group_composition | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.group_composition CASCADE;
CREATE TABLE mongoose.group_composition(
	group_composition_id serial NOT NULL,
	pack_id integer,
	date date NOT NULL,
	observer text,
	session text,
	group_status text,
	weather_start text,
	weather_end text,
	males_over_one_year integer,
	females_over_one_year integer,
	males_over_three_months integer,
	females_over_three_months integer,
	male_pups text,
	female_pups text,
	unknown_pups text,
	pups_in_den text,
	comment text,
	location geography,
	CONSTRAINT group_composition_pk PRIMARY KEY (group_composition_id)

);
-- ddl-end --
ALTER TABLE mongoose.group_composition OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.group_composition DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.group_composition ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.babysitting | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.babysitting CASCADE;
CREATE TABLE mongoose.babysitting(
	babysitting_id serial NOT NULL,
	babysitter_pack_history_id integer,
	date date,
	type text,
	litter_id integer,
	time_start time,
	den_distance integer,
	time_end time,
	accuracy integer,
	comment text,
	location geography,
	CONSTRAINT babysitting_pk PRIMARY KEY (babysitting_id)

);
-- ddl-end --
ALTER TABLE mongoose.babysitting OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.babysitting DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.babysitting ADD CONSTRAINT pack_history_fk FOREIGN KEY (babysitter_pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: litter_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.babysitting DROP CONSTRAINT IF EXISTS litter_fk CASCADE;
ALTER TABLE mongoose.babysitting ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id)
REFERENCES mongoose.litter (litter_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.anti_parasite | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.anti_parasite CASCADE;
CREATE TABLE mongoose.anti_parasite(
	anti_parasite_id serial NOT NULL,
	pack_history_id integer,
	started_date date,
	"fecal_sample_A_date" date,
	first_capture_date date,
	experiment_group text,
	"fecal_sample_B_date" date,
	"fecal_sample_C_date" date,
	second_capture date,
	"fecal_sample_D_date" date,
	"fecal_sample_E_date" date,
	"fecal_sample_F_date" date,
	comments text,
	CONSTRAINT anti_parasite_pk PRIMARY KEY (anti_parasite_id)

);
-- ddl-end --
ALTER TABLE mongoose.anti_parasite OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.anti_parasite DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.anti_parasite ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_association DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pup_association ADD CONSTRAINT individual_fk FOREIGN KEY (escort_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.maternel_conditioning_females DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.maternel_conditioning_females ADD CONSTRAINT individual_fk FOREIGN KEY (paired_female_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.ox_shielding_feeding | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.ox_shielding_feeding CASCADE;
CREATE TABLE mongoose.ox_shielding_feeding(
	ox_shielding_feeding_id serial NOT NULL,
	pack_history_id integer,
	date date NOT NULL,
	time_of_day text,
	amount_of_egg integer,
	comments text,
	CONSTRAINT ox_shielding_feeding_pk PRIMARY KEY (ox_shielding_feeding_id)

);
-- ddl-end --
ALTER TABLE mongoose.ox_shielding_feeding OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.ox_shielding_feeding DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.ox_shielding_feeding ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.ox_shielding_male | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.ox_shielding_male CASCADE;
CREATE TABLE mongoose.ox_shielding_male(
	ox_shielding_male_id serial NOT NULL,
	pack_history_id integer,
	status text,
	start_date date,
	stop_date date,
	comment text,
	CONSTRAINT ox_shielding_male_pk PRIMARY KEY (ox_shielding_male_id)

);
-- ddl-end --
ALTER TABLE mongoose.ox_shielding_male OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.ox_shielding_male DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.ox_shielding_male ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.ox_shielding_group | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.ox_shielding_group CASCADE;
CREATE TABLE mongoose.ox_shielding_group(
	ox_shielding_group_id serial NOT NULL,
	pack_history_id integer,
	treatment_group text,
	start_date date,
	comment text,
	CONSTRAINT ox_shielding_group_pk PRIMARY KEY (ox_shielding_group_id)

);
-- ddl-end --
ALTER TABLE mongoose.ox_shielding_group OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.ox_shielding_group DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.ox_shielding_group ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.interaction_outcome | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.interaction_outcome CASCADE;
CREATE TABLE mongoose.interaction_outcome(
	interaction_outcome_id serial NOT NULL,
	outcome text NOT NULL,
	CONSTRAINT ineraction_outcome_pk PRIMARY KEY (interaction_outcome_id)

);
-- ddl-end --
ALTER TABLE mongoose.interaction_outcome OWNER TO postgres;
-- ddl-end --

-- object: mongoose.inter_group_interaction | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.inter_group_interaction CASCADE;
CREATE TABLE mongoose.inter_group_interaction(
	inter_group_interaction_id serial NOT NULL,
	focalpack_id integer,
	secondpack_id integer,
	leader_individual_id integer,
	interaction_outcome_id integer,
	"time" timestamp NOT NULL,
	location geography,
	comment text,
	CONSTRAINT interaction_pk PRIMARY KEY (inter_group_interaction_id)

);
-- ddl-end --
ALTER TABLE mongoose.inter_group_interaction OWNER TO postgres;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.inter_group_interaction DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.inter_group_interaction ADD CONSTRAINT individual_fk FOREIGN KEY (leader_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.alarm | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.alarm CASCADE;
CREATE TABLE mongoose.alarm(
	alarm_id serial NOT NULL,
	date timestamp NOT NULL,
	pack_id integer NOT NULL,
	caller_individual_id integer,
	alarm_cause_id integer NOT NULL,
	others_join text,
	location geography,
	CONSTRAINT alarm_pk PRIMARY KEY (alarm_id)

);
-- ddl-end --
ALTER TABLE mongoose.alarm OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.alarm DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.alarm ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.alarm DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.alarm ADD CONSTRAINT individual_fk FOREIGN KEY (caller_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.alarm_cause | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.alarm_cause CASCADE;
CREATE TABLE mongoose.alarm_cause(
	alarm_cause_id serial NOT NULL,
	cause text NOT NULL,
	CONSTRAINT alarm_cause_pk PRIMARY KEY (alarm_cause_id)

);
-- ddl-end --
ALTER TABLE mongoose.alarm_cause OWNER TO postgres;
-- ddl-end --

-- object: alarm_cause_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.alarm DROP CONSTRAINT IF EXISTS alarm_cause_fk CASCADE;
ALTER TABLE mongoose.alarm ADD CONSTRAINT alarm_cause_fk FOREIGN KEY (alarm_cause_id)
REFERENCES mongoose.alarm_cause (alarm_cause_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pack_move | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack_move CASCADE;
CREATE TABLE mongoose.pack_move(
	pack_move_id serial NOT NULL,
	pack_id integer NOT NULL,
	leader_individual_id integer,
	pack_move_destination_id integer NOT NULL,
	direction text NOT NULL,
	"time" timestamp NOT NULL,
	width integer,
	depth integer,
	number_of_individuals integer,
	location geography,
	CONSTRAINT pack_move_pk PRIMARY KEY (pack_move_id)

);
-- ddl-end --
ALTER TABLE mongoose.pack_move OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_move DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.pack_move ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_move DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pack_move ADD CONSTRAINT individual_fk FOREIGN KEY (leader_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pack_move_destination | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack_move_destination CASCADE;
CREATE TABLE mongoose.pack_move_destination(
	pack_move_destination_id serial NOT NULL,
	destination text NOT NULL,
	CONSTRAINT pack_move_destinations_pk PRIMARY KEY (pack_move_destination_id)

);
-- ddl-end --
ALTER TABLE mongoose.pack_move_destination OWNER TO postgres;
-- ddl-end --

-- object: pack_move_destination_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_move DROP CONSTRAINT IF EXISTS pack_move_destination_fk CASCADE;
ALTER TABLE mongoose.pack_move ADD CONSTRAINT pack_move_destination_fk FOREIGN KEY (pack_move_destination_id)
REFERENCES mongoose.pack_move_destination (pack_move_destination_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_focal | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_focal CASCADE;
CREATE TABLE mongoose.oestrus_focal(
	oestrus_focal_id serial NOT NULL,
	pack_history_id integer NOT NULL,
	depth_of_pack integer,
	number_of_individuals integer,
	width integer,
	"time" timestamp NOT NULL,
	location geography,
	CONSTRAINT oestrus_focal_pk PRIMARY KEY (oestrus_focal_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_focal OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_focal DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.oestrus_focal ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_nearest | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_nearest CASCADE;
CREATE TABLE mongoose.oestrus_nearest(
	oestrus_nearest_id serial NOT NULL,
	close_individuals text,
	nearest_individual_id integer,
	"time" timestamp,
	location geography,
	oestrus_focal_id integer NOT NULL,
	CONSTRAINT oestrus_nearest_pk PRIMARY KEY (oestrus_nearest_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_nearest OWNER TO postgres;
-- ddl-end --

-- object: oestrus_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_nearest DROP CONSTRAINT IF EXISTS oestrus_focal_fk CASCADE;
ALTER TABLE mongoose.oestrus_nearest ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id)
REFERENCES mongoose.oestrus_focal (oestrus_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_nearest DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.oestrus_nearest ADD CONSTRAINT individual_fk FOREIGN KEY (nearest_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_mating | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_mating CASCADE;
CREATE TABLE mongoose.oestrus_mating(
	oestrus_mating_id serial NOT NULL,
	behaviour text,
	with_individual_id integer NOT NULL,
	female_response text,
	male_response text,
	success text,
	"time" timestamp,
	location geography,
	oestrus_focal_id integer NOT NULL,
	CONSTRAINT oestrus_mating_pk PRIMARY KEY (oestrus_mating_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_mating OWNER TO postgres;
-- ddl-end --

-- object: oestrus_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_mating DROP CONSTRAINT IF EXISTS oestrus_focal_fk CASCADE;
ALTER TABLE mongoose.oestrus_mating ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id)
REFERENCES mongoose.oestrus_focal (oestrus_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_mating DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.oestrus_mating ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_male_aggression | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_male_aggression CASCADE;
CREATE TABLE mongoose.oestrus_male_aggression(
	oestrus_male_aggression_id serial NOT NULL,
	receiver_individual_id integer,
	initiator_individual_id integer,
	level text,
	winner text,
	owner text,
	"time" timestamp,
	location geography,
	oestrus_focal_id integer NOT NULL,
	CONSTRAINT oestrus_male_aggression_pk PRIMARY KEY (oestrus_male_aggression_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_male_aggression OWNER TO postgres;
-- ddl-end --

-- object: oestrus_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_male_aggression DROP CONSTRAINT IF EXISTS oestrus_focal_fk CASCADE;
ALTER TABLE mongoose.oestrus_male_aggression ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id)
REFERENCES mongoose.oestrus_focal (oestrus_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_male_aggression DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.oestrus_male_aggression ADD CONSTRAINT individual_fk FOREIGN KEY (initiator_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_affiliation | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_affiliation CASCADE;
CREATE TABLE mongoose.oestrus_affiliation(
	oestrus_affiliation_id serial NOT NULL,
	initiate text,
	with_individual_id integer,
	over text,
	"time" timestamp,
	location geography,
	oestrus_focal_id integer NOT NULL,
	CONSTRAINT oestrus_affiliation_pk PRIMARY KEY (oestrus_affiliation_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_affiliation OWNER TO postgres;
-- ddl-end --

-- object: oestrus_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_affiliation DROP CONSTRAINT IF EXISTS oestrus_focal_fk CASCADE;
ALTER TABLE mongoose.oestrus_affiliation ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id)
REFERENCES mongoose.oestrus_focal (oestrus_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_affiliation DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.oestrus_affiliation ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_aggression | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_aggression CASCADE;
CREATE TABLE mongoose.oestrus_aggression(
	oestrus_aggression_id serial NOT NULL,
	initate text,
	with_individual_id integer,
	level text,
	over text,
	win text,
	"time" timestamp,
	location geography,
	oestrus_focal_id integer,
	CONSTRAINT oestrus_aggression_pk PRIMARY KEY (oestrus_aggression_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_aggression OWNER TO postgres;
-- ddl-end --

-- object: oestrus_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_aggression DROP CONSTRAINT IF EXISTS oestrus_focal_fk CASCADE;
ALTER TABLE mongoose.oestrus_aggression ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id)
REFERENCES mongoose.oestrus_focal (oestrus_focal_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_aggression DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.oestrus_aggression ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pup_focal | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pup_focal CASCADE;
CREATE TABLE mongoose.pup_focal(
	pup_focal_id serial NOT NULL,
	pack_history_id integer NOT NULL,
	depth integer,
	width integer,
	individuals integer,
	"time" timestamp,
	location geography,
	CONSTRAINT pup_focal_pk PRIMARY KEY (pup_focal_id)

);
-- ddl-end --
ALTER TABLE mongoose.pup_focal OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_focal DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.pup_focal ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pup_care | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pup_care CASCADE;
CREATE TABLE mongoose.pup_care(
	pup_care_id serial NOT NULL,
	pup_focal_id integer NOT NULL,
	who_individual_id integer,
	type text,
	"time" timestamp,
	location geography,
	CONSTRAINT pup_care_pk PRIMARY KEY (pup_care_id)

);
-- ddl-end --
ALTER TABLE mongoose.pup_care OWNER TO postgres;
-- ddl-end --

-- object: pup_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_care DROP CONSTRAINT IF EXISTS pup_focal_fk CASCADE;
ALTER TABLE mongoose.pup_care ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id)
REFERENCES mongoose.pup_focal (pup_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_care DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pup_care ADD CONSTRAINT individual_fk FOREIGN KEY (who_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pup_aggression | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pup_aggression CASCADE;
CREATE TABLE mongoose.pup_aggression(
	pup_aggression_id serial NOT NULL,
	pup_focal_id integer NOT NULL,
	with_individual_id integer,
	initiate text,
	level text,
	over text,
	win text,
	"time" timestamp,
	location geography,
	CONSTRAINT pup_aggression_pk PRIMARY KEY (pup_aggression_id)

);
-- ddl-end --
ALTER TABLE mongoose.pup_aggression OWNER TO postgres;
-- ddl-end --

-- object: pup_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_aggression DROP CONSTRAINT IF EXISTS pup_focal_fk CASCADE;
ALTER TABLE mongoose.pup_aggression ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id)
REFERENCES mongoose.pup_focal (pup_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_aggression DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pup_aggression ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pup_feed | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pup_feed CASCADE;
CREATE TABLE mongoose.pup_feed(
	pup_feed_id serial NOT NULL,
	pup_focal_id integer NOT NULL,
	who_individual_id integer,
	size text,
	"time" timestamp,
	location geography,
	CONSTRAINT pup_feed_pk PRIMARY KEY (pup_feed_id)

);
-- ddl-end --
ALTER TABLE mongoose.pup_feed OWNER TO postgres;
-- ddl-end --

-- object: pup_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_feed DROP CONSTRAINT IF EXISTS pup_focal_fk CASCADE;
ALTER TABLE mongoose.pup_feed ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id)
REFERENCES mongoose.pup_focal (pup_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_feed DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pup_feed ADD CONSTRAINT individual_fk FOREIGN KEY (who_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pup_find | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pup_find CASCADE;
CREATE TABLE mongoose.pup_find(
	pup_find_id serial NOT NULL,
	pup_focal_id integer,
	size text,
	"time" timestamp,
	location geography,
	CONSTRAINT pup_find_pk PRIMARY KEY (pup_find_id)

);
-- ddl-end --
ALTER TABLE mongoose.pup_find OWNER TO postgres;
-- ddl-end --

-- object: pup_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_find DROP CONSTRAINT IF EXISTS pup_focal_fk CASCADE;
ALTER TABLE mongoose.pup_find ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id)
REFERENCES mongoose.pup_focal (pup_focal_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pup_nearest | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pup_nearest CASCADE;
CREATE TABLE mongoose.pup_nearest(
	pup_nearest_id serial NOT NULL,
	pup_focal_id integer NOT NULL,
	nearest_individual_id integer,
	list_of_closest_individuals text,
	scan_time timestamp,
	CONSTRAINT pup_nearest_pk PRIMARY KEY (pup_nearest_id)

);
-- ddl-end --
ALTER TABLE mongoose.pup_nearest OWNER TO postgres;
-- ddl-end --

-- object: pup_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_nearest DROP CONSTRAINT IF EXISTS pup_focal_fk CASCADE;
ALTER TABLE mongoose.pup_nearest ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id)
REFERENCES mongoose.pup_focal (pup_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_nearest DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pup_nearest ADD CONSTRAINT individual_fk FOREIGN KEY (nearest_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pregnancy_focal | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pregnancy_focal CASCADE;
CREATE TABLE mongoose.pregnancy_focal(
	pregnancy_focal_id serial NOT NULL,
	pack_history_id integer,
	depth integer,
	width integer,
	individuals integer,
	"time" timestamp,
	location geography,
	CONSTRAINT pregnancy_focal_pk PRIMARY KEY (pregnancy_focal_id)

);
-- ddl-end --
ALTER TABLE mongoose.pregnancy_focal OWNER TO postgres;
-- ddl-end --

-- object: pack_history_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy_focal DROP CONSTRAINT IF EXISTS pack_history_fk CASCADE;
ALTER TABLE mongoose.pregnancy_focal ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id)
REFERENCES mongoose.pack_history (pack_history_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pregnancy_affiliation | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pregnancy_affiliation CASCADE;
CREATE TABLE mongoose.pregnancy_affiliation(
	pregnancy_affiliation_id serial NOT NULL,
	pregnancy_focal_id integer NOT NULL,
	with_individual_id integer,
	initiate text,
	over text,
	"time" timestamp,
	location geography,
	CONSTRAINT pregnancy_affiliation_pk PRIMARY KEY (pregnancy_affiliation_id)

);
-- ddl-end --
ALTER TABLE mongoose.pregnancy_affiliation OWNER TO postgres;
-- ddl-end --

-- object: pregnancy_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy_affiliation DROP CONSTRAINT IF EXISTS pregnancy_focal_fk CASCADE;
ALTER TABLE mongoose.pregnancy_affiliation ADD CONSTRAINT pregnancy_focal_fk FOREIGN KEY (pregnancy_focal_id)
REFERENCES mongoose.pregnancy_focal (pregnancy_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy_affiliation DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pregnancy_affiliation ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pregnancy_aggression | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pregnancy_aggression CASCADE;
CREATE TABLE mongoose.pregnancy_aggression(
	pregnancy_aggression_id serial NOT NULL,
	pregnancy_focal_id integer,
	with_individual_id integer,
	initiate text,
	level text,
	over text,
	win text,
	"time" timestamp,
	location geography,
	CONSTRAINT pregnancy_aggression_pk PRIMARY KEY (pregnancy_aggression_id)

);
-- ddl-end --
ALTER TABLE mongoose.pregnancy_aggression OWNER TO postgres;
-- ddl-end --

-- object: pregnancy_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy_aggression DROP CONSTRAINT IF EXISTS pregnancy_focal_fk CASCADE;
ALTER TABLE mongoose.pregnancy_aggression ADD CONSTRAINT pregnancy_focal_fk FOREIGN KEY (pregnancy_focal_id)
REFERENCES mongoose.pregnancy_focal (pregnancy_focal_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy_aggression DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pregnancy_aggression ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pregnancy_nearest | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pregnancy_nearest CASCADE;
CREATE TABLE mongoose.pregnancy_nearest(
	pregnancy_nearest_id serial NOT NULL,
	pregnancy_focal_id integer NOT NULL,
	nearest_individual_id integer,
	list_of_closest_individuals text,
	scan_time timestamp,
	CONSTRAINT pregnancy_nearest_pk PRIMARY KEY (pregnancy_nearest_id)

);
-- ddl-end --
ALTER TABLE mongoose.pregnancy_nearest OWNER TO postgres;
-- ddl-end --

-- object: pregnancy_focal_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy_nearest DROP CONSTRAINT IF EXISTS pregnancy_focal_fk CASCADE;
ALTER TABLE mongoose.pregnancy_nearest ADD CONSTRAINT pregnancy_focal_fk FOREIGN KEY (pregnancy_focal_id)
REFERENCES mongoose.pregnancy_focal (pregnancy_focal_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy_nearest DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pregnancy_nearest ADD CONSTRAINT individual_fk FOREIGN KEY (nearest_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pack_composition | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack_composition CASCADE;
CREATE TABLE mongoose.pack_composition(
	pack_composition_id serial NOT NULL,
	pack_id integer NOT NULL,
	pups text,
	pups_count integer,
	pregnant_individuals text,
	pregnant_count integer,
	"time" timestamp,
	location geography,
	observer text,
	CONSTRAINT pack_composition_pk PRIMARY KEY (pack_composition_id)

);
-- ddl-end --
ALTER TABLE mongoose.pack_composition OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_composition DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.pack_composition ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_composition_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.weight DROP CONSTRAINT IF EXISTS pack_composition_fk CASCADE;
ALTER TABLE mongoose.weight ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id)
REFERENCES mongoose.pack_composition (pack_composition_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_composition_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pup_association DROP CONSTRAINT IF EXISTS pack_composition_fk CASCADE;
ALTER TABLE mongoose.pup_association ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id)
REFERENCES mongoose.pack_composition (pack_composition_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.mate_guard | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.mate_guard CASCADE;
CREATE TABLE mongoose.mate_guard(
	mate_guard_id serial NOT NULL,
	pack_composition_id integer,
	female_individual_id integer,
	guard_individual_id integer,
	strength text,
	pester text,
	"time" timestamp,
	location geography,
	CONSTRAINT mate_guard_pk PRIMARY KEY (mate_guard_id)

);
-- ddl-end --
ALTER TABLE mongoose.mate_guard OWNER TO postgres;
-- ddl-end --

-- object: pack_composition_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.mate_guard DROP CONSTRAINT IF EXISTS pack_composition_fk CASCADE;
ALTER TABLE mongoose.mate_guard ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id)
REFERENCES mongoose.pack_composition (pack_composition_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pregnancy | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pregnancy CASCADE;
CREATE TABLE mongoose.pregnancy(
	pregnancy_id serial NOT NULL,
	pack_composition_id integer,
	pregnant_individual_id integer,
	CONSTRAINT pregnancy_pk PRIMARY KEY (pregnancy_id)

);
-- ddl-end --
ALTER TABLE mongoose.pregnancy OWNER TO postgres;
-- ddl-end --

-- object: pack_composition_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy DROP CONSTRAINT IF EXISTS pack_composition_fk CASCADE;
ALTER TABLE mongoose.pregnancy ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id)
REFERENCES mongoose.pack_composition (pack_composition_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pregnancy DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.pregnancy ADD CONSTRAINT individual_fk FOREIGN KEY (pregnant_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.mate_guard DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.mate_guard ADD CONSTRAINT individual_fk FOREIGN KEY (female_individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.individual_name_history | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.individual_name_history CASCADE;
CREATE TABLE mongoose.individual_name_history(
	individual_name_history_id serial NOT NULL,
	individual_id integer NOT NULL,
	name text NOT NULL,
	date_changed timestamp,
	CONSTRAINT name_history_pk PRIMARY KEY (individual_name_history_id)

);
-- ddl-end --
ALTER TABLE mongoose.individual_name_history OWNER TO postgres;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.individual_name_history DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.individual_name_history ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.pack_name_history | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.pack_name_history CASCADE;
CREATE TABLE mongoose.pack_name_history(
	pack_name_history_id serial NOT NULL,
	pack_id integer NOT NULL,
	name text NOT NULL,
	date_changed timestamp NOT NULL,
	CONSTRAINT pack_name_history_pk PRIMARY KEY (pack_name_history_id)

);
-- ddl-end --
ALTER TABLE mongoose.pack_name_history OWNER TO postgres;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.pack_name_history DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.pack_name_history ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.inter_group_interaction DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.inter_group_interaction ADD CONSTRAINT pack_fk FOREIGN KEY (focalpack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: interaction_outcome_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.inter_group_interaction DROP CONSTRAINT IF EXISTS interaction_outcome_fk CASCADE;
ALTER TABLE mongoose.inter_group_interaction ADD CONSTRAINT interaction_outcome_fk FOREIGN KEY (interaction_outcome_id)
REFERENCES mongoose.interaction_outcome (interaction_outcome_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_copulation_male | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_copulation_male CASCADE;
CREATE TABLE mongoose.oestrus_copulation_male(
	oestrus_copulation_male_id serial NOT NULL,
	oestrus_id integer,
	individual_id integer,
	CONSTRAINT oestrus_copulation_male_pk PRIMARY KEY (oestrus_copulation_male_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_copulation_male OWNER TO postgres;
-- ddl-end --

-- object: oestrus_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_copulation_male DROP CONSTRAINT IF EXISTS oestrus_fk CASCADE;
ALTER TABLE mongoose.oestrus_copulation_male ADD CONSTRAINT oestrus_fk FOREIGN KEY (oestrus_id)
REFERENCES mongoose.oestrus (oestrus_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: individual_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_copulation_male DROP CONSTRAINT IF EXISTS individual_fk CASCADE;
ALTER TABLE mongoose.oestrus_copulation_male ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id)
REFERENCES mongoose.individual (individual_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: mongoose.oestrus_event | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_event CASCADE;
CREATE TABLE mongoose.oestrus_event(
	oestrus_event_id serial NOT NULL,
	pack_id integer,
	oestrus_event_code_id integer,
	date date NOT NULL,
	oestrus_code text NOT NULL,
	exact text,
	last_seen date,
	location geography,
	comment text,
	CONSTRAINT oestrus_event_pk PRIMARY KEY (oestrus_event_id)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_event OWNER TO postgres;
-- ddl-end --

-- object: mongoose.oestrus_event_code | type: TABLE --
-- DROP TABLE IF EXISTS mongoose.oestrus_event_code CASCADE;
CREATE TABLE mongoose.oestrus_event_code(
	oestrus_event_code_id serial NOT NULL,
	code text NOT NULL,
	CONSTRAINT oestrus_event_code_pk PRIMARY KEY (oestrus_event_code_id),
	CONSTRAINT unique_oestrus_event_code UNIQUE (code)

);
-- ddl-end --
ALTER TABLE mongoose.oestrus_event_code OWNER TO postgres;
-- ddl-end --

-- object: oestrus_event_code_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_event DROP CONSTRAINT IF EXISTS oestrus_event_code_fk CASCADE;
ALTER TABLE mongoose.oestrus_event ADD CONSTRAINT oestrus_event_code_fk FOREIGN KEY (oestrus_event_code_id)
REFERENCES mongoose.oestrus_event_code (oestrus_event_code_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_fk | type: CONSTRAINT --
-- ALTER TABLE mongoose.oestrus_event DROP CONSTRAINT IF EXISTS pack_fk CASCADE;
ALTER TABLE mongoose.oestrus_event ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: pack_secondary | type: CONSTRAINT --
-- ALTER TABLE mongoose.inter_group_interaction DROP CONSTRAINT IF EXISTS pack_secondary CASCADE;
ALTER TABLE mongoose.inter_group_interaction ADD CONSTRAINT pack_secondary FOREIGN KEY (secondpack_id)
REFERENCES mongoose.pack (pack_id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --


-- Appended SQL commands --
-- View: mongoose."WEIGHTS"


CREATE OR REPLACE VIEW mongoose."WEIGHTS" AS
 SELECT p.name AS "GROUP",
    cast(w."time" as date) as "DATE",
    cast(w."time" as time) as "TIME",
    i.name AS "INDIV",
    i.sex as "SEX",
    w.weight as "WEIGHT",
    w.accuracy as "ACCURACY",
    w.session as "SESSION",
    w.collar_weight as "COLLAR",
    w.comment as "COMMENT",    
    st_x(w.location::geometry) AS latitude,
    st_y(w.location::geometry) AS longitude
   FROM mongoose.pack_history ph
     JOIN mongoose.weight w ON w.pack_history_id = ph.pack_history_id
     JOIN mongoose.individual i ON ph.individual_id = i.individual_id
     JOIN mongoose.pack p ON p.pack_id = ph.pack_id
  ORDER BY w."time";

ALTER TABLE mongoose."WEIGHTS"
    OWNER TO postgres;


-- View: mongoose."Ultrasound Data"


CREATE OR REPLACE VIEW mongoose."Ultrasound Data" AS
 SELECT u.observation_date as "DATE",
    i.name AS "INDIV",
    p.name AS "PACK",
    u.foetus_number as "FOETUS NUMBER",
    u.foetus_id,
    u.foetus_size as "FOETUS SIZE",
    u.cross_view_length as "CROSS VIEW LENGTH",
    u.cross_view_width as "CROSS VIEW WIDTH",
    u.long_view_length as "LONG VIEW LENGTH",
    u.long_view_width as "LONG VIEW WIDTH",
    u.observer as "OBSERVER",
    u.comment as "COMMENT"
   FROM mongoose.pack_history ph
     JOIN mongoose.ultrasound u ON u.pack_history_id = ph.pack_history_id
     JOIN mongoose.individual i ON ph.individual_id = i.individual_id
     JOIN mongoose.pack p ON p.pack_id = ph.pack_id
  order by observation_date, i.name, foetus_id;

ALTER TABLE mongoose."Ultrasound Data"
    OWNER TO postgres;


-- View: mongoose."Radiocollar records"



CREATE OR REPLACE VIEW mongoose."Radiocollar records" AS
 SELECT p.name AS "pack name",
    i.name AS "individual name",
    r.frequency,
    r.turned_on,
    r.fitted,
    r.removed,
    r.weight AS "Weight (g)",
    r.comment
   FROM mongoose.pack_history ph
     JOIN mongoose.radiocollar r ON r.pack_history_id = ph.pack_history_id
     JOIN mongoose.individual i ON ph.individual_id = i.individual_id
     JOIN mongoose.pack p ON p.pack_id = ph.pack_id
  ORDER BY r.fitted;

ALTER TABLE mongoose."Radiocollar records"
    OWNER TO postgres;

	-- View: mongoose."Individual Events"
CREATE OR REPLACE VIEW mongoose."Individual Events" AS	
 SELECT p.name AS "pack name",
    i.name AS "individual name",
    i.sex,
   COALESCE (al.name, l.name) AS "Litter name",
    iec.code,
    ie.date,
    ie.exact,
    ie.status,
    ie.start_end,
    ie.cause,
    st_x(ie.location::geometry) AS latitude,
    st_y(ie.location::geometry) AS longitude,
    ie.comment
FROM mongoose.individual_event ie
     JOIN mongoose.pack_history ph ON ie.pack_history_id = ph.pack_history_id
     LEFT JOIN mongoose.individual_event_code iec ON ie.individual_event_code_id = iec.individual_event_code_id
     LEFT JOIN mongoose.individual i ON ph.individual_id = i.individual_id
     LEFT JOIN mongoose.litter l ON l.litter_id = i.litter_id
	 LEFT JOIN mongoose.litter al ON al.litter_id = ie.affected_litter
     LEFT JOIN mongoose.pack p ON p.pack_id = ph.pack_id
ORDER BY ie.date;

ALTER TABLE mongoose."Individual Events"
    OWNER TO postgres;


-- View: mongoose."Pack events"



CREATE OR REPLACE VIEW mongoose."Pack events" AS
 SELECT p.name AS "pack name",
    pec.code,
    pe.date,
    pe.exact,
    pe.status,
    ST_X(pe.location::geometry) as latitude,
    ST_Y(pe.location::geometry) as longitude,
    pe.cause,
    pe.comment   
     FROM mongoose.pack_event pe
     JOIN mongoose.pack p ON p.pack_id = pe.pack_id
     JOIN mongoose.pack_event_code pec ON pe.pack_event_code_id = pec.pack_event_code_id    
  ORDER BY pe.date;

ALTER TABLE mongoose."Pack events"
    OWNER TO postgres;

CREATE OR REPLACE VIEW mongoose."pup association" AS
SELECT date, pup_pack.name as "Group",pup_litter.name as "litter" ,pup.name as "Pup", pup.sex as "Pup Sex", escort.name as "Escort",
escort.sex as "Escort Sex", strength, confidence, comment, pa.comment_editing as "Editing comments",ST_X(location::geometry) as latitude, ST_Y(location::geometry) as longitude
	FROM mongoose.pup_association pa
    join mongoose.pack_history ph on ph.pack_history_id = pa.pup_pack_history_id
    join mongoose.individual as pup on pup.individual_id = ph.individual_id
    join mongoose.pack as pup_pack on pup_pack.pack_id = ph.pack_id
    left join mongoose.individual as escort on escort.individual_id = pa.escort_id
   left join mongoose.litter as pup_litter on pup_litter.litter_id = pup.litter_id;

ALTER TABLE mongoose."pup association"
    OWNER TO postgres;

CREATE OR REPLACE VIEW mongoose."Babysitting Records" AS
SELECT date, p.name as group, l.name as "litter code", i.name as bs, i.sex, type,  time_start, den_distance, time_end, accuracy, comment, ST_X(location::geometry) as latitude, ST_Y(location::geometry) as longitude
	FROM mongoose.babysitting b
    join mongoose.pack_history ph on ph.pack_history_id = b.babysitter_pack_history_id
    join mongoose.individual i on i.individual_id = ph.individual_id
    join mongoose.pack p on p.pack_id = ph.pack_id
    join mongoose.litter l on l.litter_id = b.litter_id;

ALTER TABLE mongoose."Babysitting Records"
    OWNER TO postgres;

CREATE OR REPLACE VIEW mongoose."Group Composition" AS
SELECT date, p.name as pack, observer, session, group_status, weather_start, weather_end, males_over_one_year, females_over_one_year, males_over_three_months, females_over_three_months, male_pups, female_pups, unknown_pups, pups_in_den, comment,   st_x(gc.location::geometry) AS latitude,     st_y(gc.location::geometry) AS longitude
	FROM mongoose.group_composition gc
    join mongoose.pack p on p.pack_id = gc.pack_id;

ALTER TABLE mongoose."Group Composition"
    OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."POO DATABASE" AS
SELECT sample_number as "Sample Number", date as "Date", p.name as "Pack", pack_status as "Pack Status",
 emergence_time as "Emergence Time", freezer_time as "Time in Freezer", i.name as "Individual", collection_time as "Time of Collection",
   CASE WHEN parasite_sample = true THEN 'YES'
 		WHEN parasite_sample = false THEN 'NO'
        ELSE NULL
  END as "Parasite sample taken", comment as "Comments" 

FROM mongoose.poo_sample ps
join mongoose.pack_history ph on ph.pack_history_id = ps.pack_history_id
join mongoose.individual i on i.individual_id = ph.individual_id 
join mongoose.pack p on p.pack_id = ph.pack_id;

ALTER TABLE mongoose."POO DATABASE"
    OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Maternal Condition Experiment: Litters" AS
SELECT experiment_number as "Experiment Number", p.name as "Pack", l.name as "Litter", 
pregnancy_check_date as "Preg check trap date", date_started as "Date started",
 experiment_type as "Type of experiment", foetus_age as "Foetus age at start (weeks)",
  "number_T_females" as "No of T females", "number_C_females" as "No of C females", l.date_formed as "Litter birth date", notes as "Notes"
	FROM mongoose.maternal_conditioning_litter mcl
    join mongoose.litter l on l.litter_id = mcl.litter_id
    join mongoose.pack p on p.pack_id = l.pack_id;

ALTER TABLE MONGOOSE."Maternal Condition Experiment: Litters" 
    OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Maternal Condition Experiment: Females" AS
SELECT  p.name as "Pack" ,l.name as "Litter",  experiment_type as "Experiment type",
focus.name as "Female ID", catagory as "Category", i.name as "Paired female ID", notes as "Notes"
	FROM mongoose.maternel_conditioning_females mcf
   left join mongoose.individual i on i.individual_id = mcf.paired_female_id
    join mongoose.pack_history ph on ph.pack_history_id = mcf.pack_history_id
    join mongoose.individual focus on focus.individual_id = ph.individual_id
    join mongoose.pack p on p.pack_id = ph.pack_id
    join mongoose.litter l on l.litter_id = mcf.litter_id;

ALTER TABLE MONGOOSE."Maternal Condition Experiment: Females" 
    OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Maternal Condition Experiment: provisioning data" AS
SELECT date as "Date",  visit_time as "Visit time", p.name as "Pack" ,l.name as "Litter",
i.name as "Female ID",   egg_weight as "Amount of egg", comments as "Comments"
	FROM mongoose.provisioning_data pd
    join mongoose.pack_history ph on ph.pack_history_id = pd.pack_history_id
    join mongoose.individual i on i.individual_id = ph.individual_id 
    join mongoose.pack p on p.pack_id = ph.pack_id
  left  join mongoose.litter l on l.litter_id = pd.litter_id
  order by date asc;

ALTER TABLE MONGOOSE."Maternal Condition Experiment: provisioning data" 
    OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Jenni's blood data" AS
SELECT date, i.name as "Mongoose",  trap_time as "Trap Time", bleed_time as "Bleed time (from stopwatch)", weight as "Weight",
release_time as "Release time", sample as "Sample", spinning_time as "Spinning time", freeze_time as "Freeze time", focal as Focal,
plasma_volume_ul as "Ammount of pl", comment as "Comment"
	FROM mongoose.blood_data bd
    join mongoose.individual i on i.individual_id = bd.individual_id;

ALTER TABLE MONGOOSE."Jenni's blood data"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."HPA samples" AS
SELECT  date, i.name ,time_in_trap,  capture_time, first_blood_sample_taken_time, first_sample_id, first_blood_sample_freezer_time, second_blood_sample_taken_time, second_blood_sample_id, second_blood_sample_freezer_time, head_width, weight, ticks
	FROM mongoose.hpa_sample hs
    join mongoose.individual i on i.individual_id = hs.individual_id;

ALTER TABLE MONGOOSE."HPA samples"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."DNA Samples" AS
SELECT date as "DATE",sample_type as "SAMPLE TYPE", tissue as "TISSUE", storage as "STORAGE", i.name as "ID",tube_id AS "TUBE ID",age as "AGE", i.sex as "SEX", 
dispersal as "DISPERSAL",p.name as "PACK",  l.name as "LITTER CODE",  box_slot, comment
	FROM mongoose.dna_samples ds
    join mongoose.pack_history ph on ph.pack_history_id = ds.pack_history_id
    join mongoose.individual i on i.individual_id = ph.individual_id
    join mongoose.pack p on p.pack_id = ph.pack_id
  left  join mongoose.litter l on l.litter_id = ds.litter_id
  order by date asc;

ALTER TABLE MONGOOSE."DNA Samples"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Antiparasite experiment" AS
SELECT p.name as pack, i.name as indiv, started_date, "fecal_sample_A_date",
first_capture_date, experiment_group, "fecal_sample_B_date", 
"fecal_sample_C_date", second_capture, "fecal_sample_D_date", "fecal_sample_E_date",
"fecal_sample_F_date", comments as "notes"
	FROM mongoose.anti_parasite ap
    join mongoose.pack_history ph on ph.pack_history_id = ap.pack_history_id
    join mongoose.individual i on i.individual_id = ph.individual_id
    join mongoose.pack p on p.pack_id = ph.pack_id;

ALTER TABLE MONGOOSE."Antiparasite experiment"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."METEROLOGICAL DATA" AS
SELECT date as "DATE", rain as "RAIN_MWEYA", temp_max as "MAX TEMP", temp_min as "MIN TEMP",
temp as "TEMP", humidity_max as "Max humidity", humidity_min as "Min humidity", observer as "OBSERVER"
	FROM mongoose.meterology
    order by date asc;

ALTER TABLE MONGOOSE."METEROLOGICAL DATA"
  OWNER TO postgres;
    
CREATE OR REPLACE VIEW MONGOOSE."Ox shielding experiment - feeding record" AS
SELECT date,  time_of_day as "AM/PM", amount_of_egg as "Amount of egg (g)", i.name as "Female ID", p.name as "Pack", comments as "Comments"
	FROM mongoose.ox_shielding_feeding f
    join mongoose.pack_history ph on ph.pack_history_id = f.pack_history_id
    join mongoose.pack p on p.pack_id = ph.pack_id
    join mongoose.individual i on i.individual_id = ph.individual_id;

ALTER TABLE MONGOOSE."Ox shielding experiment - feeding record"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Ox Shielding Experiment - males being sampled" AS
SELECT p.name as "PACK", i.name as "ID",status as "STATUS", start_date as "DATE START", stop_date as "DATE STOP", comment as "COMMENT"
	FROM mongoose.ox_shielding_male om
    JOIN mongoose.pack_history ph on ph.pack_history_id = om.pack_history_id
    JOIN mongoose.individual i on i.individual_id = ph.individual_id
    JOIN mongoose.pack p on p.pack_id = ph.pack_id;

ALTER TABLE MONGOOSE."Ox Shielding Experiment - males being sampled"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Ox Shielding Experiment - female treatment groups" AS
SELECT  p.name as "Pack", i.name as "ID", treatment_group as "Treatment Group", start_date as "Date Started", comment as "Comment"
	FROM mongoose.ox_shielding_group sg
    JOIN mongoose.pack_history ph on ph.pack_history_id = sg.pack_history_id
    JOIN mongoose.individual i on i.individual_id = ph.individual_id
    JOIN mongoose.pack p on p.pack_id = ph.pack_id;

ALTER TABLE MONGOOSE."Ox Shielding Experiment - female treatment groups"
  OWNER TO postgres;
  
 CREATE OR REPLACE VIEW MONGOOSE."Inter Group Interactions" AS
select focal.name as "focal pack", second.name as "second pack"
    ,leader.name as leader, outcome.outcome as outcome, igi.time as date,igi.comment
	,ST_X(igi.location::geometry) as latitude
    ,ST_Y(igi.location::geometry) as longitude	
	from mongoose.inter_group_interaction igi
	join mongoose.pack focal on focal.pack_id = igi.focalpack_id
	join mongoose.pack second on second.pack_id = igi.secondpack_id
	left join mongoose.individual leader on leader.individual_id = igi.leader_individual_id
	left join mongoose.interaction_outcome outcome on outcome.interaction_outcome_id = igi.interaction_outcome_id
	order by igi.time asc;

ALTER TABLE MONGOOSE."Inter Group Interactions"
  OWNER TO postgres;

 CREATE OR REPLACE VIEW MONGOOSE."Pack Alarms" AS
	select p.name as "Pack", i.name as "Alarm Caller", ac.cause as "Cause", a.others_join, a.date, 
    ST_X(a.location::geometry) as latitude,
    ST_Y(a.location::geometry) as longitude
	from mongoose.alarm a
	join mongoose.individual i on i.individual_id = a.caller_individual_id
	join mongoose.pack p on p.pack_id = a.pack_id
	join mongoose.alarm_cause ac on ac.alarm_cause_id = a.alarm_cause_id
	order by date asc;

ALTER TABLE MONGOOSE."Pack Alarms"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Pack Move" AS
    select p.name as pack, i.name as leader, pm.direction, pmd.destination, pm.time, pm.width, pm.depth, pm.number_of_individuals,
	ST_X(pm.location::geometry) as latitude,
    ST_Y(pm.location::geometry) as longitude
	from mongoose.pack_move pm
	join mongoose.pack p on p.pack_id = pm.pack_id
	join mongoose.individual i on i.individual_id = pm.leader_individual_id
	join mongoose.pack_move_destination pmd on pmd.pack_move_destination_id = pm.pack_move_destination_id
	order by pm.time;

ALTER TABLE MONGOOSE."Pack Move"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."OESTRUS" AS
SELECT o.date as "DATE", o.time as "TIME", p.name as "GROUP", o.oestrus_code as "OESTRUS CODE", i.name as "FEMALE ID",  guard.name as "GUARD"
, pest1.name as "PESTERER ID", pest2.name  as "PESTERER ID 2", pest3.name  as "PESTERER ID 3", pest4.name as "PESTERER ID 4"
, o.strength as "STRENGTH", o.confidence as "CONFIDENCE",
(select string_agg(i.name, ', ')
from mongoose.oestrus_copulation_male  ocm
join mongoose.individual i on i.individual_id = ocm.individual_id
where oestrus_id = o.oestrus_id) as "COPULATION",
o.comment as "COMMENT"
	FROM mongoose.oestrus o
	left join mongoose.pack_history ph on ph.pack_history_id = o.pack_history_id
	left join mongoose.pack p on p.pack_id = ph.pack_id
	left join mongoose.individual i on i.individual_id = ph.individual_id  
	left join mongoose.individual guard on guard.individual_id = o.guard_id
	left join mongoose.individual pest1 on pest1.individual_id = o.pesterer_id_1
	left join mongoose.individual pest2 on pest2.individual_id = o.pesterer_id_2
	left join mongoose.individual pest3 on pest3.individual_id = o.pesterer_id_3
    left join mongoose.individual pest4 on pest4.individual_id = o.pesterer_id_4
	order by o.date asc;

ALTER TABLE MONGOOSE."OESTRUS"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."CAPTURES NEW 2013" AS
SELECT date as "Capture Date",
 i.name as "INDIV",
 i.transponder_id as "TRANSPONDER",
 p.name as "PACK",
-- PACK STATUS
 trap_location AS "TRAP LOCN",
 trap_time as "TRAP TIME",
 process_time as "PROCESS TIME",
 bleed_time AS "BLEED TIME",
 release_time AS "RELEASE TIME",
 age AS "AGE",
 examiner AS "Examiner",
 drugs AS "DRUGS",
 -- SEX
 reproductive_status AS "REPRO STATUS",
 teats_ext AS "TEATS EXT?",
 -- lactating
 ultrasound AS "ULTRASOUND?",
 foetuses AS "FOETUSES",
 foetus_size AS "FOET SIZE",
 -- MILK SAMPLE MK
 -- BUCKET MK
 -- FREEZE TIME MK
 weight AS "WEIGHT",
 head_width AS "HEAD WIDTH",
 head_length AS "HEAD LENGTH",
 body_length AS "BODY LENGTH",
 hind_foot_length AS "HINDFOOT LENGTH",
 tail_length AS "TAIL LENGTH",
 tail_circumference AS "TAIL CIRC",
 -- BUCKET PLxxxA
 -- FREEZE TIME PLxxxB
 -- BUCKET PLxxxB
 -- FREEZE TIME PLxxxA
 ticks AS "TICKS",
 -- BUCKET RBC
 fleas AS "FLEAS",
 wounds_and_scars AS "SCARS / WOUNDS",
 plasma_sample_id AS "PLASMA SAMPLE PL",
 plasma_freeze_time AS "FREEZE TIME PL",
 blood_sample_id AS "BLOOD SAMPLE BL",
 blood_sample_freeze_time AS "FREEZE TIME BL",
 bucket AS "BUCKET PLxxx AND BLxxx",
 white_blood_count AS "White blood WBC",
 white_blood_freeze_time AS "FREEZE TIME WBC",
 white_blood_cell_bucket AS "BUCKET WBC",
 whisker_sample_id AS "WHISKER SAMPLE WSK",
 ear_clip AS "EAR CLIP TAKEN?",
 tail_tip AS "TAIL TIP?",
 "2d4d_photo" AS "2D4D photos?",
 agd_photo AS "AGD photos?",
 blood_sugar AS "Blood sugar",
 red_cell_percentage AS "Red cell percentage",
 fat_neck_1 AS "Fat: neck 1",
 fat_neck_2 AS "Fat: neck 2", 
 fat_armpit AS "Fat: armpit",
 fat_thigh "Fat: thigh",
 comment AS "COMMENTS",
 testes_length AS "TESTES L",
 testes_width AS "TESTES W",
 tooth_wear AS "TOOTH WEAR",
 testes_depth AS "TESTES DEPTH"
	FROM mongoose.capture c
	join mongoose.pack_history ph on c.pack_history_id = ph.pack_history_id
	join mongoose.pack p on p.pack_id = ph.pack_id
	join mongoose.individual i on i.individual_id = ph.individual_id
	order by date asc;
	
ALTER TABLE MONGOOSE."CAPTURES NEW 2013"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Litter Events" AS
SELECT  date, p.name as pack,l.name as litter, lec.code, exact, last_seen, cause, 
    ST_X(le.location::geometry) as latitude,
    ST_Y(le.location::geometry) as longitude,
	comment
	FROM mongoose.litter_event le
	join mongoose.litter_event_code lec on lec.litter_event_code_id = le.litter_event_code_id
	join mongoose.litter l on l.litter_id = le.litter_id
	join mongoose.pack p on p.pack_id = l.pack_id	
	order by date asc;

ALTER TABLE MONGOOSE."Litter Events"
  OWNER TO postgres;

CREATE OR REPLACE VIEW MONGOOSE."Oestrus Events" AS
    select oe.date, p.name as pack_name,oeci.code, oe.oestrus_code,oe.exact, oe.last_seen,
    ST_X(oe.location::geometry) as latitude,
    ST_Y(oe.location::geometry) as longitude,
    oe.comment
    From mongoose.oestrus_event oe
    join mongoose.pack p on p.pack_id = oe.pack_id
    join mongoose.oestrus_event_code oeci on oeci.oestrus_event_code_id = oe.oestrus_event_code_id	
    order by date asc;

ALTER TABLE MONGOOSE."Oestrus Events"
  OWNER TO postgres;

CREATE OR REPLACE VIEW mongoose."NEW LIFE HISTORY" AS
 SELECT "Pack events".date AS "DATE",
    "Pack events"."pack name" AS "PACK",
    NULL::text AS "INDIV",
    NULL::text AS "SEX",
    NULL::text AS "AGE CAT",
    "Pack events".status AS "STATUS",
    NULL::text AS "START/END",
    "Pack events".code AS "CODE",
    "Pack events".exact AS "EXACT",
    NULL::date AS "LSEEN",
    "Pack events".cause AS "CAUSE",
    NULL::text AS "LITTER",
    "Pack events".comment AS "COMMENT",
    "Pack events".latitude AS "Latitude",
    "Pack events".longitude AS "Longitude"
   FROM mongoose."Pack events"
UNION ALL
 SELECT "Litter Events".date AS "DATE",
    "Litter Events".pack AS "PACK",
    NULL::text AS "INDIV",
    NULL::text AS "SEX",
    NULL::text AS "AGE CAT",
    NULL::text AS "STATUS",
    NULL::text AS "START/END",
    "Litter Events".code AS "CODE",
    "Litter Events".exact AS "EXACT",
    "Litter Events".last_seen AS "LSEEN",
    "Litter Events".cause AS "CAUSE",
    "Litter Events".litter AS "LITTER",
    "Litter Events".comment AS "COMMENT",
    "Litter Events".latitude AS "Latitude",
    "Litter Events".longitude AS "Longitude"
   FROM mongoose."Litter Events"
UNION ALL
 SELECT "Inter Group Interactions".date::date AS "DATE",
    "Inter Group Interactions"."focal pack" AS "PACK",
    "Inter Group Interactions".leader AS "INDIV",
    NULL::text AS "SEX",
    NULL::text AS "AGE CAT",
    NULL::text AS "STATUS",
    NULL::text AS "START/END",
    'IGI'::text AS "CODE",
    NULL::text AS "EXACT",
    NULL::date AS "LSEEN",
    "Inter Group Interactions"."second pack" AS "CAUSE",
    NULL::text AS "LITTER",
    "Inter Group Interactions".comment AS "COMMENT",
    "Inter Group Interactions".latitude AS "Latitude",
    "Inter Group Interactions".longitude AS "Longitude"
   FROM mongoose."Inter Group Interactions"
UNION ALL
 SELECT "Individual Events".date AS "DATE",
    "Individual Events"."pack name" AS "PACK",
    "Individual Events"."individual name" AS "INDIV",
    "Individual Events".sex AS "SEX",
    NULL::text AS "AGE CAT",
    "Individual Events".status AS "STATUS",
    "Individual Events".start_end AS "START/END",
    "Individual Events".code AS "CODE",
    "Individual Events".exact AS "EXACT",
    NULL::date AS "LSEEN",
    "Individual Events".cause AS "CAUSE",
    "Individual Events"."Litter name" AS "LITTER",
    "Individual Events".comment AS "COMMENT",
    "Individual Events".latitude AS "Latitude",
    "Individual Events".longitude AS "Longitude"
   FROM mongoose."Individual Events"
UNION ALL
 SELECT "Oestrus Events".date AS "DATE",
    "Oestrus Events".pack_name AS "PACK",
    NULL::text AS "INDIV",
    NULL::text AS "SEX",
    NULL::text AS "AGE CAT",
    NULL::text AS "STATUS",
    NULL::text AS "START/END",
    "Oestrus Events".code AS "CODE",
    "Oestrus Events".exact AS "EXACT",
    "Oestrus Events".last_seen AS "LSEEN",
    NULL::text AS "CAUSE",
    "Oestrus Events".oestrus_code AS "LITTER",
    "Oestrus Events".comment AS "COMMENT",
    "Oestrus Events".latitude AS "Latitude",
    "Oestrus Events".longitude AS "Longitude"
   FROM mongoose."Oestrus Events"
  ORDER BY 1;

ALTER TABLE mongoose."NEW LIFE HISTORY"
    OWNER TO postgres;

INSERT INTO mongoose.individual_event_code (code) VALUES ('BORN');
INSERT INTO mongoose.individual_event_code (code) VALUES ('DIED');
INSERT INTO mongoose.individual_event_code (code) VALUES ('ADIED');
INSERT INTO mongoose.individual_event_code (code) VALUES ('LSEEN');
INSERT INTO mongoose.individual_event_code (code) VALUES ('FSEEN');
INSERT INTO mongoose.individual_event_code (code) VALUES ('STEV');
INSERT INTO mongoose.individual_event_code (code) VALUES ('ENDEV');
INSERT INTO mongoose.individual_event_code (code) VALUES ('LEAVE');
INSERT INTO mongoose.individual_event_code (code) VALUES ('RETURN');
INSERT INTO mongoose.individual_event_code (code) VALUES ('IMM');
INSERT INTO mongoose.individual_event_code (code) VALUES ('EMM');
INSERT INTO mongoose.individual_event_code (code) VALUES ('FPREG');
INSERT INTO mongoose.individual_event_code (code) VALUES ('ABORT');
INSERT INTO mongoose.individual_event_code (code) VALUES ('BIRTH');

INSERT INTO mongoose.litter_event_code(code) VALUES ('UNSUCCESSFUL');
INSERT INTO mongoose.litter_event_code(code) VALUES ('SHORT-LIVED');
INSERT INTO mongoose.litter_event_code(code) VALUES ('SUCCESSFUL');
INSERT INTO mongoose.litter_event_code(code) VALUES ('BORN');

INSERT INTO mongoose.pack_event_code(code, detail) VALUES ('ENDGRP', 'End of pack');
INSERT INTO mongoose.pack_event_code(code, detail) VALUES ('LGRP', 'Lost Pack');
INSERT INTO mongoose.pack_event_code(code, detail) VALUES ('FGRP', 'Found Pack');
INSERT INTO mongoose.pack_event_code(code, detail) VALUES ('NGRP', 'New Pack');


INSERT INTO mongoose.pack (name) VALUES('Unknown');

INSERT INTO mongoose.individual (name) VALUES('Unknown');

INSERT INTO mongoose.interaction_outcome(outcome)
 VALUES ('retreat'),('advance'),('fight-retreat'),('fight-win');

INSERT INTO mongoose.alarm_cause(cause)
 VALUES ('predator'),('other-pack'),('humans'),('other'),('unknown');

INSERT INTO mongoose.pack_move_destination(destination)
 VALUES ('latrine'),('water'),('food'),('nothing'),('den'),('unknown');
---
