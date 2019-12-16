--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.12
-- Dumped by pg_dump version 9.6.2

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: mongoose; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA mongoose;


ALTER SCHEMA mongoose OWNER TO postgres;

--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- Name: postgis; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA public;


--
-- Name: EXTENSION postgis; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis IS 'PostGIS geometry, geography, and raster spatial types and functions';


SET search_path = mongoose, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: anti_parasite; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE anti_parasite (
    anti_parasite_id integer NOT NULL,
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
    comments text
);


ALTER TABLE anti_parasite OWNER TO postgres;

--
-- Name: individual; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE individual (
    individual_id integer NOT NULL,
    litter_id integer,
    name text NOT NULL,
    sex text,
    date_of_birth timestamp without time zone,
    transponder_id text,
    unique_id text,
    collar_weight real,
    is_mongoose boolean DEFAULT true NOT NULL
);


ALTER TABLE individual OWNER TO postgres;

--
-- Name: pack; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack (
    pack_id integer NOT NULL,
    name text NOT NULL,
    pack_created_date timestamp without time zone,
    unique_id text
);


ALTER TABLE pack OWNER TO postgres;

--
-- Name: pack_history; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack_history (
    pack_history_id integer NOT NULL,
    pack_id integer,
    individual_id integer,
    date_joined timestamp without time zone
);


ALTER TABLE pack_history OWNER TO postgres;

--
-- Name: Antiparasite experiment; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Antiparasite experiment" AS
 SELECT p.name AS pack,
    i.name AS indiv,
    ap.started_date,
    ap."fecal_sample_A_date",
    ap.first_capture_date,
    ap.experiment_group,
    ap."fecal_sample_B_date",
    ap."fecal_sample_C_date",
    ap.second_capture,
    ap."fecal_sample_D_date",
    ap."fecal_sample_E_date",
    ap."fecal_sample_F_date",
    ap.comments AS notes
   FROM (((anti_parasite ap
     JOIN pack_history ph ON ((ph.pack_history_id = ap.pack_history_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)));


ALTER TABLE "Antiparasite experiment" OWNER TO postgres;

--
-- Name: babysitting; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE babysitting (
    babysitting_id integer NOT NULL,
    babysitter_pack_history_id integer,
    date date,
    type text,
    litter_id integer,
    time_start time without time zone,
    den_distance integer,
    time_end time without time zone,
    accuracy integer,
    comment text,
    location public.geography
);


ALTER TABLE babysitting OWNER TO postgres;

--
-- Name: litter; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE litter (
    litter_id integer NOT NULL,
    pack_id integer,
    name text NOT NULL,
    unknown_pups_count integer DEFAULT 0 NOT NULL,
    date_formed timestamp without time zone
);


ALTER TABLE litter OWNER TO postgres;

--
-- Name: Babysitting Records; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Babysitting Records" AS
 SELECT b.date,
    p.name AS "group",
    l.name AS "litter code",
    i.name AS bs,
    i.sex,
    b.type,
    b.time_start,
    b.den_distance,
    b.time_end,
    b.accuracy,
    b.comment,
    public.st_x((b.location)::public.geometry) AS latitude,
    public.st_y((b.location)::public.geometry) AS longitude
   FROM ((((babysitting b
     JOIN pack_history ph ON ((ph.pack_history_id = b.babysitter_pack_history_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
     JOIN litter l ON ((l.litter_id = b.litter_id)));


ALTER TABLE "Babysitting Records" OWNER TO postgres;

--
-- Name: capture; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE capture (
    capture_id integer NOT NULL,
    pack_history_id integer,
    date date,
    trap_time time without time zone,
    process_time time without time zone,
    trap_location text,
    bleed_time time without time zone,
    release_time time without time zone,
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
    plasma_freeze_time time without time zone,
    blood_sample_id text,
    blood_sample_freeze_time time without time zone,
    bucket integer,
    white_blood_count text,
    white_blood_freeze_time time without time zone,
    white_blood_cell_bucket integer,
    whisker_sample_id text,
    ear_clip boolean,
    tail_tip boolean,
    "2d4d_photo" boolean,
    agd_photo boolean,
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
    comment text
);


ALTER TABLE capture OWNER TO postgres;

--
-- Name: CAPTURES NEW 2013; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "CAPTURES NEW 2013" AS
 SELECT c.date AS "Capture Date",
    i.name AS "INDIV",
    i.transponder_id AS "TRANSPONDER",
    p.name AS "PACK",
    c.trap_location AS "TRAP LOCN",
    c.trap_time AS "TRAP TIME",
    c.process_time AS "PROCESS TIME",
    c.bleed_time AS "BLEED TIME",
    c.release_time AS "RELEASE TIME",
    c.age AS "AGE",
    c.examiner AS "Examiner",
    c.drugs AS "DRUGS",
    c.reproductive_status AS "REPRO STATUS",
    c.teats_ext AS "TEATS EXT?",
    c.ultrasound AS "ULTRASOUND?",
    c.foetuses AS "FOETUSES",
    c.foetus_size AS "FOET SIZE",
    c.weight AS "WEIGHT",
    c.head_width AS "HEAD WIDTH",
    c.head_length AS "HEAD LENGTH",
    c.body_length AS "BODY LENGTH",
    c.hind_foot_length AS "HINDFOOT LENGTH",
    c.tail_length AS "TAIL LENGTH",
    c.tail_circumference AS "TAIL CIRC",
    c.ticks AS "TICKS",
    c.fleas AS "FLEAS",
    c.wounds_and_scars AS "SCARS / WOUNDS",
    c.plasma_sample_id AS "PLASMA SAMPLE PL",
    c.plasma_freeze_time AS "FREEZE TIME PL",
    c.blood_sample_id AS "BLOOD SAMPLE BL",
    c.blood_sample_freeze_time AS "FREEZE TIME BL",
    c.bucket AS "BUCKET PLxxx AND BLxxx",
    c.white_blood_count AS "White blood WBC",
    c.white_blood_freeze_time AS "FREEZE TIME WBC",
    c.white_blood_cell_bucket AS "BUCKET WBC",
    c.whisker_sample_id AS "WHISKER SAMPLE WSK",
    c.ear_clip AS "EAR CLIP TAKEN?",
    c.tail_tip AS "TAIL TIP?",
    c."2d4d_photo" AS "2D4D photos?",
    c.agd_photo AS "AGD photos?",
    c.blood_sugar AS "Blood sugar",
    c.red_cell_percentage AS "Red cell percentage",
    c.fat_neck_1 AS "Fat: neck 1",
    c.fat_neck_2 AS "Fat: neck 2",
    c.fat_armpit AS "Fat: armpit",
    c.fat_thigh AS "Fat: thigh",
    c.comment AS "COMMENTS",
    c.testes_length AS "TESTES L",
    c.testes_width AS "TESTES W",
    c.tooth_wear AS "TOOTH WEAR",
    c.testes_depth AS "TESTES DEPTH"
   FROM (((capture c
     JOIN pack_history ph ON ((c.pack_history_id = ph.pack_history_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
  ORDER BY c.date;


ALTER TABLE "CAPTURES NEW 2013" OWNER TO postgres;

--
-- Name: dna_samples; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE dna_samples (
    dna_samples_id integer NOT NULL,
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
    comment text
);


ALTER TABLE dna_samples OWNER TO postgres;

--
-- Name: DNA Samples; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "DNA Samples" AS
 SELECT ds.date AS "DATE",
    ds.sample_type AS "SAMPLE TYPE",
    ds.tissue AS "TISSUE",
    ds.storage AS "STORAGE",
    i.name AS "ID",
    ds.tube_id AS "TUBE ID",
    ds.age AS "AGE",
    i.sex AS "SEX",
    ds.dispersal AS "DISPERSAL",
    p.name AS "PACK",
    l.name AS "LITTER CODE",
    ds.box_slot,
    ds.comment
   FROM ((((dna_samples ds
     JOIN pack_history ph ON ((ph.pack_history_id = ds.pack_history_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
     LEFT JOIN litter l ON ((l.litter_id = ds.litter_id)))
  ORDER BY ds.date;


ALTER TABLE "DNA Samples" OWNER TO postgres;

--
-- Name: group_composition; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE group_composition (
    group_composition_id integer NOT NULL,
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
    location public.geography
);


ALTER TABLE group_composition OWNER TO postgres;

--
-- Name: Group Composition; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Group Composition" AS
 SELECT gc.date,
    p.name AS pack,
    gc.observer,
    gc.session,
    gc.group_status,
    gc.weather_start,
    gc.weather_end,
    gc.males_over_one_year,
    gc.females_over_one_year,
    gc.males_over_three_months,
    gc.females_over_three_months,
    gc.male_pups,
    gc.female_pups,
    gc.unknown_pups,
    gc.pups_in_den,
    gc.comment,
    public.st_x((gc.location)::public.geometry) AS latitude,
    public.st_y((gc.location)::public.geometry) AS longitude
   FROM (group_composition gc
     JOIN pack p ON ((p.pack_id = gc.pack_id)));


ALTER TABLE "Group Composition" OWNER TO postgres;

--
-- Name: hpa_sample; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE hpa_sample (
    hpa_sample_id integer NOT NULL,
    individual_id integer,
    date date,
    time_in_trap interval,
    capture_time time without time zone,
    first_blood_sample_taken_time interval,
    first_sample_id text,
    first_blood_sample_freezer_time time without time zone,
    second_blood_sample_taken_time interval,
    second_blood_sample_id text,
    second_blood_sample_freezer_time time without time zone,
    head_width integer,
    weight integer,
    ticks integer
);


ALTER TABLE hpa_sample OWNER TO postgres;

--
-- Name: HPA samples; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "HPA samples" AS
 SELECT hs.date,
    i.name,
    hs.time_in_trap,
    hs.capture_time,
    hs.first_blood_sample_taken_time,
    hs.first_sample_id,
    hs.first_blood_sample_freezer_time,
    hs.second_blood_sample_taken_time,
    hs.second_blood_sample_id,
    hs.second_blood_sample_freezer_time,
    hs.head_width,
    hs.weight,
    hs.ticks
   FROM (hpa_sample hs
     JOIN individual i ON ((i.individual_id = hs.individual_id)));


ALTER TABLE "HPA samples" OWNER TO postgres;

--
-- Name: individual_event; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE individual_event (
    individual_event_id integer NOT NULL,
    individual_event_code_id integer,
    pack_history_id integer,
    date date NOT NULL,
    exact text,
    start_end text,
    status text,
    cause text,
    affected_litter integer,
    location public.geography,
    comment text
);


ALTER TABLE individual_event OWNER TO postgres;

--
-- Name: individual_event_code; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE individual_event_code (
    individual_event_code_id integer NOT NULL,
    code text NOT NULL
);


ALTER TABLE individual_event_code OWNER TO postgres;

--
-- Name: Individual Events; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Individual Events" AS
 SELECT p.name AS "pack name",
    i.name AS "individual name",
    i.sex,
    COALESCE(al.name, l.name) AS "Litter name",
    iec.code,
    ie.date,
    ie.exact,
    ie.status,
    ie.start_end,
    ie.cause,
    public.st_x((ie.location)::public.geometry) AS latitude,
    public.st_y((ie.location)::public.geometry) AS longitude,
    ie.comment
   FROM ((((((individual_event ie
     JOIN pack_history ph ON ((ie.pack_history_id = ph.pack_history_id)))
     LEFT JOIN individual_event_code iec ON ((ie.individual_event_code_id = iec.individual_event_code_id)))
     LEFT JOIN individual i ON ((ph.individual_id = i.individual_id)))
     LEFT JOIN litter l ON ((l.litter_id = i.litter_id)))
     LEFT JOIN litter al ON ((al.litter_id = ie.affected_litter)))
     LEFT JOIN pack p ON ((p.pack_id = ph.pack_id)))
  ORDER BY ie.date;


ALTER TABLE "Individual Events" OWNER TO postgres;

--
-- Name: inter_group_interaction; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE inter_group_interaction (
    inter_group_interaction_id integer NOT NULL,
    focalpack_id integer,
    secondpack_id integer,
    leader_individual_id integer,
    interaction_outcome_id integer,
    "time" timestamp without time zone NOT NULL,
    location public.geography,
    comment text
);


ALTER TABLE inter_group_interaction OWNER TO postgres;

--
-- Name: interaction_outcome; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE interaction_outcome (
    interaction_outcome_id integer NOT NULL,
    outcome text NOT NULL
);


ALTER TABLE interaction_outcome OWNER TO postgres;

--
-- Name: Inter Group Interactions; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Inter Group Interactions" AS
 SELECT focal.name AS "focal pack",
    second.name AS "second pack",
    leader.name AS leader,
    outcome.outcome,
    igi."time" AS date,
    igi.comment,
    public.st_x((igi.location)::public.geometry) AS latitude,
    public.st_y((igi.location)::public.geometry) AS longitude
   FROM ((((inter_group_interaction igi
     JOIN pack focal ON ((focal.pack_id = igi.focalpack_id)))
     JOIN pack second ON ((second.pack_id = igi.secondpack_id)))
     LEFT JOIN individual leader ON ((leader.individual_id = igi.leader_individual_id)))
     LEFT JOIN interaction_outcome outcome ON ((outcome.interaction_outcome_id = igi.interaction_outcome_id)))
  ORDER BY igi."time";


ALTER TABLE "Inter Group Interactions" OWNER TO postgres;

--
-- Name: blood_data; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE blood_data (
    blood_data_id integer NOT NULL,
    individual_id integer,
    date date NOT NULL,
    trap_time time without time zone NOT NULL,
    bleed_time interval,
    weight integer NOT NULL,
    release_time time without time zone,
    sample text,
    spinning_time time without time zone,
    freeze_time time without time zone,
    focal text,
    plasma_volume_ul integer,
    comment text
);


ALTER TABLE blood_data OWNER TO postgres;

--
-- Name: Jenni's blood data; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Jenni's blood data" AS
 SELECT bd.date,
    i.name AS "Mongoose",
    bd.trap_time AS "Trap Time",
    bd.bleed_time AS "Bleed time (from stopwatch)",
    bd.weight AS "Weight",
    bd.release_time AS "Release time",
    bd.sample AS "Sample",
    bd.spinning_time AS "Spinning time",
    bd.freeze_time AS "Freeze time",
    bd.focal,
    bd.plasma_volume_ul AS "Ammount of pl",
    bd.comment AS "Comment"
   FROM (blood_data bd
     JOIN individual i ON ((i.individual_id = bd.individual_id)));


ALTER TABLE "Jenni's blood data" OWNER TO postgres;

--
-- Name: litter_event; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE litter_event (
    litter_event_id integer NOT NULL,
    litter_id integer,
    litter_event_code_id integer,
    date date NOT NULL,
    cause text,
    exact text,
    last_seen date,
    location public.geography,
    comment text
);


ALTER TABLE litter_event OWNER TO postgres;

--
-- Name: litter_event_code; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE litter_event_code (
    litter_event_code_id integer NOT NULL,
    code text NOT NULL
);


ALTER TABLE litter_event_code OWNER TO postgres;

--
-- Name: Litter Events; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Litter Events" AS
 SELECT le.date,
    p.name AS pack,
    l.name AS litter,
    lec.code,
    le.exact,
    le.last_seen,
    le.cause,
    public.st_x((le.location)::public.geometry) AS latitude,
    public.st_y((le.location)::public.geometry) AS longitude,
    le.comment
   FROM (((litter_event le
     JOIN litter_event_code lec ON ((lec.litter_event_code_id = le.litter_event_code_id)))
     JOIN litter l ON ((l.litter_id = le.litter_id)))
     JOIN pack p ON ((p.pack_id = l.pack_id)))
  ORDER BY le.date;


ALTER TABLE "Litter Events" OWNER TO postgres;

--
-- Name: meterology; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE meterology (
    meterology_id integer NOT NULL,
    date date,
    rain real,
    temp_max real,
    temp_min real,
    humidity_max real,
    humidity_min real,
    temp real,
    observer text
);


ALTER TABLE meterology OWNER TO postgres;

--
-- Name: METEROLOGICAL DATA; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "METEROLOGICAL DATA" AS
 SELECT meterology.date AS "DATE",
    meterology.rain AS "RAIN_MWEYA",
    meterology.temp_max AS "MAX TEMP",
    meterology.temp_min AS "MIN TEMP",
    meterology.temp AS "TEMP",
    meterology.humidity_max AS "Max humidity",
    meterology.humidity_min AS "Min humidity",
    meterology.observer AS "OBSERVER"
   FROM meterology
  ORDER BY meterology.date;


ALTER TABLE "METEROLOGICAL DATA" OWNER TO postgres;

--
-- Name: maternel_conditioning_females; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE maternel_conditioning_females (
    maternel_conditioning_females_id integer NOT NULL,
    pack_history_id integer,
    experiment_type text,
    paired_female_id integer,
    litter_id integer,
    catagory text,
    notes text
);


ALTER TABLE maternel_conditioning_females OWNER TO postgres;

--
-- Name: Maternal Condition Experiment: Females; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Maternal Condition Experiment: Females" AS
 SELECT p.name AS "Pack",
    l.name AS "Litter",
    mcf.experiment_type AS "Experiment type",
    focus.name AS "Female ID",
    mcf.catagory AS "Category",
    i.name AS "Paired female ID",
    mcf.notes AS "Notes"
   FROM (((((maternel_conditioning_females mcf
     LEFT JOIN individual i ON ((i.individual_id = mcf.paired_female_id)))
     JOIN pack_history ph ON ((ph.pack_history_id = mcf.pack_history_id)))
     JOIN individual focus ON ((focus.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
     JOIN litter l ON ((l.litter_id = mcf.litter_id)));


ALTER TABLE "Maternal Condition Experiment: Females" OWNER TO postgres;

--
-- Name: maternal_conditioning_litter; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE maternal_conditioning_litter (
    maternal_conditioning_litter_id integer NOT NULL,
    litter_id integer,
    experiment_number integer NOT NULL,
    pregnancy_check_date date,
    date_started date,
    experiment_type text,
    foetus_age integer,
    "number_T_females" integer,
    "number_C_females" integer,
    notes text
);


ALTER TABLE maternal_conditioning_litter OWNER TO postgres;

--
-- Name: Maternal Condition Experiment: Litters; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Maternal Condition Experiment: Litters" AS
 SELECT mcl.experiment_number AS "Experiment Number",
    p.name AS "Pack",
    l.name AS "Litter",
    mcl.pregnancy_check_date AS "Preg check trap date",
    mcl.date_started AS "Date started",
    mcl.experiment_type AS "Type of experiment",
    mcl.foetus_age AS "Foetus age at start (weeks)",
    mcl."number_T_females" AS "No of T females",
    mcl."number_C_females" AS "No of C females",
    l.date_formed AS "Litter birth date",
    mcl.notes AS "Notes"
   FROM ((maternal_conditioning_litter mcl
     JOIN litter l ON ((l.litter_id = mcl.litter_id)))
     JOIN pack p ON ((p.pack_id = l.pack_id)));


ALTER TABLE "Maternal Condition Experiment: Litters" OWNER TO postgres;

--
-- Name: provisioning_data; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE provisioning_data (
    provisioning_data_id integer NOT NULL,
    pack_history_id integer,
    litter_id integer,
    date date NOT NULL,
    visit_time text,
    egg_weight text NOT NULL,
    comments text
);


ALTER TABLE provisioning_data OWNER TO postgres;

--
-- Name: Maternal Condition Experiment: provisioning data; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Maternal Condition Experiment: provisioning data" AS
 SELECT pd.date AS "Date",
    pd.visit_time AS "Visit time",
    p.name AS "Pack",
    l.name AS "Litter",
    i.name AS "Female ID",
    pd.egg_weight AS "Amount of egg",
    pd.comments AS "Comments"
   FROM ((((provisioning_data pd
     JOIN pack_history ph ON ((ph.pack_history_id = pd.pack_history_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
     LEFT JOIN litter l ON ((l.litter_id = pd.litter_id)))
  ORDER BY pd.date;


ALTER TABLE "Maternal Condition Experiment: provisioning data" OWNER TO postgres;

--
-- Name: oestrus_event; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_event (
    oestrus_event_id integer NOT NULL,
    pack_id integer,
    oestrus_event_code_id integer,
    date date NOT NULL,
    oestrus_code text NOT NULL,
    exact text,
    last_seen date,
    location public.geography,
    comment text
);


ALTER TABLE oestrus_event OWNER TO postgres;

--
-- Name: oestrus_event_code; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_event_code (
    oestrus_event_code_id integer NOT NULL,
    code text NOT NULL
);


ALTER TABLE oestrus_event_code OWNER TO postgres;

--
-- Name: Oestrus Events; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Oestrus Events" AS
 SELECT oe.date,
    p.name AS pack_name,
    oeci.code,
    oe.oestrus_code,
    oe.exact,
    oe.last_seen,
    public.st_x((oe.location)::public.geometry) AS latitude,
    public.st_y((oe.location)::public.geometry) AS longitude,
    oe.comment
   FROM ((oestrus_event oe
     JOIN pack p ON ((p.pack_id = oe.pack_id)))
     JOIN oestrus_event_code oeci ON ((oeci.oestrus_event_code_id = oe.oestrus_event_code_id)))
  ORDER BY oe.date;


ALTER TABLE "Oestrus Events" OWNER TO postgres;

--
-- Name: pack_event; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack_event (
    pack_event_id integer NOT NULL,
    pack_id integer,
    pack_event_code_id integer,
    date date NOT NULL,
    exact text,
    status text,
    cause text,
    location public.geography,
    comment text
);


ALTER TABLE pack_event OWNER TO postgres;

--
-- Name: pack_event_code; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack_event_code (
    pack_event_code_id integer NOT NULL,
    code text NOT NULL,
    detail text
);


ALTER TABLE pack_event_code OWNER TO postgres;

--
-- Name: Pack events; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Pack events" AS
 SELECT p.name AS "pack name",
    pec.code,
    pe.date,
    pe.exact,
    pe.status,
    public.st_x((pe.location)::public.geometry) AS latitude,
    public.st_y((pe.location)::public.geometry) AS longitude,
    pe.cause,
    pe.comment
   FROM ((pack_event pe
     JOIN pack p ON ((p.pack_id = pe.pack_id)))
     JOIN pack_event_code pec ON ((pe.pack_event_code_id = pec.pack_event_code_id)))
  ORDER BY pe.date;


ALTER TABLE "Pack events" OWNER TO postgres;

--
-- Name: NEW LIFE HISTORY; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "NEW LIFE HISTORY" AS
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
   FROM "Pack events"
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
   FROM "Litter Events"
UNION ALL
 SELECT ("Inter Group Interactions".date)::date AS "DATE",
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
   FROM "Inter Group Interactions"
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
   FROM "Individual Events"
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
   FROM "Oestrus Events"
  ORDER BY 1;


ALTER TABLE "NEW LIFE HISTORY" OWNER TO postgres;

--
-- Name: oestrus; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus (
    oestrus_id integer NOT NULL,
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
    location public.geography,
    comment text
);


ALTER TABLE oestrus OWNER TO postgres;

--
-- Name: oestrus_copulation_male; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_copulation_male (
    oestrus_copulation_male_id integer NOT NULL,
    oestrus_id integer,
    individual_id integer
);


ALTER TABLE oestrus_copulation_male OWNER TO postgres;

--
-- Name: OESTRUS; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "OESTRUS" AS
 SELECT o.date AS "DATE",
    o."time" AS "TIME",
    p.name AS "GROUP",
    o.oestrus_code AS "OESTRUS CODE",
    i.name AS "FEMALE ID",
    guard.name AS "GUARD",
    pest1.name AS "PESTERER ID",
    pest2.name AS "PESTERER ID 2",
    pest3.name AS "PESTERER ID 3",
    pest4.name AS "PESTERER ID 4",
    o.strength AS "STRENGTH",
    o.confidence AS "CONFIDENCE",
    ( SELECT string_agg(i_1.name, ', '::text) AS string_agg
           FROM (oestrus_copulation_male ocm
             JOIN individual i_1 ON ((i_1.individual_id = ocm.individual_id)))
          WHERE (ocm.oestrus_id = o.oestrus_id)) AS "COPULATION",
    o.comment AS "COMMENT"
   FROM ((((((((oestrus o
     LEFT JOIN pack_history ph ON ((ph.pack_history_id = o.pack_history_id)))
     LEFT JOIN pack p ON ((p.pack_id = ph.pack_id)))
     LEFT JOIN individual i ON ((i.individual_id = ph.individual_id)))
     LEFT JOIN individual guard ON ((guard.individual_id = o.guard_id)))
     LEFT JOIN individual pest1 ON ((pest1.individual_id = o.pesterer_id_1)))
     LEFT JOIN individual pest2 ON ((pest2.individual_id = o.pesterer_id_2)))
     LEFT JOIN individual pest3 ON ((pest3.individual_id = o.pesterer_id_3)))
     LEFT JOIN individual pest4 ON ((pest4.individual_id = o.pesterer_id_4)))
  ORDER BY o.date;


ALTER TABLE "OESTRUS" OWNER TO postgres;

--
-- Name: ox_shielding_group; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE ox_shielding_group (
    ox_shielding_group_id integer NOT NULL,
    pack_history_id integer,
    treatment_group text,
    start_date date,
    comment text
);


ALTER TABLE ox_shielding_group OWNER TO postgres;

--
-- Name: Ox Shielding Experiment - female treatment groups; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Ox Shielding Experiment - female treatment groups" AS
 SELECT p.name AS "Pack",
    i.name AS "ID",
    sg.treatment_group AS "Treatment Group",
    sg.start_date AS "Date Started",
    sg.comment AS "Comment"
   FROM (((ox_shielding_group sg
     JOIN pack_history ph ON ((ph.pack_history_id = sg.pack_history_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)));


ALTER TABLE "Ox Shielding Experiment - female treatment groups" OWNER TO postgres;

--
-- Name: ox_shielding_male; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE ox_shielding_male (
    ox_shielding_male_id integer NOT NULL,
    pack_history_id integer,
    status text,
    start_date date,
    stop_date date,
    comment text
);


ALTER TABLE ox_shielding_male OWNER TO postgres;

--
-- Name: Ox Shielding Experiment - males being sampled; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Ox Shielding Experiment - males being sampled" AS
 SELECT p.name AS "PACK",
    i.name AS "ID",
    om.status AS "STATUS",
    om.start_date AS "DATE START",
    om.stop_date AS "DATE STOP",
    om.comment AS "COMMENT"
   FROM (((ox_shielding_male om
     JOIN pack_history ph ON ((ph.pack_history_id = om.pack_history_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)));


ALTER TABLE "Ox Shielding Experiment - males being sampled" OWNER TO postgres;

--
-- Name: ox_shielding_feeding; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE ox_shielding_feeding (
    ox_shielding_feeding_id integer NOT NULL,
    pack_history_id integer,
    date date NOT NULL,
    time_of_day text,
    amount_of_egg integer,
    comments text
);


ALTER TABLE ox_shielding_feeding OWNER TO postgres;

--
-- Name: Ox shielding experiment - feeding record; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Ox shielding experiment - feeding record" AS
 SELECT f.date,
    f.time_of_day AS "AM/PM",
    f.amount_of_egg AS "Amount of egg (g)",
    i.name AS "Female ID",
    p.name AS "Pack",
    f.comments AS "Comments"
   FROM (((ox_shielding_feeding f
     JOIN pack_history ph ON ((ph.pack_history_id = f.pack_history_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)));


ALTER TABLE "Ox shielding experiment - feeding record" OWNER TO postgres;

--
-- Name: poo_sample; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE poo_sample (
    poo_sample_id integer NOT NULL,
    pack_history_id integer,
    sample_number text NOT NULL,
    date date,
    pack_status text,
    emergence_time time without time zone,
    collection_time time without time zone,
    freezer_time time without time zone,
    parasite_sample boolean,
    comment text
);


ALTER TABLE poo_sample OWNER TO postgres;

--
-- Name: POO DATABASE; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "POO DATABASE" AS
 SELECT ps.sample_number AS "Sample Number",
    ps.date AS "Date",
    p.name AS "Pack",
    ps.pack_status AS "Pack Status",
    ps.emergence_time AS "Emergence Time",
    ps.freezer_time AS "Time in Freezer",
    i.name AS "Individual",
    ps.collection_time AS "Time of Collection",
        CASE
            WHEN (ps.parasite_sample = true) THEN 'YES'::text
            WHEN (ps.parasite_sample = false) THEN 'NO'::text
            ELSE NULL::text
        END AS "Parasite sample taken",
    ps.comment AS "Comments"
   FROM (((poo_sample ps
     JOIN pack_history ph ON ((ph.pack_history_id = ps.pack_history_id)))
     JOIN individual i ON ((i.individual_id = ph.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)));


ALTER TABLE "POO DATABASE" OWNER TO postgres;

--
-- Name: alarm; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE alarm (
    alarm_id integer NOT NULL,
    date timestamp without time zone NOT NULL,
    pack_id integer NOT NULL,
    caller_individual_id integer,
    alarm_cause_id integer NOT NULL,
    others_join text,
    location public.geography
);


ALTER TABLE alarm OWNER TO postgres;

--
-- Name: alarm_cause; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE alarm_cause (
    alarm_cause_id integer NOT NULL,
    cause text NOT NULL
);


ALTER TABLE alarm_cause OWNER TO postgres;

--
-- Name: Pack Alarms; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Pack Alarms" AS
 SELECT p.name AS "Pack",
    i.name AS "Alarm Caller",
    ac.cause AS "Cause",
    a.others_join,
    a.date,
    public.st_x((a.location)::public.geometry) AS latitude,
    public.st_y((a.location)::public.geometry) AS longitude
   FROM (((alarm a
     JOIN individual i ON ((i.individual_id = a.caller_individual_id)))
     JOIN pack p ON ((p.pack_id = a.pack_id)))
     JOIN alarm_cause ac ON ((ac.alarm_cause_id = a.alarm_cause_id)))
  ORDER BY a.date;


ALTER TABLE "Pack Alarms" OWNER TO postgres;

--
-- Name: pack_move; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack_move (
    pack_move_id integer NOT NULL,
    pack_id integer NOT NULL,
    leader_individual_id integer,
    pack_move_destination_id integer NOT NULL,
    direction text NOT NULL,
    "time" timestamp without time zone NOT NULL,
    width integer,
    depth integer,
    number_of_individuals integer,
    location public.geography
);


ALTER TABLE pack_move OWNER TO postgres;

--
-- Name: pack_move_destination; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack_move_destination (
    pack_move_destination_id integer NOT NULL,
    destination text NOT NULL
);


ALTER TABLE pack_move_destination OWNER TO postgres;

--
-- Name: Pack Move; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Pack Move" AS
 SELECT p.name AS pack,
    i.name AS leader,
    pm.direction,
    pmd.destination,
    pm."time",
    pm.width,
    pm.depth,
    pm.number_of_individuals,
    public.st_x((pm.location)::public.geometry) AS latitude,
    public.st_y((pm.location)::public.geometry) AS longitude
   FROM (((pack_move pm
     JOIN pack p ON ((p.pack_id = pm.pack_id)))
     JOIN individual i ON ((i.individual_id = pm.leader_individual_id)))
     JOIN pack_move_destination pmd ON ((pmd.pack_move_destination_id = pm.pack_move_destination_id)))
  ORDER BY pm."time";


ALTER TABLE "Pack Move" OWNER TO postgres;

--
-- Name: radiocollar; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE radiocollar (
    radiocollar_id integer NOT NULL,
    frequency smallint,
    weight smallint,
    fitted timestamp without time zone,
    turned_on timestamp without time zone,
    removed timestamp without time zone,
    comment text,
    date_entered timestamp without time zone,
    pack_history_id integer
);


ALTER TABLE radiocollar OWNER TO postgres;

--
-- Name: Radiocollar records; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Radiocollar records" AS
 SELECT p.name AS "pack name",
    i.name AS "individual name",
    r.frequency,
    r.turned_on,
    r.fitted,
    r.removed,
    r.weight AS "Weight (g)",
    r.comment
   FROM (((pack_history ph
     JOIN radiocollar r ON ((r.pack_history_id = ph.pack_history_id)))
     JOIN individual i ON ((ph.individual_id = i.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
  ORDER BY r.fitted;


ALTER TABLE "Radiocollar records" OWNER TO postgres;

--
-- Name: ultrasound; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE ultrasound (
    ultrasound_id integer NOT NULL,
    observation_date timestamp without time zone NOT NULL,
    foetus_number text,
    foetus_id integer NOT NULL,
    foetus_size text,
    cross_view_length numeric,
    cross_view_width numeric,
    long_view_length numeric,
    long_view_width numeric,
    observer text,
    comment text,
    pack_history_id integer
);


ALTER TABLE ultrasound OWNER TO postgres;

--
-- Name: Ultrasound Data; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "Ultrasound Data" AS
 SELECT u.observation_date AS "DATE",
    i.name AS "INDIV",
    p.name AS "PACK",
    u.foetus_number AS "FOETUS NUMBER",
    u.foetus_id,
    u.foetus_size AS "FOETUS SIZE",
    u.cross_view_length AS "CROSS VIEW LENGTH",
    u.cross_view_width AS "CROSS VIEW WIDTH",
    u.long_view_length AS "LONG VIEW LENGTH",
    u.long_view_width AS "LONG VIEW WIDTH",
    u.observer AS "OBSERVER",
    u.comment AS "COMMENT"
   FROM (((pack_history ph
     JOIN ultrasound u ON ((u.pack_history_id = ph.pack_history_id)))
     JOIN individual i ON ((ph.individual_id = i.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
  ORDER BY u.observation_date, i.name, u.foetus_id;


ALTER TABLE "Ultrasound Data" OWNER TO postgres;

--
-- Name: weight; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE weight (
    weight_id integer NOT NULL,
    weight integer NOT NULL,
    "time" timestamp without time zone NOT NULL,
    accuracy smallint,
    session text,
    collar_weight integer,
    location public.geography(Point,4326),
    comment text,
    pack_history_id integer,
    pack_composition_id integer
);


ALTER TABLE weight OWNER TO postgres;

--
-- Name: WEIGHTS; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "WEIGHTS" AS
 SELECT p.name AS "GROUP",
    (w."time")::date AS "DATE",
    (w."time")::time without time zone AS "TIME",
    i.name AS "INDIV",
    i.sex AS "SEX",
    w.weight AS "WEIGHT",
    w.accuracy AS "ACCURACY",
    w.session AS "SESSION",
    w.collar_weight AS "COLLAR",
    w.comment AS "COMMENT",
    public.st_x((w.location)::public.geometry) AS latitude,
    public.st_y((w.location)::public.geometry) AS longitude
   FROM (((pack_history ph
     JOIN weight w ON ((w.pack_history_id = ph.pack_history_id)))
     JOIN individual i ON ((ph.individual_id = i.individual_id)))
     JOIN pack p ON ((p.pack_id = ph.pack_id)))
  ORDER BY w."time";


ALTER TABLE "WEIGHTS" OWNER TO postgres;

--
-- Name: alarm_alarm_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE alarm_alarm_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE alarm_alarm_id_seq OWNER TO postgres;

--
-- Name: alarm_alarm_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE alarm_alarm_id_seq OWNED BY alarm.alarm_id;


--
-- Name: alarm_cause_alarm_cause_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE alarm_cause_alarm_cause_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE alarm_cause_alarm_cause_id_seq OWNER TO postgres;

--
-- Name: alarm_cause_alarm_cause_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE alarm_cause_alarm_cause_id_seq OWNED BY alarm_cause.alarm_cause_id;


--
-- Name: anti_parasite_anti_parasite_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE anti_parasite_anti_parasite_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE anti_parasite_anti_parasite_id_seq OWNER TO postgres;

--
-- Name: anti_parasite_anti_parasite_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE anti_parasite_anti_parasite_id_seq OWNED BY anti_parasite.anti_parasite_id;


--
-- Name: babysitting_babysitting_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE babysitting_babysitting_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE babysitting_babysitting_id_seq OWNER TO postgres;

--
-- Name: babysitting_babysitting_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE babysitting_babysitting_id_seq OWNED BY babysitting.babysitting_id;


--
-- Name: blood_data_blood_data_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE blood_data_blood_data_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE blood_data_blood_data_id_seq OWNER TO postgres;

--
-- Name: blood_data_blood_data_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE blood_data_blood_data_id_seq OWNED BY blood_data.blood_data_id;


--
-- Name: capture_capture_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE capture_capture_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE capture_capture_id_seq OWNER TO postgres;

--
-- Name: capture_capture_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE capture_capture_id_seq OWNED BY capture.capture_id;


--
-- Name: dna_samples_dna_samples_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE dna_samples_dna_samples_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE dna_samples_dna_samples_id_seq OWNER TO postgres;

--
-- Name: dna_samples_dna_samples_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE dna_samples_dna_samples_id_seq OWNED BY dna_samples.dna_samples_id;


--
-- Name: event_log; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE event_log (
    event_log_id integer NOT NULL,
    message_id text NOT NULL,
    delivered_count integer DEFAULT 1 NOT NULL,
    type text NOT NULL,
    object json NOT NULL,
    success boolean,
    error text,
    date_created timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE event_log OWNER TO postgres;

--
-- Name: event_log_event_log_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE event_log_event_log_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE event_log_event_log_id_seq OWNER TO postgres;

--
-- Name: event_log_event_log_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE event_log_event_log_id_seq OWNED BY event_log.event_log_id;


--
-- Name: group_composition_group_composition_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE group_composition_group_composition_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE group_composition_group_composition_id_seq OWNER TO postgres;

--
-- Name: group_composition_group_composition_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE group_composition_group_composition_id_seq OWNED BY group_composition.group_composition_id;


--
-- Name: hpa_sample_hpa_sample_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE hpa_sample_hpa_sample_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hpa_sample_hpa_sample_id_seq OWNER TO postgres;

--
-- Name: hpa_sample_hpa_sample_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE hpa_sample_hpa_sample_id_seq OWNED BY hpa_sample.hpa_sample_id;


--
-- Name: individual_event_code_individual_event_code_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE individual_event_code_individual_event_code_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE individual_event_code_individual_event_code_id_seq OWNER TO postgres;

--
-- Name: individual_event_code_individual_event_code_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE individual_event_code_individual_event_code_id_seq OWNED BY individual_event_code.individual_event_code_id;


--
-- Name: individual_event_individual_event_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE individual_event_individual_event_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE individual_event_individual_event_id_seq OWNER TO postgres;

--
-- Name: individual_event_individual_event_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE individual_event_individual_event_id_seq OWNED BY individual_event.individual_event_id;


--
-- Name: individual_individual_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE individual_individual_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE individual_individual_id_seq OWNER TO postgres;

--
-- Name: individual_individual_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE individual_individual_id_seq OWNED BY individual.individual_id;


--
-- Name: individual_name_history; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE individual_name_history (
    individual_name_history_id integer NOT NULL,
    individual_id integer NOT NULL,
    name text NOT NULL,
    date_changed timestamp without time zone
);


ALTER TABLE individual_name_history OWNER TO postgres;

--
-- Name: individual_name_history_individual_name_history_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE individual_name_history_individual_name_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE individual_name_history_individual_name_history_id_seq OWNER TO postgres;

--
-- Name: individual_name_history_individual_name_history_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE individual_name_history_individual_name_history_id_seq OWNED BY individual_name_history.individual_name_history_id;


--
-- Name: inter_group_interaction_inter_group_interaction_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE inter_group_interaction_inter_group_interaction_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE inter_group_interaction_inter_group_interaction_id_seq OWNER TO postgres;

--
-- Name: inter_group_interaction_inter_group_interaction_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE inter_group_interaction_inter_group_interaction_id_seq OWNED BY inter_group_interaction.inter_group_interaction_id;


--
-- Name: interaction_outcome_interaction_outcome_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE interaction_outcome_interaction_outcome_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE interaction_outcome_interaction_outcome_id_seq OWNER TO postgres;

--
-- Name: interaction_outcome_interaction_outcome_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE interaction_outcome_interaction_outcome_id_seq OWNED BY interaction_outcome.interaction_outcome_id;


--
-- Name: litter_event_code_litter_event_code_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE litter_event_code_litter_event_code_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE litter_event_code_litter_event_code_id_seq OWNER TO postgres;

--
-- Name: litter_event_code_litter_event_code_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE litter_event_code_litter_event_code_id_seq OWNED BY litter_event_code.litter_event_code_id;


--
-- Name: litter_event_litter_event_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE litter_event_litter_event_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE litter_event_litter_event_id_seq OWNER TO postgres;

--
-- Name: litter_event_litter_event_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE litter_event_litter_event_id_seq OWNED BY litter_event.litter_event_id;


--
-- Name: litter_litter_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE litter_litter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE litter_litter_id_seq OWNER TO postgres;

--
-- Name: litter_litter_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE litter_litter_id_seq OWNED BY litter.litter_id;


--
-- Name: mate_guard; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE mate_guard (
    mate_guard_id integer NOT NULL,
    pack_composition_id integer,
    female_individual_id integer,
    guard_individual_id integer,
    strength text,
    pester text,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE mate_guard OWNER TO postgres;

--
-- Name: mate_guard_mate_guard_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE mate_guard_mate_guard_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE mate_guard_mate_guard_id_seq OWNER TO postgres;

--
-- Name: mate_guard_mate_guard_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE mate_guard_mate_guard_id_seq OWNED BY mate_guard.mate_guard_id;


--
-- Name: maternal_conditioning_litter_maternal_conditioning_litter_i_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE maternal_conditioning_litter_maternal_conditioning_litter_i_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE maternal_conditioning_litter_maternal_conditioning_litter_i_seq OWNER TO postgres;

--
-- Name: maternal_conditioning_litter_maternal_conditioning_litter_i_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE maternal_conditioning_litter_maternal_conditioning_litter_i_seq OWNED BY maternal_conditioning_litter.maternal_conditioning_litter_id;


--
-- Name: maternel_conditioning_females_maternel_conditioning_females_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE maternel_conditioning_females_maternel_conditioning_females_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE maternel_conditioning_females_maternel_conditioning_females_seq OWNER TO postgres;

--
-- Name: maternel_conditioning_females_maternel_conditioning_females_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE maternel_conditioning_females_maternel_conditioning_females_seq OWNED BY maternel_conditioning_females.maternel_conditioning_females_id;


--
-- Name: meterology_meterology_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE meterology_meterology_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE meterology_meterology_id_seq OWNER TO postgres;

--
-- Name: meterology_meterology_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE meterology_meterology_id_seq OWNED BY meterology.meterology_id;


--
-- Name: oestrus_affiliation; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_affiliation (
    oestrus_affiliation_id integer NOT NULL,
    initiate text,
    with_individual_id integer,
    over text,
    "time" timestamp without time zone,
    location public.geography,
    oestrus_focal_id integer NOT NULL
);


ALTER TABLE oestrus_affiliation OWNER TO postgres;

--
-- Name: oestrus_affiliation_oestrus_affiliation_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_affiliation_oestrus_affiliation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_affiliation_oestrus_affiliation_id_seq OWNER TO postgres;

--
-- Name: oestrus_affiliation_oestrus_affiliation_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_affiliation_oestrus_affiliation_id_seq OWNED BY oestrus_affiliation.oestrus_affiliation_id;


--
-- Name: oestrus_aggression; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_aggression (
    oestrus_aggression_id integer NOT NULL,
    initate text,
    with_individual_id integer,
    level text,
    over text,
    win text,
    "time" timestamp without time zone,
    location public.geography,
    oestrus_focal_id integer
);


ALTER TABLE oestrus_aggression OWNER TO postgres;

--
-- Name: oestrus_aggression_oestrus_aggression_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_aggression_oestrus_aggression_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_aggression_oestrus_aggression_id_seq OWNER TO postgres;

--
-- Name: oestrus_aggression_oestrus_aggression_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_aggression_oestrus_aggression_id_seq OWNED BY oestrus_aggression.oestrus_aggression_id;


--
-- Name: oestrus_copulation_male_oestrus_copulation_male_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_copulation_male_oestrus_copulation_male_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_copulation_male_oestrus_copulation_male_id_seq OWNER TO postgres;

--
-- Name: oestrus_copulation_male_oestrus_copulation_male_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_copulation_male_oestrus_copulation_male_id_seq OWNED BY oestrus_copulation_male.oestrus_copulation_male_id;


--
-- Name: oestrus_event_code_oestrus_event_code_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_event_code_oestrus_event_code_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_event_code_oestrus_event_code_id_seq OWNER TO postgres;

--
-- Name: oestrus_event_code_oestrus_event_code_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_event_code_oestrus_event_code_id_seq OWNED BY oestrus_event_code.oestrus_event_code_id;


--
-- Name: oestrus_event_oestrus_event_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_event_oestrus_event_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_event_oestrus_event_id_seq OWNER TO postgres;

--
-- Name: oestrus_event_oestrus_event_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_event_oestrus_event_id_seq OWNED BY oestrus_event.oestrus_event_id;


--
-- Name: oestrus_focal; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_focal (
    oestrus_focal_id integer NOT NULL,
    pack_history_id integer NOT NULL,
    depth_of_pack integer,
    number_of_individuals integer,
    width integer,
    "time" timestamp without time zone NOT NULL,
    location public.geography
);


ALTER TABLE oestrus_focal OWNER TO postgres;

--
-- Name: oestrus_focal_oestrus_focal_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_focal_oestrus_focal_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_focal_oestrus_focal_id_seq OWNER TO postgres;

--
-- Name: oestrus_focal_oestrus_focal_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_focal_oestrus_focal_id_seq OWNED BY oestrus_focal.oestrus_focal_id;


--
-- Name: oestrus_male_aggression; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_male_aggression (
    oestrus_male_aggression_id integer NOT NULL,
    receiver_individual_id integer,
    initiator_individual_id integer,
    level text,
    winner text,
    owner text,
    "time" timestamp without time zone,
    location public.geography,
    oestrus_focal_id integer NOT NULL
);


ALTER TABLE oestrus_male_aggression OWNER TO postgres;

--
-- Name: oestrus_male_aggression_oestrus_male_aggression_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_male_aggression_oestrus_male_aggression_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_male_aggression_oestrus_male_aggression_id_seq OWNER TO postgres;

--
-- Name: oestrus_male_aggression_oestrus_male_aggression_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_male_aggression_oestrus_male_aggression_id_seq OWNED BY oestrus_male_aggression.oestrus_male_aggression_id;


--
-- Name: oestrus_mating; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_mating (
    oestrus_mating_id integer NOT NULL,
    behaviour text,
    with_individual_id integer NOT NULL,
    female_response text,
    male_response text,
    success text,
    "time" timestamp without time zone,
    location public.geography,
    oestrus_focal_id integer NOT NULL
);


ALTER TABLE oestrus_mating OWNER TO postgres;

--
-- Name: oestrus_mating_oestrus_mating_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_mating_oestrus_mating_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_mating_oestrus_mating_id_seq OWNER TO postgres;

--
-- Name: oestrus_mating_oestrus_mating_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_mating_oestrus_mating_id_seq OWNED BY oestrus_mating.oestrus_mating_id;


--
-- Name: oestrus_nearest; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE oestrus_nearest (
    oestrus_nearest_id integer NOT NULL,
    close_individuals text,
    nearest_individual_id integer,
    "time" timestamp without time zone,
    location public.geography,
    oestrus_focal_id integer NOT NULL
);


ALTER TABLE oestrus_nearest OWNER TO postgres;

--
-- Name: oestrus_nearest_oestrus_nearest_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_nearest_oestrus_nearest_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_nearest_oestrus_nearest_id_seq OWNER TO postgres;

--
-- Name: oestrus_nearest_oestrus_nearest_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_nearest_oestrus_nearest_id_seq OWNED BY oestrus_nearest.oestrus_nearest_id;


--
-- Name: oestrus_oestrus_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE oestrus_oestrus_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE oestrus_oestrus_id_seq OWNER TO postgres;

--
-- Name: oestrus_oestrus_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE oestrus_oestrus_id_seq OWNED BY oestrus.oestrus_id;


--
-- Name: ox_shielding_feeding_ox_shielding_feeding_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE ox_shielding_feeding_ox_shielding_feeding_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE ox_shielding_feeding_ox_shielding_feeding_id_seq OWNER TO postgres;

--
-- Name: ox_shielding_feeding_ox_shielding_feeding_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE ox_shielding_feeding_ox_shielding_feeding_id_seq OWNED BY ox_shielding_feeding.ox_shielding_feeding_id;


--
-- Name: ox_shielding_group_ox_shielding_group_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE ox_shielding_group_ox_shielding_group_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE ox_shielding_group_ox_shielding_group_id_seq OWNER TO postgres;

--
-- Name: ox_shielding_group_ox_shielding_group_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE ox_shielding_group_ox_shielding_group_id_seq OWNED BY ox_shielding_group.ox_shielding_group_id;


--
-- Name: ox_shielding_male_ox_shielding_male_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE ox_shielding_male_ox_shielding_male_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE ox_shielding_male_ox_shielding_male_id_seq OWNER TO postgres;

--
-- Name: ox_shielding_male_ox_shielding_male_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE ox_shielding_male_ox_shielding_male_id_seq OWNED BY ox_shielding_male.ox_shielding_male_id;


--
-- Name: pack_composition; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack_composition (
    pack_composition_id integer NOT NULL,
    pack_id integer NOT NULL,
    pups text,
    pups_count integer,
    pregnant_individuals text,
    pregnant_count integer,
    "time" timestamp without time zone,
    location public.geography,
    observer text
);


ALTER TABLE pack_composition OWNER TO postgres;

--
-- Name: pack_composition_pack_composition_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_composition_pack_composition_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_composition_pack_composition_id_seq OWNER TO postgres;

--
-- Name: pack_composition_pack_composition_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_composition_pack_composition_id_seq OWNED BY pack_composition.pack_composition_id;


--
-- Name: pack_event_code_pack_event_code_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_event_code_pack_event_code_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_event_code_pack_event_code_id_seq OWNER TO postgres;

--
-- Name: pack_event_code_pack_event_code_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_event_code_pack_event_code_id_seq OWNED BY pack_event_code.pack_event_code_id;


--
-- Name: pack_event_pack_event_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_event_pack_event_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_event_pack_event_id_seq OWNER TO postgres;

--
-- Name: pack_event_pack_event_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_event_pack_event_id_seq OWNED BY pack_event.pack_event_id;


--
-- Name: pack_history_pack_history_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_history_pack_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_history_pack_history_id_seq OWNER TO postgres;

--
-- Name: pack_history_pack_history_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_history_pack_history_id_seq OWNED BY pack_history.pack_history_id;


--
-- Name: pack_move_destination_pack_move_destination_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_move_destination_pack_move_destination_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_move_destination_pack_move_destination_id_seq OWNER TO postgres;

--
-- Name: pack_move_destination_pack_move_destination_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_move_destination_pack_move_destination_id_seq OWNED BY pack_move_destination.pack_move_destination_id;


--
-- Name: pack_move_pack_move_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_move_pack_move_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_move_pack_move_id_seq OWNER TO postgres;

--
-- Name: pack_move_pack_move_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_move_pack_move_id_seq OWNED BY pack_move.pack_move_id;


--
-- Name: pack_name_history; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pack_name_history (
    pack_name_history_id integer NOT NULL,
    pack_id integer NOT NULL,
    name text NOT NULL,
    date_changed timestamp without time zone NOT NULL
);


ALTER TABLE pack_name_history OWNER TO postgres;

--
-- Name: pack_name_history_pack_name_history_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_name_history_pack_name_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_name_history_pack_name_history_id_seq OWNER TO postgres;

--
-- Name: pack_name_history_pack_name_history_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_name_history_pack_name_history_id_seq OWNED BY pack_name_history.pack_name_history_id;


--
-- Name: pack_pack_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pack_pack_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pack_pack_id_seq OWNER TO postgres;

--
-- Name: pack_pack_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pack_pack_id_seq OWNED BY pack.pack_id;


--
-- Name: poo_sample_poo_sample_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE poo_sample_poo_sample_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE poo_sample_poo_sample_id_seq OWNER TO postgres;

--
-- Name: poo_sample_poo_sample_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE poo_sample_poo_sample_id_seq OWNED BY poo_sample.poo_sample_id;


--
-- Name: pregnancy; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pregnancy (
    pregnancy_id integer NOT NULL,
    pack_composition_id integer,
    pregnant_individual_id integer
);


ALTER TABLE pregnancy OWNER TO postgres;

--
-- Name: pregnancy_affiliation; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pregnancy_affiliation (
    pregnancy_affiliation_id integer NOT NULL,
    pregnancy_focal_id integer NOT NULL,
    with_individual_id integer,
    initiate text,
    over text,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pregnancy_affiliation OWNER TO postgres;

--
-- Name: pregnancy_affiliation_pregnancy_affiliation_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pregnancy_affiliation_pregnancy_affiliation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pregnancy_affiliation_pregnancy_affiliation_id_seq OWNER TO postgres;

--
-- Name: pregnancy_affiliation_pregnancy_affiliation_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pregnancy_affiliation_pregnancy_affiliation_id_seq OWNED BY pregnancy_affiliation.pregnancy_affiliation_id;


--
-- Name: pregnancy_aggression; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pregnancy_aggression (
    pregnancy_aggression_id integer NOT NULL,
    pregnancy_focal_id integer,
    with_individual_id integer,
    initiate text,
    level text,
    over text,
    win text,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pregnancy_aggression OWNER TO postgres;

--
-- Name: pregnancy_aggression_pregnancy_aggression_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pregnancy_aggression_pregnancy_aggression_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pregnancy_aggression_pregnancy_aggression_id_seq OWNER TO postgres;

--
-- Name: pregnancy_aggression_pregnancy_aggression_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pregnancy_aggression_pregnancy_aggression_id_seq OWNED BY pregnancy_aggression.pregnancy_aggression_id;


--
-- Name: pregnancy_focal; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pregnancy_focal (
    pregnancy_focal_id integer NOT NULL,
    pack_history_id integer,
    depth integer,
    width integer,
    individuals integer,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pregnancy_focal OWNER TO postgres;

--
-- Name: pregnancy_focal_pregnancy_focal_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pregnancy_focal_pregnancy_focal_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pregnancy_focal_pregnancy_focal_id_seq OWNER TO postgres;

--
-- Name: pregnancy_focal_pregnancy_focal_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pregnancy_focal_pregnancy_focal_id_seq OWNED BY pregnancy_focal.pregnancy_focal_id;


--
-- Name: pregnancy_nearest; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pregnancy_nearest (
    pregnancy_nearest_id integer NOT NULL,
    pregnancy_focal_id integer NOT NULL,
    nearest_individual_id integer,
    list_of_closest_individuals text,
    scan_time timestamp without time zone
);


ALTER TABLE pregnancy_nearest OWNER TO postgres;

--
-- Name: pregnancy_nearest_pregnancy_nearest_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pregnancy_nearest_pregnancy_nearest_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pregnancy_nearest_pregnancy_nearest_id_seq OWNER TO postgres;

--
-- Name: pregnancy_nearest_pregnancy_nearest_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pregnancy_nearest_pregnancy_nearest_id_seq OWNED BY pregnancy_nearest.pregnancy_nearest_id;


--
-- Name: pregnancy_pregnancy_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pregnancy_pregnancy_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pregnancy_pregnancy_id_seq OWNER TO postgres;

--
-- Name: pregnancy_pregnancy_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pregnancy_pregnancy_id_seq OWNED BY pregnancy.pregnancy_id;


--
-- Name: provisioning_data_provisioning_data_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE provisioning_data_provisioning_data_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE provisioning_data_provisioning_data_id_seq OWNER TO postgres;

--
-- Name: provisioning_data_provisioning_data_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE provisioning_data_provisioning_data_id_seq OWNED BY provisioning_data.provisioning_data_id;


--
-- Name: pup_association; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pup_association (
    pup_association_id integer NOT NULL,
    pup_pack_history_id integer,
    escort_id integer,
    pack_composition_id integer,
    date date,
    strength text,
    confidence text,
    location public.geography,
    comment text,
    comment_editing text
);


ALTER TABLE pup_association OWNER TO postgres;

--
-- Name: pup association; Type: VIEW; Schema: mongoose; Owner: postgres
--

CREATE VIEW "pup association" AS
 SELECT pa.date,
    pup_pack.name AS "Group",
    pup_litter.name AS litter,
    pup.name AS "Pup",
    pup.sex AS "Pup Sex",
    escort.name AS "Escort",
    escort.sex AS "Escort Sex",
    pa.strength,
    pa.confidence,
    pa.comment,
    pa.comment_editing AS "Editing comments",
    public.st_x((pa.location)::public.geometry) AS latitude,
    public.st_y((pa.location)::public.geometry) AS longitude
   FROM (((((pup_association pa
     JOIN pack_history ph ON ((ph.pack_history_id = pa.pup_pack_history_id)))
     JOIN individual pup ON ((pup.individual_id = ph.individual_id)))
     JOIN pack pup_pack ON ((pup_pack.pack_id = ph.pack_id)))
     LEFT JOIN individual escort ON ((escort.individual_id = pa.escort_id)))
     LEFT JOIN litter pup_litter ON ((pup_litter.litter_id = pup.litter_id)));


ALTER TABLE "pup association" OWNER TO postgres;

--
-- Name: pup_aggression; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pup_aggression (
    pup_aggression_id integer NOT NULL,
    pup_focal_id integer NOT NULL,
    with_individual_id integer,
    initiate text,
    level text,
    over text,
    win text,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pup_aggression OWNER TO postgres;

--
-- Name: pup_aggression_pup_aggression_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pup_aggression_pup_aggression_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pup_aggression_pup_aggression_id_seq OWNER TO postgres;

--
-- Name: pup_aggression_pup_aggression_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pup_aggression_pup_aggression_id_seq OWNED BY pup_aggression.pup_aggression_id;


--
-- Name: pup_association_pup_association_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pup_association_pup_association_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pup_association_pup_association_id_seq OWNER TO postgres;

--
-- Name: pup_association_pup_association_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pup_association_pup_association_id_seq OWNED BY pup_association.pup_association_id;


--
-- Name: pup_care; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pup_care (
    pup_care_id integer NOT NULL,
    pup_focal_id integer NOT NULL,
    who_individual_id integer,
    type text,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pup_care OWNER TO postgres;

--
-- Name: pup_care_pup_care_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pup_care_pup_care_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pup_care_pup_care_id_seq OWNER TO postgres;

--
-- Name: pup_care_pup_care_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pup_care_pup_care_id_seq OWNED BY pup_care.pup_care_id;


--
-- Name: pup_feed; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pup_feed (
    pup_feed_id integer NOT NULL,
    pup_focal_id integer NOT NULL,
    who_individual_id integer,
    size text,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pup_feed OWNER TO postgres;

--
-- Name: pup_feed_pup_feed_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pup_feed_pup_feed_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pup_feed_pup_feed_id_seq OWNER TO postgres;

--
-- Name: pup_feed_pup_feed_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pup_feed_pup_feed_id_seq OWNED BY pup_feed.pup_feed_id;


--
-- Name: pup_find; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pup_find (
    pup_find_id integer NOT NULL,
    pup_focal_id integer,
    size text,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pup_find OWNER TO postgres;

--
-- Name: pup_find_pup_find_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pup_find_pup_find_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pup_find_pup_find_id_seq OWNER TO postgres;

--
-- Name: pup_find_pup_find_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pup_find_pup_find_id_seq OWNED BY pup_find.pup_find_id;


--
-- Name: pup_focal; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pup_focal (
    pup_focal_id integer NOT NULL,
    pack_history_id integer NOT NULL,
    depth integer,
    width integer,
    individuals integer,
    "time" timestamp without time zone,
    location public.geography
);


ALTER TABLE pup_focal OWNER TO postgres;

--
-- Name: pup_focal_pup_focal_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pup_focal_pup_focal_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pup_focal_pup_focal_id_seq OWNER TO postgres;

--
-- Name: pup_focal_pup_focal_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pup_focal_pup_focal_id_seq OWNED BY pup_focal.pup_focal_id;


--
-- Name: pup_nearest; Type: TABLE; Schema: mongoose; Owner: postgres
--

CREATE TABLE pup_nearest (
    pup_nearest_id integer NOT NULL,
    pup_focal_id integer NOT NULL,
    nearest_individual_id integer,
    list_of_closest_individuals text,
    scan_time timestamp without time zone
);


ALTER TABLE pup_nearest OWNER TO postgres;

--
-- Name: pup_nearest_pup_nearest_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE pup_nearest_pup_nearest_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE pup_nearest_pup_nearest_id_seq OWNER TO postgres;

--
-- Name: pup_nearest_pup_nearest_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE pup_nearest_pup_nearest_id_seq OWNED BY pup_nearest.pup_nearest_id;


--
-- Name: radiocollar_radiocollar_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE radiocollar_radiocollar_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE radiocollar_radiocollar_id_seq OWNER TO postgres;

--
-- Name: radiocollar_radiocollar_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE radiocollar_radiocollar_id_seq OWNED BY radiocollar.radiocollar_id;


--
-- Name: ultrasound_ultrasound_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE ultrasound_ultrasound_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE ultrasound_ultrasound_id_seq OWNER TO postgres;

--
-- Name: ultrasound_ultrasound_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE ultrasound_ultrasound_id_seq OWNED BY ultrasound.ultrasound_id;


--
-- Name: weight_weight_id_seq; Type: SEQUENCE; Schema: mongoose; Owner: postgres
--

CREATE SEQUENCE weight_weight_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE weight_weight_id_seq OWNER TO postgres;

--
-- Name: weight_weight_id_seq; Type: SEQUENCE OWNED BY; Schema: mongoose; Owner: postgres
--

ALTER SEQUENCE weight_weight_id_seq OWNED BY weight.weight_id;


--
-- Name: alarm alarm_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY alarm ALTER COLUMN alarm_id SET DEFAULT nextval('alarm_alarm_id_seq'::regclass);


--
-- Name: alarm_cause alarm_cause_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY alarm_cause ALTER COLUMN alarm_cause_id SET DEFAULT nextval('alarm_cause_alarm_cause_id_seq'::regclass);


--
-- Name: anti_parasite anti_parasite_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY anti_parasite ALTER COLUMN anti_parasite_id SET DEFAULT nextval('anti_parasite_anti_parasite_id_seq'::regclass);


--
-- Name: babysitting babysitting_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY babysitting ALTER COLUMN babysitting_id SET DEFAULT nextval('babysitting_babysitting_id_seq'::regclass);


--
-- Name: blood_data blood_data_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY blood_data ALTER COLUMN blood_data_id SET DEFAULT nextval('blood_data_blood_data_id_seq'::regclass);


--
-- Name: capture capture_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY capture ALTER COLUMN capture_id SET DEFAULT nextval('capture_capture_id_seq'::regclass);


--
-- Name: dna_samples dna_samples_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY dna_samples ALTER COLUMN dna_samples_id SET DEFAULT nextval('dna_samples_dna_samples_id_seq'::regclass);


--
-- Name: event_log event_log_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY event_log ALTER COLUMN event_log_id SET DEFAULT nextval('event_log_event_log_id_seq'::regclass);


--
-- Name: group_composition group_composition_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY group_composition ALTER COLUMN group_composition_id SET DEFAULT nextval('group_composition_group_composition_id_seq'::regclass);


--
-- Name: hpa_sample hpa_sample_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY hpa_sample ALTER COLUMN hpa_sample_id SET DEFAULT nextval('hpa_sample_hpa_sample_id_seq'::regclass);


--
-- Name: individual individual_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual ALTER COLUMN individual_id SET DEFAULT nextval('individual_individual_id_seq'::regclass);


--
-- Name: individual_event individual_event_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_event ALTER COLUMN individual_event_id SET DEFAULT nextval('individual_event_individual_event_id_seq'::regclass);


--
-- Name: individual_event_code individual_event_code_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_event_code ALTER COLUMN individual_event_code_id SET DEFAULT nextval('individual_event_code_individual_event_code_id_seq'::regclass);


--
-- Name: individual_name_history individual_name_history_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_name_history ALTER COLUMN individual_name_history_id SET DEFAULT nextval('individual_name_history_individual_name_history_id_seq'::regclass);


--
-- Name: inter_group_interaction inter_group_interaction_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY inter_group_interaction ALTER COLUMN inter_group_interaction_id SET DEFAULT nextval('inter_group_interaction_inter_group_interaction_id_seq'::regclass);


--
-- Name: interaction_outcome interaction_outcome_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY interaction_outcome ALTER COLUMN interaction_outcome_id SET DEFAULT nextval('interaction_outcome_interaction_outcome_id_seq'::regclass);


--
-- Name: litter litter_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter ALTER COLUMN litter_id SET DEFAULT nextval('litter_litter_id_seq'::regclass);


--
-- Name: litter_event litter_event_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter_event ALTER COLUMN litter_event_id SET DEFAULT nextval('litter_event_litter_event_id_seq'::regclass);


--
-- Name: litter_event_code litter_event_code_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter_event_code ALTER COLUMN litter_event_code_id SET DEFAULT nextval('litter_event_code_litter_event_code_id_seq'::regclass);


--
-- Name: mate_guard mate_guard_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY mate_guard ALTER COLUMN mate_guard_id SET DEFAULT nextval('mate_guard_mate_guard_id_seq'::regclass);


--
-- Name: maternal_conditioning_litter maternal_conditioning_litter_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY maternal_conditioning_litter ALTER COLUMN maternal_conditioning_litter_id SET DEFAULT nextval('maternal_conditioning_litter_maternal_conditioning_litter_i_seq'::regclass);


--
-- Name: maternel_conditioning_females maternel_conditioning_females_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY maternel_conditioning_females ALTER COLUMN maternel_conditioning_females_id SET DEFAULT nextval('maternel_conditioning_females_maternel_conditioning_females_seq'::regclass);


--
-- Name: meterology meterology_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY meterology ALTER COLUMN meterology_id SET DEFAULT nextval('meterology_meterology_id_seq'::regclass);


--
-- Name: oestrus oestrus_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus ALTER COLUMN oestrus_id SET DEFAULT nextval('oestrus_oestrus_id_seq'::regclass);


--
-- Name: oestrus_affiliation oestrus_affiliation_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_affiliation ALTER COLUMN oestrus_affiliation_id SET DEFAULT nextval('oestrus_affiliation_oestrus_affiliation_id_seq'::regclass);


--
-- Name: oestrus_aggression oestrus_aggression_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_aggression ALTER COLUMN oestrus_aggression_id SET DEFAULT nextval('oestrus_aggression_oestrus_aggression_id_seq'::regclass);


--
-- Name: oestrus_copulation_male oestrus_copulation_male_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_copulation_male ALTER COLUMN oestrus_copulation_male_id SET DEFAULT nextval('oestrus_copulation_male_oestrus_copulation_male_id_seq'::regclass);


--
-- Name: oestrus_event oestrus_event_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_event ALTER COLUMN oestrus_event_id SET DEFAULT nextval('oestrus_event_oestrus_event_id_seq'::regclass);


--
-- Name: oestrus_event_code oestrus_event_code_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_event_code ALTER COLUMN oestrus_event_code_id SET DEFAULT nextval('oestrus_event_code_oestrus_event_code_id_seq'::regclass);


--
-- Name: oestrus_focal oestrus_focal_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_focal ALTER COLUMN oestrus_focal_id SET DEFAULT nextval('oestrus_focal_oestrus_focal_id_seq'::regclass);


--
-- Name: oestrus_male_aggression oestrus_male_aggression_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_male_aggression ALTER COLUMN oestrus_male_aggression_id SET DEFAULT nextval('oestrus_male_aggression_oestrus_male_aggression_id_seq'::regclass);


--
-- Name: oestrus_mating oestrus_mating_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_mating ALTER COLUMN oestrus_mating_id SET DEFAULT nextval('oestrus_mating_oestrus_mating_id_seq'::regclass);


--
-- Name: oestrus_nearest oestrus_nearest_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_nearest ALTER COLUMN oestrus_nearest_id SET DEFAULT nextval('oestrus_nearest_oestrus_nearest_id_seq'::regclass);


--
-- Name: ox_shielding_feeding ox_shielding_feeding_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_feeding ALTER COLUMN ox_shielding_feeding_id SET DEFAULT nextval('ox_shielding_feeding_ox_shielding_feeding_id_seq'::regclass);


--
-- Name: ox_shielding_group ox_shielding_group_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_group ALTER COLUMN ox_shielding_group_id SET DEFAULT nextval('ox_shielding_group_ox_shielding_group_id_seq'::regclass);


--
-- Name: ox_shielding_male ox_shielding_male_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_male ALTER COLUMN ox_shielding_male_id SET DEFAULT nextval('ox_shielding_male_ox_shielding_male_id_seq'::regclass);


--
-- Name: pack pack_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack ALTER COLUMN pack_id SET DEFAULT nextval('pack_pack_id_seq'::regclass);


--
-- Name: pack_composition pack_composition_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_composition ALTER COLUMN pack_composition_id SET DEFAULT nextval('pack_composition_pack_composition_id_seq'::regclass);


--
-- Name: pack_event pack_event_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_event ALTER COLUMN pack_event_id SET DEFAULT nextval('pack_event_pack_event_id_seq'::regclass);


--
-- Name: pack_event_code pack_event_code_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_event_code ALTER COLUMN pack_event_code_id SET DEFAULT nextval('pack_event_code_pack_event_code_id_seq'::regclass);


--
-- Name: pack_history pack_history_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_history ALTER COLUMN pack_history_id SET DEFAULT nextval('pack_history_pack_history_id_seq'::regclass);


--
-- Name: pack_move pack_move_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_move ALTER COLUMN pack_move_id SET DEFAULT nextval('pack_move_pack_move_id_seq'::regclass);


--
-- Name: pack_move_destination pack_move_destination_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_move_destination ALTER COLUMN pack_move_destination_id SET DEFAULT nextval('pack_move_destination_pack_move_destination_id_seq'::regclass);


--
-- Name: pack_name_history pack_name_history_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_name_history ALTER COLUMN pack_name_history_id SET DEFAULT nextval('pack_name_history_pack_name_history_id_seq'::regclass);


--
-- Name: poo_sample poo_sample_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY poo_sample ALTER COLUMN poo_sample_id SET DEFAULT nextval('poo_sample_poo_sample_id_seq'::regclass);


--
-- Name: pregnancy pregnancy_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy ALTER COLUMN pregnancy_id SET DEFAULT nextval('pregnancy_pregnancy_id_seq'::regclass);


--
-- Name: pregnancy_affiliation pregnancy_affiliation_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_affiliation ALTER COLUMN pregnancy_affiliation_id SET DEFAULT nextval('pregnancy_affiliation_pregnancy_affiliation_id_seq'::regclass);


--
-- Name: pregnancy_aggression pregnancy_aggression_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_aggression ALTER COLUMN pregnancy_aggression_id SET DEFAULT nextval('pregnancy_aggression_pregnancy_aggression_id_seq'::regclass);


--
-- Name: pregnancy_focal pregnancy_focal_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_focal ALTER COLUMN pregnancy_focal_id SET DEFAULT nextval('pregnancy_focal_pregnancy_focal_id_seq'::regclass);


--
-- Name: pregnancy_nearest pregnancy_nearest_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_nearest ALTER COLUMN pregnancy_nearest_id SET DEFAULT nextval('pregnancy_nearest_pregnancy_nearest_id_seq'::regclass);


--
-- Name: provisioning_data provisioning_data_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY provisioning_data ALTER COLUMN provisioning_data_id SET DEFAULT nextval('provisioning_data_provisioning_data_id_seq'::regclass);


--
-- Name: pup_aggression pup_aggression_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_aggression ALTER COLUMN pup_aggression_id SET DEFAULT nextval('pup_aggression_pup_aggression_id_seq'::regclass);


--
-- Name: pup_association pup_association_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_association ALTER COLUMN pup_association_id SET DEFAULT nextval('pup_association_pup_association_id_seq'::regclass);


--
-- Name: pup_care pup_care_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_care ALTER COLUMN pup_care_id SET DEFAULT nextval('pup_care_pup_care_id_seq'::regclass);


--
-- Name: pup_feed pup_feed_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_feed ALTER COLUMN pup_feed_id SET DEFAULT nextval('pup_feed_pup_feed_id_seq'::regclass);


--
-- Name: pup_find pup_find_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_find ALTER COLUMN pup_find_id SET DEFAULT nextval('pup_find_pup_find_id_seq'::regclass);


--
-- Name: pup_focal pup_focal_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_focal ALTER COLUMN pup_focal_id SET DEFAULT nextval('pup_focal_pup_focal_id_seq'::regclass);


--
-- Name: pup_nearest pup_nearest_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_nearest ALTER COLUMN pup_nearest_id SET DEFAULT nextval('pup_nearest_pup_nearest_id_seq'::regclass);


--
-- Name: radiocollar radiocollar_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY radiocollar ALTER COLUMN radiocollar_id SET DEFAULT nextval('radiocollar_radiocollar_id_seq'::regclass);


--
-- Name: ultrasound ultrasound_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ultrasound ALTER COLUMN ultrasound_id SET DEFAULT nextval('ultrasound_ultrasound_id_seq'::regclass);


--
-- Name: weight weight_id; Type: DEFAULT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY weight ALTER COLUMN weight_id SET DEFAULT nextval('weight_weight_id_seq'::regclass);


--
-- Data for Name: alarm; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY alarm (alarm_id, date, pack_id, caller_individual_id, alarm_cause_id, others_join, location) FROM stdin;
\.


--
-- Name: alarm_alarm_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('alarm_alarm_id_seq', 1, false);


--
-- Data for Name: alarm_cause; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY alarm_cause (alarm_cause_id, cause) FROM stdin;
1	predator
2	other-pack
3	humans
4	other
5	unknown
\.


--
-- Name: alarm_cause_alarm_cause_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('alarm_cause_alarm_cause_id_seq', 5, true);


--
-- Data for Name: anti_parasite; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY anti_parasite (anti_parasite_id, pack_history_id, started_date, "fecal_sample_A_date", first_capture_date, experiment_group, "fecal_sample_B_date", "fecal_sample_C_date", second_capture, "fecal_sample_D_date", "fecal_sample_E_date", "fecal_sample_F_date", comments) FROM stdin;
\.


--
-- Name: anti_parasite_anti_parasite_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('anti_parasite_anti_parasite_id_seq', 1, false);


--
-- Data for Name: babysitting; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY babysitting (babysitting_id, babysitter_pack_history_id, date, type, litter_id, time_start, den_distance, time_end, accuracy, comment, location) FROM stdin;
\.


--
-- Name: babysitting_babysitting_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('babysitting_babysitting_id_seq', 1, false);


--
-- Data for Name: blood_data; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY blood_data (blood_data_id, individual_id, date, trap_time, bleed_time, weight, release_time, sample, spinning_time, freeze_time, focal, plasma_volume_ul, comment) FROM stdin;
\.


--
-- Name: blood_data_blood_data_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('blood_data_blood_data_id_seq', 1, false);


--
-- Data for Name: capture; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY capture (capture_id, pack_history_id, date, trap_time, process_time, trap_location, bleed_time, release_time, examiner, age, drugs, reproductive_status, teats_ext, ultrasound, foetuses, foetus_size, weight, head_width, head_length, body_length, hind_foot_length, tail_length, tail_circumference, ticks, fleas, wounds_and_scars, plasma_sample_id, plasma_freeze_time, blood_sample_id, blood_sample_freeze_time, bucket, white_blood_count, white_blood_freeze_time, white_blood_cell_bucket, whisker_sample_id, ear_clip, tail_tip, "2d4d_photo", agd_photo, blood_sugar, red_cell_percentage, fat_neck_1, fat_neck_2, fat_armpit, fat_thigh, testes_length, testes_width, testes_depth, tooth_wear, comment) FROM stdin;
\.


--
-- Name: capture_capture_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('capture_capture_id_seq', 1, false);


--
-- Data for Name: dna_samples; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY dna_samples (dna_samples_id, pack_history_id, date, litter_id, sample_type, tissue, storage, tube_id, age, dispersal, box_slot, comment) FROM stdin;
\.


--
-- Name: dna_samples_dna_samples_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('dna_samples_dna_samples_id_seq', 1, false);


--
-- Data for Name: event_log; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY event_log (event_log_id, message_id, delivered_count, type, object, success, error, date_created) FROM stdin;
\.


--
-- Name: event_log_event_log_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('event_log_event_log_id_seq', 1, false);


--
-- Data for Name: group_composition; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY group_composition (group_composition_id, pack_id, date, observer, session, group_status, weather_start, weather_end, males_over_one_year, females_over_one_year, males_over_three_months, females_over_three_months, male_pups, female_pups, unknown_pups, pups_in_den, comment, location) FROM stdin;
\.


--
-- Name: group_composition_group_composition_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('group_composition_group_composition_id_seq', 1, false);


--
-- Data for Name: hpa_sample; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY hpa_sample (hpa_sample_id, individual_id, date, time_in_trap, capture_time, first_blood_sample_taken_time, first_sample_id, first_blood_sample_freezer_time, second_blood_sample_taken_time, second_blood_sample_id, second_blood_sample_freezer_time, head_width, weight, ticks) FROM stdin;
\.


--
-- Name: hpa_sample_hpa_sample_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('hpa_sample_hpa_sample_id_seq', 1, false);


--
-- Data for Name: individual; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY individual (individual_id, litter_id, name, sex, date_of_birth, transponder_id, unique_id, collar_weight, is_mongoose) FROM stdin;
1	\N	Unknown	\N	\N	\N	\N	\N	t
\.


--
-- Data for Name: individual_event; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY individual_event (individual_event_id, individual_event_code_id, pack_history_id, date, exact, start_end, status, cause, affected_litter, location, comment) FROM stdin;
\.


--
-- Data for Name: individual_event_code; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY individual_event_code (individual_event_code_id, code) FROM stdin;
1	BORN
2	DIED
3	ADIED
4	LSEEN
5	FSEEN
6	STEV
7	ENDEV
8	LEAVE
9	RETURN
10	IMM
11	EMM
12	FPREG
13	ABORT
14	BIRTH
\.


--
-- Name: individual_event_code_individual_event_code_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('individual_event_code_individual_event_code_id_seq', 14, true);


--
-- Name: individual_event_individual_event_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('individual_event_individual_event_id_seq', 1, false);


--
-- Name: individual_individual_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('individual_individual_id_seq', 1, true);


--
-- Data for Name: individual_name_history; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY individual_name_history (individual_name_history_id, individual_id, name, date_changed) FROM stdin;
\.


--
-- Name: individual_name_history_individual_name_history_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('individual_name_history_individual_name_history_id_seq', 1, false);


--
-- Data for Name: inter_group_interaction; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY inter_group_interaction (inter_group_interaction_id, focalpack_id, secondpack_id, leader_individual_id, interaction_outcome_id, "time", location, comment) FROM stdin;
\.


--
-- Name: inter_group_interaction_inter_group_interaction_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('inter_group_interaction_inter_group_interaction_id_seq', 1, false);


--
-- Data for Name: interaction_outcome; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY interaction_outcome (interaction_outcome_id, outcome) FROM stdin;
1	retreat
2	advance
3	fight-retreat
4	fight-win
\.


--
-- Name: interaction_outcome_interaction_outcome_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('interaction_outcome_interaction_outcome_id_seq', 4, true);


--
-- Data for Name: litter; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY litter (litter_id, pack_id, name, unknown_pups_count, date_formed) FROM stdin;
\.


--
-- Data for Name: litter_event; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY litter_event (litter_event_id, litter_id, litter_event_code_id, date, cause, exact, last_seen, location, comment) FROM stdin;
\.


--
-- Data for Name: litter_event_code; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY litter_event_code (litter_event_code_id, code) FROM stdin;
1	UNSUCCESSFUL
2	SHORT-LIVED
3	SUCCESSFUL
4	BORN
\.


--
-- Name: litter_event_code_litter_event_code_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('litter_event_code_litter_event_code_id_seq', 4, true);


--
-- Name: litter_event_litter_event_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('litter_event_litter_event_id_seq', 1, false);


--
-- Name: litter_litter_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('litter_litter_id_seq', 1, false);


--
-- Data for Name: mate_guard; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY mate_guard (mate_guard_id, pack_composition_id, female_individual_id, guard_individual_id, strength, pester, "time", location) FROM stdin;
\.


--
-- Name: mate_guard_mate_guard_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('mate_guard_mate_guard_id_seq', 1, false);


--
-- Data for Name: maternal_conditioning_litter; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY maternal_conditioning_litter (maternal_conditioning_litter_id, litter_id, experiment_number, pregnancy_check_date, date_started, experiment_type, foetus_age, "number_T_females", "number_C_females", notes) FROM stdin;
\.


--
-- Name: maternal_conditioning_litter_maternal_conditioning_litter_i_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('maternal_conditioning_litter_maternal_conditioning_litter_i_seq', 1, false);


--
-- Data for Name: maternel_conditioning_females; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY maternel_conditioning_females (maternel_conditioning_females_id, pack_history_id, experiment_type, paired_female_id, litter_id, catagory, notes) FROM stdin;
\.


--
-- Name: maternel_conditioning_females_maternel_conditioning_females_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('maternel_conditioning_females_maternel_conditioning_females_seq', 1, false);


--
-- Data for Name: meterology; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY meterology (meterology_id, date, rain, temp_max, temp_min, humidity_max, humidity_min, temp, observer) FROM stdin;
\.


--
-- Name: meterology_meterology_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('meterology_meterology_id_seq', 1, false);


--
-- Data for Name: oestrus; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus (oestrus_id, pack_history_id, date, "time", oestrus_code, guard_id, pesterer_id_1, pesterer_id_2, pesterer_id_3, pesterer_id_4, strength, confidence, location, comment) FROM stdin;
\.


--
-- Data for Name: oestrus_affiliation; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_affiliation (oestrus_affiliation_id, initiate, with_individual_id, over, "time", location, oestrus_focal_id) FROM stdin;
\.


--
-- Name: oestrus_affiliation_oestrus_affiliation_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_affiliation_oestrus_affiliation_id_seq', 1, false);


--
-- Data for Name: oestrus_aggression; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_aggression (oestrus_aggression_id, initate, with_individual_id, level, over, win, "time", location, oestrus_focal_id) FROM stdin;
\.


--
-- Name: oestrus_aggression_oestrus_aggression_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_aggression_oestrus_aggression_id_seq', 1, false);


--
-- Data for Name: oestrus_copulation_male; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_copulation_male (oestrus_copulation_male_id, oestrus_id, individual_id) FROM stdin;
\.


--
-- Name: oestrus_copulation_male_oestrus_copulation_male_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_copulation_male_oestrus_copulation_male_id_seq', 1, false);


--
-- Data for Name: oestrus_event; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_event (oestrus_event_id, pack_id, oestrus_event_code_id, date, oestrus_code, exact, last_seen, location, comment) FROM stdin;
\.


--
-- Data for Name: oestrus_event_code; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_event_code (oestrus_event_code_id, code) FROM stdin;
\.


--
-- Name: oestrus_event_code_oestrus_event_code_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_event_code_oestrus_event_code_id_seq', 1, false);


--
-- Name: oestrus_event_oestrus_event_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_event_oestrus_event_id_seq', 1, false);


--
-- Data for Name: oestrus_focal; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_focal (oestrus_focal_id, pack_history_id, depth_of_pack, number_of_individuals, width, "time", location) FROM stdin;
\.


--
-- Name: oestrus_focal_oestrus_focal_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_focal_oestrus_focal_id_seq', 1, false);


--
-- Data for Name: oestrus_male_aggression; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_male_aggression (oestrus_male_aggression_id, receiver_individual_id, initiator_individual_id, level, winner, owner, "time", location, oestrus_focal_id) FROM stdin;
\.


--
-- Name: oestrus_male_aggression_oestrus_male_aggression_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_male_aggression_oestrus_male_aggression_id_seq', 1, false);


--
-- Data for Name: oestrus_mating; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_mating (oestrus_mating_id, behaviour, with_individual_id, female_response, male_response, success, "time", location, oestrus_focal_id) FROM stdin;
\.


--
-- Name: oestrus_mating_oestrus_mating_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_mating_oestrus_mating_id_seq', 1, false);


--
-- Data for Name: oestrus_nearest; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY oestrus_nearest (oestrus_nearest_id, close_individuals, nearest_individual_id, "time", location, oestrus_focal_id) FROM stdin;
\.


--
-- Name: oestrus_nearest_oestrus_nearest_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_nearest_oestrus_nearest_id_seq', 1, false);


--
-- Name: oestrus_oestrus_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('oestrus_oestrus_id_seq', 1, false);


--
-- Data for Name: ox_shielding_feeding; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY ox_shielding_feeding (ox_shielding_feeding_id, pack_history_id, date, time_of_day, amount_of_egg, comments) FROM stdin;
\.


--
-- Name: ox_shielding_feeding_ox_shielding_feeding_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('ox_shielding_feeding_ox_shielding_feeding_id_seq', 1, false);


--
-- Data for Name: ox_shielding_group; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY ox_shielding_group (ox_shielding_group_id, pack_history_id, treatment_group, start_date, comment) FROM stdin;
\.


--
-- Name: ox_shielding_group_ox_shielding_group_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('ox_shielding_group_ox_shielding_group_id_seq', 1, false);


--
-- Data for Name: ox_shielding_male; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY ox_shielding_male (ox_shielding_male_id, pack_history_id, status, start_date, stop_date, comment) FROM stdin;
\.


--
-- Name: ox_shielding_male_ox_shielding_male_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('ox_shielding_male_ox_shielding_male_id_seq', 1, false);


--
-- Data for Name: pack; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack (pack_id, name, pack_created_date, unique_id) FROM stdin;
1	Unknown	\N	\N
\.


--
-- Data for Name: pack_composition; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack_composition (pack_composition_id, pack_id, pups, pups_count, pregnant_individuals, pregnant_count, "time", location, observer) FROM stdin;
\.


--
-- Name: pack_composition_pack_composition_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_composition_pack_composition_id_seq', 1, false);


--
-- Data for Name: pack_event; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack_event (pack_event_id, pack_id, pack_event_code_id, date, exact, status, cause, location, comment) FROM stdin;
\.


--
-- Data for Name: pack_event_code; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack_event_code (pack_event_code_id, code, detail) FROM stdin;
1	ENDGRP	End of pack
2	LGRP	Lost Pack
3	FGRP	Found Pack
4	NGRP	New Pack
\.


--
-- Name: pack_event_code_pack_event_code_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_event_code_pack_event_code_id_seq', 4, true);


--
-- Name: pack_event_pack_event_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_event_pack_event_id_seq', 1, false);


--
-- Data for Name: pack_history; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack_history (pack_history_id, pack_id, individual_id, date_joined) FROM stdin;
\.


--
-- Name: pack_history_pack_history_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_history_pack_history_id_seq', 1, false);


--
-- Data for Name: pack_move; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack_move (pack_move_id, pack_id, leader_individual_id, pack_move_destination_id, direction, "time", width, depth, number_of_individuals, location) FROM stdin;
\.


--
-- Data for Name: pack_move_destination; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack_move_destination (pack_move_destination_id, destination) FROM stdin;
1	latrine
2	water
3	food
4	nothing
5	den
6	unknown
\.


--
-- Name: pack_move_destination_pack_move_destination_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_move_destination_pack_move_destination_id_seq', 6, true);


--
-- Name: pack_move_pack_move_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_move_pack_move_id_seq', 1, false);


--
-- Data for Name: pack_name_history; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pack_name_history (pack_name_history_id, pack_id, name, date_changed) FROM stdin;
\.


--
-- Name: pack_name_history_pack_name_history_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_name_history_pack_name_history_id_seq', 1, false);


--
-- Name: pack_pack_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pack_pack_id_seq', 1, true);


--
-- Data for Name: poo_sample; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY poo_sample (poo_sample_id, pack_history_id, sample_number, date, pack_status, emergence_time, collection_time, freezer_time, parasite_sample, comment) FROM stdin;
\.


--
-- Name: poo_sample_poo_sample_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('poo_sample_poo_sample_id_seq', 1, false);


--
-- Data for Name: pregnancy; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pregnancy (pregnancy_id, pack_composition_id, pregnant_individual_id) FROM stdin;
\.


--
-- Data for Name: pregnancy_affiliation; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pregnancy_affiliation (pregnancy_affiliation_id, pregnancy_focal_id, with_individual_id, initiate, over, "time", location) FROM stdin;
\.


--
-- Name: pregnancy_affiliation_pregnancy_affiliation_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pregnancy_affiliation_pregnancy_affiliation_id_seq', 1, false);


--
-- Data for Name: pregnancy_aggression; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pregnancy_aggression (pregnancy_aggression_id, pregnancy_focal_id, with_individual_id, initiate, level, over, win, "time", location) FROM stdin;
\.


--
-- Name: pregnancy_aggression_pregnancy_aggression_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pregnancy_aggression_pregnancy_aggression_id_seq', 1, false);


--
-- Data for Name: pregnancy_focal; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pregnancy_focal (pregnancy_focal_id, pack_history_id, depth, width, individuals, "time", location) FROM stdin;
\.


--
-- Name: pregnancy_focal_pregnancy_focal_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pregnancy_focal_pregnancy_focal_id_seq', 1, false);


--
-- Data for Name: pregnancy_nearest; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pregnancy_nearest (pregnancy_nearest_id, pregnancy_focal_id, nearest_individual_id, list_of_closest_individuals, scan_time) FROM stdin;
\.


--
-- Name: pregnancy_nearest_pregnancy_nearest_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pregnancy_nearest_pregnancy_nearest_id_seq', 1, false);


--
-- Name: pregnancy_pregnancy_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pregnancy_pregnancy_id_seq', 1, false);


--
-- Data for Name: provisioning_data; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY provisioning_data (provisioning_data_id, pack_history_id, litter_id, date, visit_time, egg_weight, comments) FROM stdin;
\.


--
-- Name: provisioning_data_provisioning_data_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('provisioning_data_provisioning_data_id_seq', 1, false);


--
-- Data for Name: pup_aggression; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pup_aggression (pup_aggression_id, pup_focal_id, with_individual_id, initiate, level, over, win, "time", location) FROM stdin;
\.


--
-- Name: pup_aggression_pup_aggression_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pup_aggression_pup_aggression_id_seq', 1, false);


--
-- Data for Name: pup_association; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pup_association (pup_association_id, pup_pack_history_id, escort_id, pack_composition_id, date, strength, confidence, location, comment, comment_editing) FROM stdin;
\.


--
-- Name: pup_association_pup_association_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pup_association_pup_association_id_seq', 1, false);


--
-- Data for Name: pup_care; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pup_care (pup_care_id, pup_focal_id, who_individual_id, type, "time", location) FROM stdin;
\.


--
-- Name: pup_care_pup_care_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pup_care_pup_care_id_seq', 1, false);


--
-- Data for Name: pup_feed; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pup_feed (pup_feed_id, pup_focal_id, who_individual_id, size, "time", location) FROM stdin;
\.


--
-- Name: pup_feed_pup_feed_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pup_feed_pup_feed_id_seq', 1, false);


--
-- Data for Name: pup_find; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pup_find (pup_find_id, pup_focal_id, size, "time", location) FROM stdin;
\.


--
-- Name: pup_find_pup_find_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pup_find_pup_find_id_seq', 1, false);


--
-- Data for Name: pup_focal; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pup_focal (pup_focal_id, pack_history_id, depth, width, individuals, "time", location) FROM stdin;
\.


--
-- Name: pup_focal_pup_focal_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pup_focal_pup_focal_id_seq', 1, false);


--
-- Data for Name: pup_nearest; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY pup_nearest (pup_nearest_id, pup_focal_id, nearest_individual_id, list_of_closest_individuals, scan_time) FROM stdin;
\.


--
-- Name: pup_nearest_pup_nearest_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('pup_nearest_pup_nearest_id_seq', 1, false);


--
-- Data for Name: radiocollar; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY radiocollar (radiocollar_id, frequency, weight, fitted, turned_on, removed, comment, date_entered, pack_history_id) FROM stdin;
\.


--
-- Name: radiocollar_radiocollar_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('radiocollar_radiocollar_id_seq', 1, false);


--
-- Data for Name: ultrasound; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY ultrasound (ultrasound_id, observation_date, foetus_number, foetus_id, foetus_size, cross_view_length, cross_view_width, long_view_length, long_view_width, observer, comment, pack_history_id) FROM stdin;
\.


--
-- Name: ultrasound_ultrasound_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('ultrasound_ultrasound_id_seq', 1, false);


--
-- Data for Name: weight; Type: TABLE DATA; Schema: mongoose; Owner: postgres
--

COPY weight (weight_id, weight, "time", accuracy, session, collar_weight, location, comment, pack_history_id, pack_composition_id) FROM stdin;
\.


--
-- Name: weight_weight_id_seq; Type: SEQUENCE SET; Schema: mongoose; Owner: postgres
--

SELECT pg_catalog.setval('weight_weight_id_seq', 1, false);


SET search_path = public, pg_catalog;

--
-- Data for Name: spatial_ref_sys; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY spatial_ref_sys (srid, auth_name, auth_srid, srtext, proj4text) FROM stdin;
\.


SET search_path = mongoose, pg_catalog;

--
-- Name: event_log EventLog_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY event_log
    ADD CONSTRAINT "EventLog_pk" PRIMARY KEY (event_log_id);


--
-- Name: litter UQ_Litter_name_unique; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter
    ADD CONSTRAINT "UQ_Litter_name_unique" UNIQUE (name);


--
-- Name: alarm_cause alarm_cause_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY alarm_cause
    ADD CONSTRAINT alarm_cause_pk PRIMARY KEY (alarm_cause_id);


--
-- Name: alarm alarm_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY alarm
    ADD CONSTRAINT alarm_pk PRIMARY KEY (alarm_id);


--
-- Name: anti_parasite anti_parasite_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY anti_parasite
    ADD CONSTRAINT anti_parasite_pk PRIMARY KEY (anti_parasite_id);


--
-- Name: babysitting babysitting_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY babysitting
    ADD CONSTRAINT babysitting_pk PRIMARY KEY (babysitting_id);


--
-- Name: blood_data blood_data_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY blood_data
    ADD CONSTRAINT blood_data_pk PRIMARY KEY (blood_data_id);


--
-- Name: capture capture_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY capture
    ADD CONSTRAINT capture_pk PRIMARY KEY (capture_id);


--
-- Name: dna_samples dna_samples_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY dna_samples
    ADD CONSTRAINT dna_samples_pk PRIMARY KEY (dna_samples_id);


--
-- Name: pack_event event_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_event
    ADD CONSTRAINT event_pk PRIMARY KEY (pack_event_id);


--
-- Name: group_composition group_composition_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY group_composition
    ADD CONSTRAINT group_composition_pk PRIMARY KEY (group_composition_id);


--
-- Name: hpa_sample hpa_sample_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY hpa_sample
    ADD CONSTRAINT hpa_sample_pk PRIMARY KEY (hpa_sample_id);


--
-- Name: individual_event_code individual_event_code_unique; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_event_code
    ADD CONSTRAINT individual_event_code_unique UNIQUE (code);


--
-- Name: individual individual_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual
    ADD CONSTRAINT individual_pk PRIMARY KEY (individual_id);


--
-- Name: individual_event individualevent_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_event
    ADD CONSTRAINT individualevent_pk PRIMARY KEY (individual_event_id);


--
-- Name: individual_event_code individualeventcode_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_event_code
    ADD CONSTRAINT individualeventcode_pk PRIMARY KEY (individual_event_code_id);


--
-- Name: interaction_outcome ineraction_outcome_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY interaction_outcome
    ADD CONSTRAINT ineraction_outcome_pk PRIMARY KEY (interaction_outcome_id);


--
-- Name: inter_group_interaction interaction_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY inter_group_interaction
    ADD CONSTRAINT interaction_pk PRIMARY KEY (inter_group_interaction_id);


--
-- Name: litter_event_code litter_code_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter_event_code
    ADD CONSTRAINT litter_code_pk PRIMARY KEY (litter_event_code_id);


--
-- Name: litter_event_code litter_code_unique; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter_event_code
    ADD CONSTRAINT litter_code_unique UNIQUE (code);


--
-- Name: litter_event litter_event_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter_event
    ADD CONSTRAINT litter_event_pk PRIMARY KEY (litter_event_id);


--
-- Name: litter litter_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter
    ADD CONSTRAINT litter_pk PRIMARY KEY (litter_id);


--
-- Name: mate_guard mate_guard_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY mate_guard
    ADD CONSTRAINT mate_guard_pk PRIMARY KEY (mate_guard_id);


--
-- Name: maternal_conditioning_litter maternal_conditioning_litter_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY maternal_conditioning_litter
    ADD CONSTRAINT maternal_conditioning_litter_pk PRIMARY KEY (maternal_conditioning_litter_id);


--
-- Name: maternel_conditioning_females maternel_conditioning_females_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY maternel_conditioning_females
    ADD CONSTRAINT maternel_conditioning_females_pk PRIMARY KEY (maternel_conditioning_females_id);


--
-- Name: meterology meterology_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY meterology
    ADD CONSTRAINT meterology_pk PRIMARY KEY (meterology_id);


--
-- Name: individual_name_history name_history_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_name_history
    ADD CONSTRAINT name_history_pk PRIMARY KEY (individual_name_history_id);


--
-- Name: pack name_unique; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack
    ADD CONSTRAINT name_unique UNIQUE (name);


--
-- Name: oestrus_affiliation oestrus_affiliation_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_affiliation
    ADD CONSTRAINT oestrus_affiliation_pk PRIMARY KEY (oestrus_affiliation_id);


--
-- Name: oestrus_aggression oestrus_aggression_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_aggression
    ADD CONSTRAINT oestrus_aggression_pk PRIMARY KEY (oestrus_aggression_id);


--
-- Name: oestrus_copulation_male oestrus_copulation_male_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_copulation_male
    ADD CONSTRAINT oestrus_copulation_male_pk PRIMARY KEY (oestrus_copulation_male_id);


--
-- Name: oestrus_event_code oestrus_event_code_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_event_code
    ADD CONSTRAINT oestrus_event_code_pk PRIMARY KEY (oestrus_event_code_id);


--
-- Name: oestrus_event oestrus_event_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_event
    ADD CONSTRAINT oestrus_event_pk PRIMARY KEY (oestrus_event_id);


--
-- Name: oestrus_focal oestrus_focal_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_focal
    ADD CONSTRAINT oestrus_focal_pk PRIMARY KEY (oestrus_focal_id);


--
-- Name: oestrus_male_aggression oestrus_male_aggression_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_male_aggression
    ADD CONSTRAINT oestrus_male_aggression_pk PRIMARY KEY (oestrus_male_aggression_id);


--
-- Name: oestrus_mating oestrus_mating_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_mating
    ADD CONSTRAINT oestrus_mating_pk PRIMARY KEY (oestrus_mating_id);


--
-- Name: oestrus_nearest oestrus_nearest_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_nearest
    ADD CONSTRAINT oestrus_nearest_pk PRIMARY KEY (oestrus_nearest_id);


--
-- Name: oestrus oestrus_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus
    ADD CONSTRAINT oestrus_pk PRIMARY KEY (oestrus_id);


--
-- Name: ox_shielding_feeding ox_shielding_feeding_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_feeding
    ADD CONSTRAINT ox_shielding_feeding_pk PRIMARY KEY (ox_shielding_feeding_id);


--
-- Name: ox_shielding_group ox_shielding_group_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_group
    ADD CONSTRAINT ox_shielding_group_pk PRIMARY KEY (ox_shielding_group_id);


--
-- Name: ox_shielding_male ox_shielding_male_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_male
    ADD CONSTRAINT ox_shielding_male_pk PRIMARY KEY (ox_shielding_male_id);


--
-- Name: pack_composition pack_composition_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_composition
    ADD CONSTRAINT pack_composition_pk PRIMARY KEY (pack_composition_id);


--
-- Name: pack_move_destination pack_move_destinations_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_move_destination
    ADD CONSTRAINT pack_move_destinations_pk PRIMARY KEY (pack_move_destination_id);


--
-- Name: pack_move pack_move_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_move
    ADD CONSTRAINT pack_move_pk PRIMARY KEY (pack_move_id);


--
-- Name: pack_name_history pack_name_history_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_name_history
    ADD CONSTRAINT pack_name_history_pk PRIMARY KEY (pack_name_history_id);


--
-- Name: pack pack_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack
    ADD CONSTRAINT pack_pk PRIMARY KEY (pack_id);


--
-- Name: pack_event_code packevnettypes_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_event_code
    ADD CONSTRAINT packevnettypes_pk PRIMARY KEY (pack_event_code_id);


--
-- Name: pack_history packhistory_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_history
    ADD CONSTRAINT packhistory_pk PRIMARY KEY (pack_history_id);


--
-- Name: poo_sample poo_sample_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY poo_sample
    ADD CONSTRAINT poo_sample_pk PRIMARY KEY (poo_sample_id);


--
-- Name: pregnancy_affiliation pregnancy_affiliation_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_affiliation
    ADD CONSTRAINT pregnancy_affiliation_pk PRIMARY KEY (pregnancy_affiliation_id);


--
-- Name: pregnancy_aggression pregnancy_aggression_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_aggression
    ADD CONSTRAINT pregnancy_aggression_pk PRIMARY KEY (pregnancy_aggression_id);


--
-- Name: pregnancy_focal pregnancy_focal_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_focal
    ADD CONSTRAINT pregnancy_focal_pk PRIMARY KEY (pregnancy_focal_id);


--
-- Name: pregnancy_nearest pregnancy_nearest_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_nearest
    ADD CONSTRAINT pregnancy_nearest_pk PRIMARY KEY (pregnancy_nearest_id);


--
-- Name: pregnancy pregnancy_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy
    ADD CONSTRAINT pregnancy_pk PRIMARY KEY (pregnancy_id);


--
-- Name: provisioning_data provisioning_data_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY provisioning_data
    ADD CONSTRAINT provisioning_data_pk PRIMARY KEY (provisioning_data_id);


--
-- Name: pup_aggression pup_aggression_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_aggression
    ADD CONSTRAINT pup_aggression_pk PRIMARY KEY (pup_aggression_id);


--
-- Name: pup_association pup_association_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_association
    ADD CONSTRAINT pup_association_pk PRIMARY KEY (pup_association_id);


--
-- Name: pup_care pup_care_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_care
    ADD CONSTRAINT pup_care_pk PRIMARY KEY (pup_care_id);


--
-- Name: pup_feed pup_feed_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_feed
    ADD CONSTRAINT pup_feed_pk PRIMARY KEY (pup_feed_id);


--
-- Name: pup_find pup_find_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_find
    ADD CONSTRAINT pup_find_pk PRIMARY KEY (pup_find_id);


--
-- Name: pup_focal pup_focal_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_focal
    ADD CONSTRAINT pup_focal_pk PRIMARY KEY (pup_focal_id);


--
-- Name: pup_nearest pup_nearest_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_nearest
    ADD CONSTRAINT pup_nearest_pk PRIMARY KEY (pup_nearest_id);


--
-- Name: radiocollar radiocollar_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY radiocollar
    ADD CONSTRAINT radiocollar_pk PRIMARY KEY (radiocollar_id);


--
-- Name: poo_sample sample_number_unique; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY poo_sample
    ADD CONSTRAINT sample_number_unique UNIQUE (sample_number);


--
-- Name: ultrasound ultrasound_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ultrasound
    ADD CONSTRAINT ultrasound_pk PRIMARY KEY (ultrasound_id);


--
-- Name: event_log uniqueMessageId; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY event_log
    ADD CONSTRAINT "uniqueMessageId" UNIQUE (message_id);


--
-- Name: individual unique_individual_unique_id; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual
    ADD CONSTRAINT unique_individual_unique_id UNIQUE (unique_id);


--
-- Name: individual unique_name; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual
    ADD CONSTRAINT unique_name UNIQUE (name);


--
-- Name: oestrus_event_code unique_oestrus_event_code; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_event_code
    ADD CONSTRAINT unique_oestrus_event_code UNIQUE (code);


--
-- Name: pack unique_pack_unique_id; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack
    ADD CONSTRAINT unique_pack_unique_id UNIQUE (unique_id);


--
-- Name: pack_event_code uq_code_is_unique; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_event_code
    ADD CONSTRAINT uq_code_is_unique UNIQUE (code);


--
-- Name: weight weight_pk; Type: CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY weight
    ADD CONSTRAINT weight_pk PRIMARY KEY (weight_id);


--
-- Name: alarm alarm_cause_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY alarm
    ADD CONSTRAINT alarm_cause_fk FOREIGN KEY (alarm_cause_id) REFERENCES alarm_cause(alarm_cause_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: individual_event individual_event_code_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_event
    ADD CONSTRAINT individual_event_code_fk FOREIGN KEY (individual_event_code_id) REFERENCES individual_event_code(individual_event_code_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pack_history individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_history
    ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: blood_data individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY blood_data
    ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: hpa_sample individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY hpa_sample
    ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_association individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_association
    ADD CONSTRAINT individual_fk FOREIGN KEY (escort_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: maternel_conditioning_females individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY maternel_conditioning_females
    ADD CONSTRAINT individual_fk FOREIGN KEY (paired_female_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: inter_group_interaction individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY inter_group_interaction
    ADD CONSTRAINT individual_fk FOREIGN KEY (leader_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: alarm individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY alarm
    ADD CONSTRAINT individual_fk FOREIGN KEY (caller_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pack_move individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_move
    ADD CONSTRAINT individual_fk FOREIGN KEY (leader_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_nearest individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_nearest
    ADD CONSTRAINT individual_fk FOREIGN KEY (nearest_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_mating individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_mating
    ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: oestrus_male_aggression individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_male_aggression
    ADD CONSTRAINT individual_fk FOREIGN KEY (initiator_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_affiliation individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_affiliation
    ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_aggression individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_aggression
    ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_care individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_care
    ADD CONSTRAINT individual_fk FOREIGN KEY (who_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_aggression individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_aggression
    ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_feed individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_feed
    ADD CONSTRAINT individual_fk FOREIGN KEY (who_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_nearest individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_nearest
    ADD CONSTRAINT individual_fk FOREIGN KEY (nearest_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pregnancy_affiliation individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_affiliation
    ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pregnancy_aggression individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_aggression
    ADD CONSTRAINT individual_fk FOREIGN KEY (with_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pregnancy_nearest individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_nearest
    ADD CONSTRAINT individual_fk FOREIGN KEY (nearest_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pregnancy individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy
    ADD CONSTRAINT individual_fk FOREIGN KEY (pregnant_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: mate_guard individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY mate_guard
    ADD CONSTRAINT individual_fk FOREIGN KEY (female_individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: individual_name_history individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_name_history
    ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: oestrus_copulation_male individual_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_copulation_male
    ADD CONSTRAINT individual_fk FOREIGN KEY (individual_id) REFERENCES individual(individual_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: inter_group_interaction interaction_outcome_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY inter_group_interaction
    ADD CONSTRAINT interaction_outcome_fk FOREIGN KEY (interaction_outcome_id) REFERENCES interaction_outcome(interaction_outcome_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: litter_event litter_event_code_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter_event
    ADD CONSTRAINT litter_event_code_fk FOREIGN KEY (litter_event_code_id) REFERENCES litter_event_code(litter_event_code_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: individual litter_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual
    ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id) REFERENCES litter(litter_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: litter_event litter_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter_event
    ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id) REFERENCES litter(litter_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: maternal_conditioning_litter litter_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY maternal_conditioning_litter
    ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id) REFERENCES litter(litter_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: babysitting litter_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY babysitting
    ADD CONSTRAINT litter_fk FOREIGN KEY (litter_id) REFERENCES litter(litter_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_event oestrus_event_code_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_event
    ADD CONSTRAINT oestrus_event_code_fk FOREIGN KEY (oestrus_event_code_id) REFERENCES oestrus_event_code(oestrus_event_code_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_copulation_male oestrus_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_copulation_male
    ADD CONSTRAINT oestrus_fk FOREIGN KEY (oestrus_id) REFERENCES oestrus(oestrus_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_nearest oestrus_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_nearest
    ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id) REFERENCES oestrus_focal(oestrus_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: oestrus_mating oestrus_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_mating
    ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id) REFERENCES oestrus_focal(oestrus_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: oestrus_male_aggression oestrus_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_male_aggression
    ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id) REFERENCES oestrus_focal(oestrus_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: oestrus_affiliation oestrus_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_affiliation
    ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id) REFERENCES oestrus_focal(oestrus_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: oestrus_aggression oestrus_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_aggression
    ADD CONSTRAINT oestrus_focal_fk FOREIGN KEY (oestrus_focal_id) REFERENCES oestrus_focal(oestrus_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: weight pack_composition_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY weight
    ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id) REFERENCES pack_composition(pack_composition_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_association pack_composition_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_association
    ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id) REFERENCES pack_composition(pack_composition_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: mate_guard pack_composition_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY mate_guard
    ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id) REFERENCES pack_composition(pack_composition_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pregnancy pack_composition_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy
    ADD CONSTRAINT pack_composition_fk FOREIGN KEY (pack_composition_id) REFERENCES pack_composition(pack_composition_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pack_event pack_event_code_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_event
    ADD CONSTRAINT pack_event_code_fk FOREIGN KEY (pack_event_code_id) REFERENCES pack_event_code(pack_event_code_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: litter pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY litter
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pack_history pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_history
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pack_event pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_event
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: group_composition pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY group_composition
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: alarm pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY alarm
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pack_move pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_move
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pack_composition pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_composition
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pack_name_history pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_name_history
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: inter_group_interaction pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY inter_group_interaction
    ADD CONSTRAINT pack_fk FOREIGN KEY (focalpack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_event pack_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_event
    ADD CONSTRAINT pack_fk FOREIGN KEY (pack_id) REFERENCES pack(pack_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: weight pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY weight
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: ultrasound pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ultrasound
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: radiocollar pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY radiocollar
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: individual_event pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY individual_event
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: capture pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY capture
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_association pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_association
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pup_pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: provisioning_data pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY provisioning_data
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: poo_sample pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY poo_sample
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: maternel_conditioning_females pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY maternel_conditioning_females
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: dna_samples pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY dna_samples
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: babysitting pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY babysitting
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (babysitter_pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: anti_parasite pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY anti_parasite
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: ox_shielding_feeding pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_feeding
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: ox_shielding_male pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_male
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: ox_shielding_group pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY ox_shielding_group
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: oestrus_focal pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY oestrus_focal
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pup_focal pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_focal
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pregnancy_focal pack_history_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_focal
    ADD CONSTRAINT pack_history_fk FOREIGN KEY (pack_history_id) REFERENCES pack_history(pack_history_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pack_move pack_move_destination_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pack_move
    ADD CONSTRAINT pack_move_destination_fk FOREIGN KEY (pack_move_destination_id) REFERENCES pack_move_destination(pack_move_destination_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: inter_group_interaction pack_secondary; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY inter_group_interaction
    ADD CONSTRAINT pack_secondary FOREIGN KEY (secondpack_id) REFERENCES pack(pack_id) MATCH FULL;


--
-- Name: pregnancy_affiliation pregnancy_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_affiliation
    ADD CONSTRAINT pregnancy_focal_fk FOREIGN KEY (pregnancy_focal_id) REFERENCES pregnancy_focal(pregnancy_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pregnancy_aggression pregnancy_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_aggression
    ADD CONSTRAINT pregnancy_focal_fk FOREIGN KEY (pregnancy_focal_id) REFERENCES pregnancy_focal(pregnancy_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pregnancy_nearest pregnancy_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pregnancy_nearest
    ADD CONSTRAINT pregnancy_focal_fk FOREIGN KEY (pregnancy_focal_id) REFERENCES pregnancy_focal(pregnancy_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pup_care pup_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_care
    ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id) REFERENCES pup_focal(pup_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pup_aggression pup_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_aggression
    ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id) REFERENCES pup_focal(pup_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pup_feed pup_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_feed
    ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id) REFERENCES pup_focal(pup_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: pup_find pup_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_find
    ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id) REFERENCES pup_focal(pup_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: pup_nearest pup_focal_fk; Type: FK CONSTRAINT; Schema: mongoose; Owner: postgres
--

ALTER TABLE ONLY pup_nearest
    ADD CONSTRAINT pup_focal_fk FOREIGN KEY (pup_focal_id) REFERENCES pup_focal(pup_focal_id) MATCH FULL ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

